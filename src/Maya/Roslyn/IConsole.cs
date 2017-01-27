using Discord;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Maya.Roslyn
{
    public class IConsole
    {
        public static void TitleCard(string title, string version = null, ConsoleColor? color = null)
        {
            if (color == null)
                color = ConsoleColor.Cyan;

            var card = new List<string>();
            card.Add($"┌{new string('─', 12)}{new string('─', title.Count())}{new string('─', 12)}┐");
            card.Add($"│{new string(' ', 12)}{title}{new string(' ', 12)}│");
            if (version != null)
            {
                int diff = title.Count() - version.Count() / 2;

                if (diff > 0)
                    card.Add($"│{new string(' ', 12 + diff)}{version}{new string(' ', 12 + diff)}│");
            }
            card.Add($"└{new string('─', 12)}{new string('─', title.Count())}{new string('─', 12)}┘");

            Console.Title = title;
            IConsole.NewLine(string.Join(Environment.NewLine, card));
        }
        public static void Append(string text, ConsoleColor? foreground = null, ConsoleColor? background = null)
        {
            if (foreground == null)
                foreground = ConsoleColor.White;
            if (background == null)
                background = ConsoleColor.Black;

            Console.ForegroundColor = (ConsoleColor)foreground;
            Console.BackgroundColor = (ConsoleColor)background;
            Console.Write(text);
        }
        public static void NewLine(string text = "", ConsoleColor? foreground = null, ConsoleColor? background = null)
        {
            if (foreground == null)
                foreground = ConsoleColor.White;
            if (background == null)
                background = ConsoleColor.Black;

            Console.ForegroundColor = (ConsoleColor)foreground;
            Console.BackgroundColor = (ConsoleColor)background;
            Console.Write(Environment.NewLine + text);
        }
        public static void Log(LogSeverity severity, string source, string message)
        {
            NewLine($"{DateTime.Now.ToString("hh:mm:ss")} ", ConsoleColor.DarkMagenta);
            Append($"[{severity}] ", ConsoleColor.Cyan);
            Append($"{source}: ", ConsoleColor.Yellow);
            Append(message, ConsoleColor.DarkRed);
        }
        public static void Log(IUserMessage msg)
        {
            var channel = (msg.Channel as IGuildChannel);
            NewLine($"{DateTime.Now.ToString("hh:mm:ss")} ", ConsoleColor.DarkMagenta);

            if (channel?.Guild == null)
                Append($"[PM] ", ConsoleColor.DarkRed);
            else
                Append($"[{channel.Guild.Name} #{channel.Name}] ", ConsoleColor.Yellow);

            Append($"{msg.Author}: ", ConsoleColor.Yellow);
            Append(msg.Content, ConsoleColor.White);
        }
        public static void ResetColors()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
        }
    }
}
