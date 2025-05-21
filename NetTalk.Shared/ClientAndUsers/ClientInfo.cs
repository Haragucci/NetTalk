using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetTalk.Shared
{
    public class ClientInfo
    {
        public TcpClient Client;
        public string Name;
        public string RoomId;

        public ClientInfo(TcpClient client, string name, string roomId)
        {
            Client = client;
            Name = name;
            RoomId = roomId;
        }
    }
}
