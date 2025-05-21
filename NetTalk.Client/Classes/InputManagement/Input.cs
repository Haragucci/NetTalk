using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetTalk.Client.Classes.InputManagement
{
    public static class Input
    {
        public static void RedrawInputLine(string currentInput)
        {
            int line = Console.CursorTop;
            Console.SetCursorPosition(0, line);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("> " + currentInput + new string(' ', Console.WindowWidth - currentInput.Length - 2));
            Console.ResetColor();
            Console.SetCursorPosition(2 + currentInput.Length, line);
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
    }
}
