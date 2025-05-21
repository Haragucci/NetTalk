using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetTalk.Server
{
    public static class ConsoleUI
    {
        public static void DrawHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("║                         ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("NetTalk Server");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("                     ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("║");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

    }
}
