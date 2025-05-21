using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NetTalk.Client.Classes.UI;
using NetTalk.Client.Classes.Logging;
using NetTalk.Client.Classes.InputManagement;

namespace NetTalk.Client
{
    class ChatClient
    {
        static string ReadLineBuffer = "";
        static volatile bool isRunning = true;
        static string ServerAddress = "172.16.2.206";


        static void Main()
        {
            Console.Title = "NetTalk Client";
            while (true)
            {
                bool logout = false;
                string name = "";
                string password = "";
                bool authenticated = false;

                while (!authenticated)
                {
                    Console.Clear();
                    UI.DrawWelcomeHeader();
                    string option = Console.ReadLine();

                    Console.Write("Username: ");
                    name = Console.ReadLine()?.Trim() ?? "";
                    Console.Write("Password: ");
                    password = AuthHandler.ReadPassword();

                    if (option == "1")
                    {
                        if (name == "" || password == "")
                        {
                            Logger.PrintLog("INFO", "Username and password can not be empty!");
                            Console.ReadKey();
                        }
                        if (AuthHandler.Register(name, password))
                        {
                            Logger.PrintLog("INFO", "Signing up succeeded! Logging in now...", ConsoleColor.Green);
                            AuthHandler.LoginTest(name, password);
                            authenticated = true;
                            Thread.Sleep(300);
                            Console.Clear();
                        }
                        else
                        {
                            Logger.PrintLog("ERROR", "Signing up failed! Try again!", ConsoleColor.Red);
                            Console.ReadLine();
                            Console.Clear();
                        }
                    }
                    else if (option == "2")
                    {
                        if (AuthHandler.LoginTest(name, password))
                        {
                            Logger.PrintLog("INFO", "Login succeeded", ConsoleColor.Green);
                            Console.ReadLine();
                            Console.Clear();
                            authenticated = true;
                        }
                        else
                        {
                            Logger.PrintLog("ERROR", "Login failed! Try again!", ConsoleColor.Red);
                            Console.ReadLine();
                            Console.Clear();
                        }
                    }
                    else
                    {
                        Console.WriteLine("Wrong Input ... Try again!");
                        Console.ReadLine();
                        Console.Clear();
                    }
                }

                while (!logout)
                {
                    string roomName = "";
                    string roomId = "";
                    bool isCreatingRoom = false;
                    bool connected = false;

                    while (!connected)
                    {
                        Console.Clear();
                        roomId = "";
                        isCreatingRoom = false;
                        UI.DrawLoginHeader(name);
                        string roomOption = Console.ReadLine();

                        if (roomOption == "1")
                        {
                            Console.Write("Please Enter a name for your Room: ");
                            roomName = Console.ReadLine();
                            roomId = AuthHandler.GenerateRoomId();
                            isCreatingRoom = true;
                            Logger.PrintLog("ROOM CREATED", $"Your room has been created: {roomId}", ConsoleColor.Yellow);
                            connected = true;
                        }
                        else if (roomOption == "2")
                        {
                            try
                            {
                                TcpClient listClient = new TcpClient(ServerAddress, 5000);
                                NetworkStream listStream = listClient.GetStream();
                                string listRequest = $"LIST_ROOMS|{name}|{password}";

                                string roomListResponse = ServerRequest(listRequest, listClient, listStream);
                                listClient.Close();


                                if (roomListResponse.StartsWith("You"))
                                {
                                    Console.WriteLine("You have no created or joined rooms!");
                                }
                                else
                                {
                                    Console.WriteLine("\nAvailable rooms:\n");

                                    var rooms = roomListResponse.Split('\n');
                                    foreach (var room in rooms)
                                    {
                                        Console.WriteLine($"- {room.Trim()}");
                                    }

                                    var availableRooms = rooms
                                     .Select(r => r.Trim())
                                     .Where(r => !string.IsNullOrWhiteSpace(r))
                                     .Select(r => r.Split(new[] { " - " }, StringSplitOptions.None)[0].Trim())
                                     .ToList();


                                    roomId = GetRoomIdWithTabSupport(availableRooms);
                                }
                            }
                            catch
                            {
                                Logger.PrintLog("ERROR", "Failed to retrieve rooms.\nPress ENTER to return to the menu...", ConsoleColor.Red);
                                Console.ReadLine();
                                return;
                            }

                            try
                            {
                                TcpClient testClient = new TcpClient(ServerAddress, 5000);
                                NetworkStream testStream = testClient.GetStream();
                                string introTest = $"LOGIN|{name}|{password}|{roomId}|TEST";

                                string response = ServerRequest(introTest, testClient, testStream);

                                testClient.Close();

                                if (response.Contains("[SERVER] Room does not exist!"))
                                {
                                    Logger.PrintLog("ERROR", "Wrong Room-ID.\nPress ENTER to return to the menu...", ConsoleColor.Red);
                                    Console.ReadLine();
                                }
                                else if (response.Contains("[SERVER] You are banned from this room"))
                                {
                                    Logger.PrintLog("ERROR", "You are banned from this room.\nPress ENTER to return to the menu...", ConsoleColor.Red);
                                    Console.ReadLine();
                                }
                                else
                                {
                                    connected = true;
                                }
                            }
                            catch
                            {
                                Logger.PrintLog("ERROR", "Connection to the server failed!\nPress ENTER to return to the menu...", ConsoleColor.Red);
                                Console.ReadLine();
                            }
                        }
                        else if (roomOption == "3")
                        {
                            authenticated = false;
                            name = "";
                            password = "";
                            Console.WriteLine("Logout Succed! Press any key to continue ...");
                            Console.ReadKey();
                            connected = false;
                            logout = true;
                            break;
                        }
                        else if (roomOption == "4")
                        {
                            Console.WriteLine("Press any key to exit ...");
                            Console.ReadKey();
                            Environment.Exit(0);
                        }
                        else if (roomOption == "/delall" && name == "admin")
                        {
                            Console.WriteLine("Are you sure u want to delete Everything? Enter [yes] or press enter to Cancel!");
                            string input = Console.ReadLine();
                            if (input != "yes")
                            {
                                Console.WriteLine("Canceled! Press any key to exit ...");
                                Console.ReadKey();
                                break;
                            }

                            Console.WriteLine("All Data will be Deleted Soon!");
                            try
                            {
                                TcpClient testClient = new TcpClient(ServerAddress, 5000);
                                NetworkStream testStream = testClient.GetStream();
                                string introTest = $"DELETEALL|{name}";
                                string response = ServerRequest(introTest, testClient, testStream);

                                if (response.Contains("All data was deleted!"))
                                {
                                    Console.WriteLine("All data have been deleted!");
                                    Console.ReadKey();
                                    authenticated = false;
                                    name = "";
                                    password = "";
                                    connected = false;
                                    logout = true;
                                    break;
                                }
                                else
                                {
                                    Console.WriteLine($"Server response: {response}");
                                    Console.ReadKey();
                                }

                                testClient.Close();
                            }
                            catch
                            {
                                Console.WriteLine("Something went wrong!");
                                Console.ReadKey();
                            }
                        }
                    }

                    if (!logout)
                    {
                        TcpClient client = new TcpClient(ServerAddress, 5000);
                        NetworkStream stream = client.GetStream();

                        string intro = $"LOGIN|{name}|{password}|{roomId}|{isCreatingRoom.ToString().ToUpper()}|{roomName}";
                        string introResponse = ServerRequest(intro, client, stream);

                        if (introResponse.Contains("[SERVER] Room does not exist!") ||
                            introResponse.Contains("[SERVER] You are banned from this room"))
                        {
                            Logger.PrintLog("ERROR", introResponse.Trim() + "\nPress ENTER to return to the menu...", ConsoleColor.Red);
                            stream.Close();
                            client.Close();
                            Console.ReadLine();
                            Console.Clear();
                            continue;
                        }

                        Console.Clear();
                        UI.DrawHeader(roomId, name);

                        isRunning = true;
                        bool shouldReconnect = false;

                        Thread thread = new Thread(() =>
                        {
                            byte[] buffer = new byte[1024];
                            while (isRunning)
                            {
                                try
                                {
                                    int bytes = stream.Read(buffer, 0, buffer.Length);
                                    if (bytes == 0) break;

                                    string rawMessage = Encoding.UTF8.GetString(buffer, 0, bytes).Trim();
                                    string[] messages = rawMessage.Split('\n');

                                    foreach (string msg in messages)
                                    {
                                        if (string.IsNullOrWhiteSpace(msg)) continue;

                                        string message = msg.Trim();
                                        ConsoleColor color = ConsoleColor.Gray;

                                        if (message.StartsWith("[COLOR="))
                                        {
                                            int endIndex = message.IndexOf(']');
                                            if (endIndex > 7)
                                            {
                                                string colorCode = message.Substring(7, endIndex - 7);
                                                if (Enum.TryParse(colorCode, true, out ConsoleColor parsedColor))
                                                {
                                                    color = parsedColor;
                                                }

                                                message = message.Substring(endIndex + 1);
                                            }
                                        }

                                        if (message.Contains("[SERVER] You have been kicked from the room") ||
                                            message.Contains("[SERVER] You have been banned from the room"))
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("\n[DISCONNECT] " + message);
                                            Console.ResetColor();
                                            isRunning = false;
                                            shouldReconnect = true;
                                            break;
                                        }
                                        else if (message.Contains("[SERVER] You left the room"))
                                        {
                                            Console.ForegroundColor = ConsoleColor.Yellow;
                                            Console.WriteLine("\n[INFO] You left the room. Press ENTER to return to the menu...");
                                            Console.ResetColor();
                                            isRunning = false;
                                            shouldReconnect = true;
                                            break;
                                        }

                                        int cursorTop = Console.CursorTop;
                                        Console.SetCursorPosition(0, cursorTop);
                                        Console.Write(new string(' ', Console.WindowWidth));
                                        Console.SetCursorPosition(0, cursorTop);

                                        Console.ForegroundColor = color;
                                        Console.WriteLine(message);
                                        Console.ResetColor();

                                        Input.RedrawInputLine(ReadLineBuffer);
                                    }
                                }
                                catch
                                {
                                    break;
                                }
                            }
                        });
                        thread.Start();

                        InputLoop(stream, name, roomId);
                        thread.Join();

                        if (shouldReconnect)
                        {
                            Console.ReadLine();
                            Console.Clear();
                            continue;
                        }
                    }
                }
            }
        }

