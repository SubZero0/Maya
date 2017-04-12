using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Audio;
using System.IO;
using Maya.Controllers;

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
                LogLevel = LogSeverity.Error,
                AlwaysDownloadUsers = true,
            });

            Discord.Log += (message) =>
            {
                Console.WriteLine($"{message.ToString()}");
                return Task.CompletedTask;
            };

            var map = new DependencyMap();
            map.Add(Discord);

            MainHandler = new MainHandler(Discord);
            Discord.GuildAvailable += MainHandler.GuildAvailableEvent;
            Discord.LeftGuild += MainHandler.LeftGuildEvent;
            await MainHandler.InitializeEarlyAsync(map);

            await Discord.LoginAsync(TokenType.Bot, MainHandler.ConfigHandler.GetBotToken());
            await Discord.StartAsync();

            await Task.Delay(3000);
            Console.WriteLine("Connected!");

            await Discord.SetGameAsync(null);

            await MainHandler.InitializeLaterAsync();

            await Task.Delay(-1);
        }
    }
}
