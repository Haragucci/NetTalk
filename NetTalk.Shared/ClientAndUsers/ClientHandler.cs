using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetTalk.Shared
{
    public static class ClientHandler
    {
        static HashSet<string> validRooms = new HashSet<string>();
        static List<ClientInfo> clients = new List<ClientInfo>();
        static Dictionary<string, User> users = UserStorage.LoadUsers();

        public static Dictionary<string, User> GetUsers()
        {
            return users;
        }

        public static List<ClientInfo> GetClients()
        {
            return clients;
        }

        public static string GetIdByUsername(string username)
        {
            users = UserStorage.LoadUsers();
            foreach (var u in users)
            {
                if (string.Equals(u.Value.Username, username, StringComparison.OrdinalIgnoreCase))
                {
                    return u.Value.Id;
                }
                else if (string.Equals(u.Value.Id, username, StringComparison.OrdinalIgnoreCase))
                {
                    return u.Value.Id;
                }
            }
            return null;
        }

        public static void HandleClient(TcpClient client)
        {
            string clientEndpoint = client.Client.RemoteEndPoint?.ToString() ?? "Unknown";
            Logger.Info($"Incoming connection from {clientEndpoint}");

            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string intro = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            string[] parts = intro.Split('|');


            if (parts.Length < 2)
            {
                Logger.Warn($"Malformed message from {clientEndpoint}: {intro}");
                return;
            }

            string command = parts[0].Trim().ToUpper();

            if (command == "REGISTER")
            {
                string username = parts[1].Trim();
                string password = parts[2].Trim();

                Logger.Auth($"Register attempt by user '{username}' from {clientEndpoint}");

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    Logger.Error($"Empty username or password in registration from {clientEndpoint}");
                    Send(stream, "[SERVER] Username or Password empty!");
                    client.Close();
                    return;
                }

                var users = UserStorage.LoadUsers();

                if (users.Any(u => u.Value.Username == username))
                {
                    Logger.Warn($"Registration failed: Username '{username}' already exists.");
                    Send(stream, "[SERVER] Username already exists!");
                    client.Close();
                    return;
                }

                string id = Guid.NewGuid().ToString();
                users[id] = new User { Id = id, Username = username, PasswordHash = PasswordUtils.HashPassword(password) };
                UserStorage.SaveUsers(users);

                Logger.Auth($"User '{username}' registered successfully.");
                Send(stream, "[SERVER] Signing up succeeded");
                return;
            }
            else if (command == "LOGIN")
            {
                string username = parts[1].Trim();
                string password = parts[2].Trim();
                string roomId = parts.Length >= 4 ? parts[3].Trim() : "";
                string flag = parts.Length >= 5 ? parts[4].Trim().ToUpper() : "";
                string roomName = parts.Length >= 6 ? parts[5].Trim() : "";

                var users = UserStorage.LoadUsers();
                var userEntry = users.FirstOrDefault(u => u.Value.Username == username);

                if (userEntry.Value == null || userEntry.Value.PasswordHash != PasswordUtils.HashPassword(password))
                {
                    Send(stream, "[SERVER] Login failed!");
                    client.Close();
                    return;
                }

                bool isTestConnection = flag == "TEST";
                bool isCreatingRoom = flag == "TRUE";

                var rooms = RoomStorage.LoadRooms();
                var roomExists = rooms.Any(r => r.RoomId == roomId);

                if (isTestConnection)
                {
                    if (userEntry.Value != null && userEntry.Value.PasswordHash == PasswordUtils.HashPassword(password))
                    {
                        Send(stream, "[SERVER] Login succeeded");
                    }
                    else
                    {
                        Send(stream, "[SERVER] Login failed!");
                    }
                    return;
                }

                string id = GetIdByUsername(username);

                if (isCreatingRoom)
                {
                    if (roomExists)
                    {
                        Send(stream, "[SERVER] Room already exists!");
                        client.Close();
                        return;
                    }

                    rooms.Add(new Room
                    {
                        RoomId = roomId,
                        Name = roomName,
                        Creator = id,
                        Operators = new List<string> { id },
                        BannedUsers = new List<string>(),
                        JoinedUsers = new List<string>()
                    });
                    RoomStorage.SaveRooms(rooms);

                    Send(stream, "[SERVER] Room created!");
                    roomExists = true;
                }

                if (!roomExists)
                {
                    Send(stream, "[SERVER] Room does not exist!");
                    client.Close();
                    return;
                }

                var room = rooms.FirstOrDefault(r => r.RoomId == roomId);

                if (room.BannedUsers != null && room.BannedUsers.Contains(id))
                {
                    Send(stream, "[SERVER] You are banned from this room.");
                    client.Close();
                    return;
                }

                var clientInfo = new ClientInfo(client, username, roomId);

                lock (clients)
                {
                    clients.Add(clientInfo);
                }
                string JoinedUserId = GetIdByUsername(username);
                if (JoinedUserId != null && !room.Exists(JoinedUserId))
                {
                    room.JoinedUsers.Add(JoinedUserId);
                    RoomStorage.SaveRooms(rooms);
                }
                if (!isCreatingRoom)
                {
                    RoomManager.BroadcastToRoom(roomId, $"[JOIN] {username} has joined the Room", ConsoleColor.Yellow, null);
                }
                Send(stream, "[JOIN] You have joined the room!");

                try
                {
                    while (true)
                    {
                        int bytes = stream.Read(buffer, 0, buffer.Length);
                        if (bytes == 0) break;

                        string[] messages = Encoding.UTF8.GetString(buffer, 0, bytes).Split('\n');
                        foreach (string message in messages)
                        {
                            if (string.IsNullOrWhiteSpace(message)) continue;

                            string rawMessage = message.Trim();
                            int colonIndex = rawMessage.IndexOf(':');
                            if (colonIndex == -1) continue;

                            string senderName = rawMessage.Substring(0, colonIndex).Trim();
                            string messageText = rawMessage.Substring(colonIndex + 1).Trim();

                            room = RoomStorage.LoadRooms().FirstOrDefault(r => r.RoomId == roomId);
                            string tag = "";
                            if (room != null)
                            {
                                string userId = ClientHandler.GetIdByUsername(senderName);
                                if (room.Creator == userId)
                                {
                                    tag = " [OWNER]";
                                }
                                else if (room.Operators.Contains(userId))
                                {
                                    tag = " [OP]";
                                }
                            }

                            string fullMessage = $"[{DateTime.Now:HH:mm}]{tag} {senderName}: {messageText}\n";

                            if (messageText.StartsWith("/"))
                            {
                                CommandHandler.HandleCommand(messageText, senderName, roomId, stream);
                                continue;
                            }
                            ConsoleColor messageColor = ConsoleColor.Gray;

                            if (fullMessage.Contains("[JOIN]") || fullMessage.Contains("[LEAVE]"))
                                messageColor = ConsoleColor.DarkYellow;
                            else if (fullMessage.Contains("[PROMOTE]") || fullMessage.Contains("[DEMOTE]"))
                                messageColor = ConsoleColor.Cyan;
                            else if (fullMessage.Contains("[SERVER]"))
                                messageColor = ConsoleColor.Red;
                            else if (tag.Contains("[OWNER]"))
                                messageColor = ConsoleColor.DarkRed;
                            else if (tag.Contains("[OP]"))
                                messageColor = ConsoleColor.Blue;

                            RoomManager.BroadcastToRoom(roomId, fullMessage, messageColor, client);

                        }
                    }
                }
                catch { }

                lock (clients)
                {
                    clients.Remove(clientInfo);
                }

                RoomManager.BroadcastToRoom(roomId, $"[LEAVE] {username} left the Room", ConsoleColor.DarkGray, null);
                client.Close();
            }
            else if (command == "GET_CREDENTIALS")
            {
                if (parts.Length < 2)
                {
                    Send(stream, "[SERVER] Malformed message: Missing username");
                    client.Close();
                    return;
                }

                string requestedUsername = parts[1].Trim();

                var users = UserStorage.LoadUsers();
                var userEntry = users.FirstOrDefault(u => u.Value.Username.Equals(requestedUsername, StringComparison.OrdinalIgnoreCase));

                if (userEntry.Value == null)
                {
                    Send(stream, "[SERVER] Username not registered!");
                    client.Close();
                    return;
                }

                string username = userEntry.Value.Username;
                string passwordHash = userEntry.Value.PasswordHash;

                Send(stream, $"CREDENTIALS|{username}|{passwordHash}");
            }
            else if (command == "LIST_ROOMS")
            {
                parts = intro.Split('|');
                if (parts.Length >= 3)
                {
                    string username = parts[1].Trim();
                    string password = parts[2].Trim();

                    var users = UserStorage.LoadUsers();
                    var userEntry = users.FirstOrDefault(u => u.Value.Username == username);

                    if (userEntry.Value == null || userEntry.Value.PasswordHash != PasswordUtils.HashPassword(password))
                    {
                        Send(stream, "[SERVER] Login failed!");
                        client.Close();
                        return;
                    }

                    string userId = userEntry.Key;

                    var allRooms = RoomStorage.LoadRooms();

                    var createdRooms = allRooms
                        .Where(r => r.Creator == userId)
                        .Select(r => $"{r.RoomId} - {r.Name} - (created)")
                        .ToList();

                    var joinedRooms = allRooms
                        .Where(r => r.Exists(userId) && r.Creator != userId)
                        .Select(r => $"{r.RoomId} - {r.Name} - (joined)")
                        .ToList();

                    var combinedList = createdRooms.Concat(joinedRooms).ToList();

                    string response = combinedList.Count > 0
                        ? string.Join("\n", combinedList)
                        : "You have not created or joined any rooms.";

                    byte[] data = Encoding.UTF8.GetBytes(response);
                    stream.Write(data, 0, data.Length);
                }

                client.Close();
                return;
            }
            else if (command.StartsWith("DELETEALL"))
            {
                string requestUser = parts.Length > 1 ? parts[1] : "";

                if (requestUser != "admin")
                {
                    string response = "Access denied!";
                    byte[] data = Encoding.UTF8.GetBytes(response);
                    stream.Write(data, 0, data.Length);
                    client.Close();
                    return;
                }

                try
                {
                    RoomStorage.DelEveryRoom();
                    string response = "All data was deleted!";
                    byte[] data = Encoding.UTF8.GetBytes(response);
                    stream.Write(data, 0, data.Length);
                }
                catch (Exception e)
                {
                    string response = "Server error: " + e.Message;
                    byte[] data = Encoding.UTF8.GetBytes(response);
                    stream.Write(data, 0, data.Length);
                }

                client.Close();
                return;
            }
        }


        public static void Send(NetworkStream stream, string message)
        {
            string timestamped = FormatServerMessage(message);
            byte[] data = Encoding.UTF8.GetBytes(timestamped);
            stream.Write(data, 0, data.Length);
        }

        private static string FormatServerMessage(string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            if (message.StartsWith("[SERVER]") || message.StartsWith("[JOIN]") || message.StartsWith("[LEAVE]") ||
                message.StartsWith("[PROMOTE]") || message.StartsWith("[DEMOTE]") || message.StartsWith("[KICK]") ||
                message.StartsWith("[MUTE]") || message.StartsWith("[UNMUTE]"))
            {
                return $"[{timestamp}] {message}";
            }
            return message;
        }

    }
}
