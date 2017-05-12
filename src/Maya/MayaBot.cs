using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.IO;
using Maya.Controllers;
using Microsoft.Extensions.DependencyInjection;

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
                LogLevel = LogSeverity.Info,
                MessageCacheSize = 100
            });

            Discord.Log += (message) =>
            {
                Console.WriteLine($"{message.ToString()}");
                return Task.CompletedTask;
            };
            Discord.Ready += async () =>
            {
                Console.WriteLine("Connected!");
                await Discord.SetGameAsync(null);
                await MainHandler.InitializeLaterAsync();
            };

            var services = new ServiceCollection();
            services.AddSingleton(Discord);

            MainHandler = new MainHandler(Discord);
            Discord.GuildAvailable += MainHandler.GuildAvailableEvent;
            Discord.LeftGuild += MainHandler.LeftGuildEvent;
            await MainHandler.InitializeEarlyAsync(services.BuildServiceProvider());

            await Discord.LoginAsync(TokenType.Bot, MainHandler.ConfigHandler.GetBotToken());
            await Discord.StartAsync();
            await Task.Delay(-1);
        }
    }
}