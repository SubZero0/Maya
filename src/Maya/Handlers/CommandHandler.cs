using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using Maya.Controllers;
using Discord;
using Maya.ModulesAddons;

namespace Maya
{
    public class CommandHandler
    {
        private CommandService commands;
        private DiscordSocketClient client;
        private IDependencyMap map;
        private MainHandler MainHandler;

        public async Task Install(MainHandler _MainHandler, IDependencyMap _map)
        {
            MainHandler = _MainHandler;
            client = _map.Get<DiscordSocketClient>();
            commands = new CommandService();
            _map.Add(commands);
            map = _map;

            await commands.AddModulesAsync(Assembly.GetEntryAssembly());

            client.MessageReceived += HandleCommand;
        }

        public Task HandleCommand(SocketMessage parameterMessage)
        {
            var msg = parameterMessage as SocketUserMessage;
            if (msg == null) return Task.CompletedTask;
            if (msg.Channel is ITextChannel && !MainHandler.GuildConfigHandler((msg.Channel as ITextChannel).Guild).isChannelAllowed(msg.Channel)) return Task.CompletedTask;
            if (msg.Channel is ITextChannel && MainHandler.GuildIgnoreHandler((msg.Channel as ITextChannel).Guild).contains(msg.Author.Id)) return Task.CompletedTask;
            int argPos = 0;
            if (!(/*msg.HasMentionPrefix(client.CurrentUser, ref argPos) || */msg.HasStringPrefix(MainHandler.getCommandPrefix(msg.Channel), ref argPos))) return Task.CompletedTask;
            var _ = HandleCommandAsync(msg, argPos);
            return Task.CompletedTask;
        }

        public async Task HandleCommandAsync(SocketUserMessage msg, int argPos)
        {
            var context = new MayaCommandContext(client, MainHandler, msg);
            var result = await commands.ExecuteAsync(context, argPos, map);
            if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                if (result.ErrorReason != "null")
                    await msg.Channel.SendMessageAsync(result.ErrorReason);
        }
    }
}
