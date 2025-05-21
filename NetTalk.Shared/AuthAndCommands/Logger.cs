using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetTalk.Shared
{
    public static class Logger
    {
        public static void Info(string message) => Log("INFO", message, ConsoleColor.Cyan);
        public static void Error(string message) => Log("ERROR", message, ConsoleColor.Red);
        public static void Debug(string message) => Log("DEBUG", message, ConsoleColor.Gray);
        public static void Auth(string message) => Log("AUTH", message, ConsoleColor.Green);
        public static void Room(string message) => Log("ROOM", message, ConsoleColor.Yellow);
        public static void Warn(string message) => Log("WARN", message, ConsoleColor.DarkYellow);

        private static void Log(string level, string message, ConsoleColor color)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Console.ForegroundColor = color;
            Console.WriteLine($"[{timestamp}] [{level}] {message}");
            Console.ResetColor();
        }
    }
}
