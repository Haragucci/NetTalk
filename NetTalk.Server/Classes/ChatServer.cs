using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using NetTalk.Shared;

namespace NetTalk.Server
{
    class ChatServer
    {
        static void Main()
        {
            Console.Title = "NetTalk Server";
            ConsoleUI.DrawHeader();

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            TcpListener server = new TcpListener(IPAddress.Any, 5000);
            server.Start();
            Console.WriteLine($"[{timestamp}] [NetTalk] Server is now listening on port 5000");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                Thread thread = new Thread(() => ClientHandler.HandleClient(client));
                thread.Start();
            }
        } 
    }
}
