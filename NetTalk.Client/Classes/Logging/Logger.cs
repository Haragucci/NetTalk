using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetTalk.Client.Classes.Logging
{
    public static class Logger
    {
        public static void PrintLog(string level, string message, ConsoleColor? overrideColor = null)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string paddedLevel = level.ToUpper().PadRight(8);

            ConsoleColor color;
            if (overrideColor.HasValue)
            {
                color = overrideColor.Value;
            }
            else
            {
                string upperLevel = level.ToUpper();
                if (upperLevel == "INFO")
                    color = ConsoleColor.Green;
                else if (upperLevel == "ERROR")
                    color = ConsoleColor.Red;
                else if (upperLevel == "WARNING")
                    color = ConsoleColor.Yellow;
                else if (upperLevel == "DEBUG")
                    color = ConsoleColor.Cyan;
                else
                    color = ConsoleColor.Gray;
            }

            Console.ForegroundColor = color;
            Console.WriteLine($"[{timestamp}] [{paddedLevel}] {message}");
            Console.ResetColor();
        }
    }
}
