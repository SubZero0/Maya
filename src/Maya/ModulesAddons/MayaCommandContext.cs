using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Maya.Controllers;

namespace Maya.ModulesAddons
{
    public class ModuleCommand : ModuleBase<MayaCommandContext> { }
    public class MayaCommandContext : ICommandContext
    {
        public IDiscordClient Client { get; }
        public IGuild Guild { get; }
        public MainHandler MainHandler { get; }
        public IMessageChannel Channel { get; }
        public IUser User { get; }
        public IUserMessage Message { get; }

        public bool IsPrivate => Channel is IPrivateChannel;

        public MayaCommandContext(IDiscordClient client, MainHandler handler, IUserMessage msg)
        {
            Client = client;
            Guild = (msg.Channel as IGuildChannel)?.Guild;
            Channel = msg.Channel;
            User = msg.Author;
            Message = msg;
            MainHandler = handler;
        }
    }
}
