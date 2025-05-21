using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetTalk.Client.Classes.UI
{
    public static class UI
    {
        public static void DrawLoginHeader(string name)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("╔════════════════════════════════════════════╗");
            Console.WriteLine($"          Logged in as: {name}               ");
            Console.WriteLine("╚════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine("\n[1] Create Room");
            Console.WriteLine("[2] Join Room");
            Console.WriteLine("[3] Logout");
            Console.WriteLine("[4] Exit");
            Console.Write("Choose option: ");
        }

        public static void DrawHeader(string roomId, string username)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("╔════════════════════════════════════════════╗");
            Console.WriteLine($"║        Logged in as: {username.PadRight(22)}║");
            Console.WriteLine($"║       Room-ID: {roomId.PadRight(28)}║");
            Console.WriteLine("╚════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        public static void DrawWelcomeHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════╗");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("║");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("              Welcome to ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("NetTalk ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("                     ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("║");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╚══════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine("[1] Sign up");
            Console.WriteLine("[2] Log in");
            Console.Write("Choose option: ");
        }
    }
}
