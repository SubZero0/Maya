using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Audio;
using System.IO;
using Maya.Controllers;
using Maya.Roslyn;

namespace Maya
{
    public class MayaBot
    {
        private DiscordSocketClient Discord;
        private MainHandler MainHandler;

        public async Task RunAsync()
        { 
            if (!Directory.Exists("Temp"))
                Directory.CreateDirectory("Temp");
            var files = Directory.GetFiles("Temp");
            foreach (string f in files)
                File.Delete(f);

            Discord = new DiscordSocketClient(new DiscordSocketConfig()
            {
                AudioMode = AudioMode.Outgoing,
                LogLevel = LogSeverity.Info,
                DownloadUsersOnGuildAvailable = true
            });
            IConsole.TitleCard("Maya", DiscordConfig.Version);

            Discord.Log += (l)
                => Task.Run(()
                => IConsole.Log(l.Severity, l.Source, l.Exception?.ToString() ?? l.Message));

            var map = new DependencyMap();
            map.Add(Discord);

            MainHandler = new MainHandler(Discord);
            Discord.GuildAvailable += MainHandler.GuildAvailableEvent;
            Discord.LeftGuild += MainHandler.LeftGuildEvent;
            await MainHandler.InitializeEarlyAsync(map);

            await Discord.LoginAsync(TokenType.Bot, MainHandler.ConfigHandler.GetBotToken());
            await Discord.ConnectAsync();

            IConsole.Log(LogSeverity.Info, "Game", "Input Game: ");
            await Discord.SetGameAsync(Console.ReadLine());

            await MainHandler.InitializeLaterAsync();

            await Task.Delay(-1);
        }
    }
}