        public static string ServerRequest(string intro, TcpClient client, NetworkStream stream)
        {
            byte[] introBytes = Encoding.UTF8.GetBytes(intro);
            stream.Write(introBytes, 0, introBytes.Length);

            byte[] bufferIntro = new byte[1024];
            int introBytesRead = stream.Read(bufferIntro, 0, bufferIntro.Length);
            string introResponse = Encoding.UTF8.GetString(bufferIntro, 0, introBytesRead);

            return introResponse;
        }

        public static string GetRoomIdWithTabSupport(List<string> availableRooms)
        {
            int currentIndex = 0;
            string currentInput = "";
            ConsoleKeyInfo key;

            Console.Write("\nEnter Room-ID (press TAB to cycle through rooms): ");

            while (true)
            {
                key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return currentInput.Trim().ToUpper();
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (currentInput.Length > 0)
                    {
                        currentInput = currentInput.Substring(0, currentInput.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else if (key.Key == ConsoleKey.Tab)
                {
                    if (availableRooms.Count > 0)
                    {
                        currentInput = availableRooms[currentIndex % availableRooms.Count];
                        currentIndex++;

                        Console.Write("\rEnter Room-ID (press TAB to cycle through rooms): ".PadRight(Console.WindowWidth));
                        Console.Write($"\rEnter Room-ID (press TAB to cycle through rooms): {currentInput}");
                    }
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    currentInput += key.KeyChar;
                    Console.Write(key.KeyChar);
                }
            }
        }

        static void InputLoop(NetworkStream stream, string name, string roomId)
        {
            StringBuilder inputBuffer = new StringBuilder();
            while (isRunning)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

                if (keyInfo.Key == ConsoleKey.Backspace && inputBuffer.Length > 0)
                {
                    inputBuffer.Length--;
                    Input.RedrawInputLine(inputBuffer.ToString());
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    string message = inputBuffer.ToString().Trim();
                    inputBuffer.Clear();
                    Input.RedrawInputLine("");

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        string rawMessage = $"{name}:{message}\n";
                        byte[] data = Encoding.UTF8.GetBytes(rawMessage);
                        stream.Write(data, 0, data.Length);
                    }

                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    inputBuffer.Append(keyInfo.KeyChar);
                    Input.RedrawInputLine(inputBuffer.ToString());
                }

                ReadLineBuffer = inputBuffer.ToString();
            }
        }
    }
}
