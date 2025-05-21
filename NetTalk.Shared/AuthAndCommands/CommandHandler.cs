using NetTalk.Shared;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetTalk.Shared
{
    public static class CommandHandler
    {
        public static void HandleCommand(string message, string username, string roomId, NetworkStream stream)
        {
            var parts = message.Split(' ');

            if (parts[0] == "/leave")
            {
                LeaveRoom(username, roomId, stream);
            }
            else if (parts[0] == "/kick")
            {
                if (parts.Length > 1)
                {
                    KickUser(parts[1], roomId, username, stream);
                }
            }
            else if (parts[0] == "/ban")
            {
                if (parts.Length > 1)
                {
                    BanUser(parts[1], roomId, username, stream);
                }
            }
            else if (parts[0] == "/op")
            {
                if (parts.Length > 1)
                {
                    PromoteToOperator(parts[1], roomId, username, stream);
                }
            }
            else if (parts[0] == "/unban")
            {
                if (parts.Length > 1)
                {
                    UnbanUser(parts[1], roomId, username, stream);
                }
            }
            else if (parts[0] == "/deop")
            {
                if (parts.Length > 1)
                {
                    DemoteToUser(parts[1], roomId, username, stream);
                }
            }
            else if (parts[0] == "/active")
            {
                ShowActiveClients(roomId, username, stream);
            }
            else if (parts[0] == "/rename")
            {
                RenameChat(parts[1], roomId, username, stream);
            }
            else
            {
                CommandNotFound(stream);
            }
        }

        private static void RenameChat(string v, string roomId, string admin, NetworkStream stream)
        {
            string adminId = "";
            try
            {
                adminId = ClientHandler.GetIdByUsername(admin);
            }
            catch (Exception e)
            {
                ClientHandler.Send(stream, "[SERVER] " + e.Message);
                return;
            }
            var rooms = RoomStorage.LoadRooms();
            var room = rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room == null || room.Creator != adminId)
            {
                ClientHandler.Send(stream, "[SERVER] Only the room creator can promote users to operators!");
                return;
            }
            else
            {
                try
                {
                    room.Name = v;
                    RoomStorage.SaveRooms(rooms);
                    ClientHandler.Send(stream, "[SERVER] The Talk has been renamed succesfully");
                }
                catch (Exception e)
                {
                    ClientHandler.Send(stream, "[SERVER] Error: " + e.Message);
                }
            }
        }

        public static void ShowActiveClients(string roomId, string username, NetworkStream stream)
        {
            var clients = ClientHandler.GetClients();

            lock (clients)
            {
                var client = clients.FirstOrDefault(c => c.Name == username && c.RoomId == roomId);
                if (client != null)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("[SERVER] Active users: ");
                    foreach (var c in clients.Where(c => c.RoomId == roomId))
                    {
                        sb.AppendLine($"- {c.Name}");
                    }

                    byte[] data = Encoding.UTF8.GetBytes(sb.ToString());
                    stream.Write(data, 0, data.Length);
                }
            }
        }


        public static void LeaveRoom(string username, string roomId, NetworkStream stream)
        {
            var clients = ClientHandler.GetClients();

            lock (clients)
            {
                var client = clients.FirstOrDefault(c => c.Name == username && c.RoomId == roomId);
                if (client != null)
                {
                    clients.Remove(client);
                    ClientHandler.Send(stream, "[SERVER] You left the room");
                    client.Client.Close();
                }
            }
        }

        public static void CommandNotFound(NetworkStream stream)
        {
            ClientHandler.Send(stream, "[SERVER] Command not found!");
        }


        public static void KickUser(string target, string roomId, string admin, NetworkStream stream)
        {
            string id = "";
            string adminId = "";
            try
            {
                id = ClientHandler.GetIdByUsername(target);
                adminId = ClientHandler.GetIdByUsername(admin);
            }
            catch (Exception e)
            {
                ClientHandler.Send(stream, "[SERVER] " + e.Message);
                return;
            }
            var room = RoomStorage.LoadRooms().FirstOrDefault(r => r.RoomId == roomId);
            if (room == null || (!room.Operators.Contains(adminId) && room.Creator != adminId))
            {
                ClientHandler.Send(stream, "[SERVER] You are not authorized to kick users!");
                return;
            }
            if (id == adminId)
            {
                ClientHandler.Send(stream, "[SERVER] You can not kick urself!");
                return;
            }
            if (room.Creator == id)
            {
                ClientHandler.Send(stream, "[SERVER] You are not authorized to kick the Creater!");
                return;
            }
            var clients = ClientHandler.GetClients();
            var clientToKick = clients.FirstOrDefault(c => c.Name == target && c.RoomId == roomId);
            if (clientToKick != null)
            {
                clients.Remove(clientToKick);
                ClientHandler.Send(clientToKick.Client.GetStream(), "[SERVER] You have been kicked from the room");
                RoomManager.BroadcastToRoom(roomId, $"[KICK] {target} was kicked by {admin}", ConsoleColor.Red, null);
                clientToKick.Client.Close();
            }
            else
            {
                ClientHandler.Send(stream, "[SERVER] User not found in the room!");
            }
        }

        public static void BanUser(string target, string roomId, string admin, NetworkStream stream)
        {
            string id = "";
            string adminId = "";
            try
            {
                id = ClientHandler.GetIdByUsername(target);
                adminId = ClientHandler.GetIdByUsername(admin);
            }
            catch (Exception e)
            {
                ClientHandler.Send(stream, "[SERVER] " + e.Message);
                return;
            }
            var rooms = RoomStorage.LoadRooms();
            var room = rooms.FirstOrDefault(r => r.RoomId == roomId);

            if (room == null)
            {
                ClientHandler.Send(stream, "[SERVER] Room not found!");
                return;
            }

            if (!room.Operators.Contains(adminId) && room.Creator != adminId)
            {
                ClientHandler.Send(stream, "[SERVER] You are not authorized to ban users!");
                return;
            }

            if (id == room.Creator)
            {
                ClientHandler.Send(stream, "[SERVER] You are not authorized to ban the Creator!");
                return;
            }

            if (id == adminId || id == adminId)
            {
                ClientHandler.Send(stream, "[SERVER] You can not ban urself!");
                return;
            }

            if (room.BannedUsers == null)
                room.BannedUsers = new List<string>();

            if (room.BannedUsers.Contains(id))
            {
                ClientHandler.Send(stream, "[SERVER] User is already banned.");
                return;
            }

            room.BannedUsers.Add(id);
            RoomStorage.SaveRooms(rooms);

            var clients = ClientHandler.GetClients();
            var clientToBan = clients.FirstOrDefault(c => c.Name == target && c.RoomId == roomId);
            if (clientToBan != null)
            {
                clients.Remove(clientToBan);
                ClientHandler.Send(clientToBan.Client.GetStream(), "[SERVER] You have been banned from the room");
                RoomManager.BroadcastToRoom(roomId, $"[BAN] {target} was banned by {admin}", ConsoleColor.Red, null);
                clientToBan.Client.Close();
            }
            else
            {
                RoomManager.BroadcastToRoom(roomId, $"[BAN] {target} was banned by {admin}", ConsoleColor.Red, null);
                ClientHandler.Send(stream, "[SERVER] User was not currently online but is now banned.");
            }
        }


        public static void UnbanUser(string target, string roomId, string admin, NetworkStream stream)
        {
            string id = "";
            string adminId = "";
            try
            {
                id = ClientHandler.GetIdByUsername(target);
                adminId = ClientHandler.GetIdByUsername(admin);
            }
            catch (Exception e)
            {
                ClientHandler.Send(stream, "[SERVER] " + e.Message);
                return;
            }
            var rooms = RoomStorage.LoadRooms();
            var room = rooms.FirstOrDefault(r => r.RoomId == roomId);

            if (room == null)
            {
                ClientHandler.Send(stream, "[SERVER] Room not found!");
                return;
            }

            if (!room.Operators.Contains(adminId) && room.Creator != adminId)
            {
                ClientHandler.Send(stream, "[SERVER] You are not authorized to unban users!");
                return;
            }

            if (id == room.Creator)
            {
                ClientHandler.Send(stream, "[SERVER] You are not authorized to unban the Creator!");
                return;
            }

            if (room.BannedUsers == null)
                room.BannedUsers = new List<string>();

            if (room.BannedUsers.Contains(id))
            {
                room.BannedUsers.Remove(id);
                RoomStorage.SaveRooms(rooms);
                RoomManager.BroadcastToRoom(roomId, $"[UNBAN] {target} was unbanned by {admin}", ConsoleColor.Red, null);
                return;
            }
        }


        public static void PromoteToOperator(string target, string roomId, string admin, NetworkStream stream)
        {
            string id = "";
            string adminId = "";
            try
            {
                id = ClientHandler.GetIdByUsername(target);
                adminId = ClientHandler.GetIdByUsername(admin);
            }
            catch (Exception e)
            {
                ClientHandler.Send(stream, "[SERVER] " + e.Message);
                return;
            }
            var rooms = RoomStorage.LoadRooms();
            var room = rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room == null || room.Creator != adminId)
            {
                ClientHandler.Send(stream, "[SERVER] Only the room creator can promote users to operators!");
                return;
            }
            bool found = true;
            if (id == null || id == "")
            {
                ClientHandler.Send(stream, "[SERVER] User dont Exist!");
                found = false;
            }

            if (!room.Operators.Contains(id) && found)
            {
                room.Operators.Add(id);
                RoomStorage.SaveRooms(rooms);
                RoomManager.BroadcastToRoom(roomId, $"[PROMOTE] {target} was promoted to operator by {admin}", ConsoleColor.Green, null);
            }
            else
            {
                if (found)
                {
                    ClientHandler.Send(stream, "[SERVER] User is already an operator");
                }
            }
        }

        public static void DemoteToUser(string target, string roomId, string admin, NetworkStream stream)
        {
            string id = "";
            string adminId = "";
            try
            {
                id = ClientHandler.GetIdByUsername(target);
                adminId = ClientHandler.GetIdByUsername(admin);
            }
            catch (Exception e)
            {
                ClientHandler.Send(stream, "[SERVER] " + e.Message);
                return;
            }
            var rooms = RoomStorage.LoadRooms();
            var room = rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room == null || room.Creator != adminId)
            {
                ClientHandler.Send(stream, "[SERVER] Only the room creator can demote users!");
                return;
            }

            if (room.Operators.Contains(id))
            {
                room.Operators.Remove(id);
                RoomStorage.SaveRooms(rooms);
                RoomManager.BroadcastToRoom(roomId, $"[DEMOTE] {target} was demotet to user by {admin}", ConsoleColor.Green, null);
            }
            else
            {
                ClientHandler.Send(stream, "[SERVER] User is not an Operator!");
            }
        }

    }
}
