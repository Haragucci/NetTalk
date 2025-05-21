using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetTalk.Shared
{
    public static class RoomManager
    {
        public static void BroadcastToRoom(string roomId, string message, ConsoleColor color, TcpClient sender)
        {
            List<ClientInfo> clients = ClientHandler.GetClients();

            string colorPrefix = $"[COLOR={color}]";
            string fullMessage = $"{colorPrefix}{message}";

            byte[] data = Encoding.UTF8.GetBytes(fullMessage);

            lock (clients)
            {
                foreach (var c in clients)
                {
                    if (c.RoomId == roomId)
                    {
                        try
                        {
                            c.Client.GetStream().Write(data, 0, data.Length);
                        }
                        catch { }
                    }
                }
            }
        }

    }
}
