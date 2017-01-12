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

        public async Task Run()
        {
            if (!Directory.Exists("Temp"))
                Directory.CreateDirectory("Temp");
            var files = Directory.GetFiles("Temp");
            foreach (string f in files)
                File.Delete(f);

            Discord = new DiscordSocketClient(new DiscordSocketConfig()
            {
                AudioMode = AudioMode.Outgoing,
                LogLevel = LogSeverity.Info
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
            await MainHandler.InitializeEarly(map);

            await Discord.LoginAsync(TokenType.Bot, MainHandler.ConfigHandler.getBotToken());
            await Discord.ConnectAsync();
            Console.WriteLine("Connected!");

            await Discord.SetGameAsync(null);

            await MainHandler.InitializeLater();

            await Task.Delay(-1);
        }
    }
}
