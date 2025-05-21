using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetTalk.Client
{
    public class AuthHandler
    {
        static string ServerAddress = "YOUR LOCAL IP ADDRESS FROM SERVER";
        public static bool LoginTest(string name, string password)
        {
            try
            {
                TcpClient client = new TcpClient(ServerAddress, 5000);
                NetworkStream stream = client.GetStream();
                string msg = $"LOGIN|{name}|{password}|dummyroom|TEST";
                byte[] data = Encoding.UTF8.GetBytes(msg);
                stream.Write(data, 0, data.Length);

                byte[] buffer = new byte[1024];
                int bytes = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytes);
                client.Close();
                return response.Contains("Login succeeded");
            }
            catch
            {
                return false;
            }
        }

        public static bool Register(string name, string password)
        {
            try
            {
                TcpClient client = new TcpClient(ServerAddress, 5000);
                NetworkStream stream = client.GetStream();
                string msg = $"REGISTER|{name}|{password}|IGNORED|IGNORED";
                byte[] data = Encoding.UTF8.GetBytes(msg);
                stream.Write(data, 0, data.Length);

                byte[] buffer = new byte[1024];
                int bytes = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytes);
                client.Close();
                return response.Contains("succeeded");
            }
            catch
            {
                Console.WriteLine("[ERROR] Connection to the Server failed!");
                return false;
            }
        }

        public static string GenerateRoomId()
        {
            var rnd = new Random();
            string RandomPart() => $"{Convert.ToBase64String(BitConverter.GetBytes(rnd.Next())).Replace("=", "").Replace("+", "").Replace("/", "").Substring(0, 4).ToUpper()}";
            return $"{RandomPart()}-{RandomPart()}-{RandomPart()}";
        }

        public static string ReadPassword()
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Enter) break;
                if (key.Key == ConsoleKey.Backspace && sb.Length > 0)
                {
                    sb.Length--;
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    sb.Append(key.KeyChar);
                    Console.Write("*");
                }
            }
            Console.WriteLine();
            return sb.ToString();
        }
    }
}
