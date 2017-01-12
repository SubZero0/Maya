using Maya.Handlers;
using Maya.Modules.Functions;
using Maya.WoWS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Commands;
using Discord;
using Maya.GuildHandlers;

namespace Maya.Controllers
{
    public class MainHandler
    {
        public DiscordSocketClient Client;

        //One-time Handlers
        public Handlers.ConfigHandler ConfigHandler { get; private set; }
        public CommandHandler CommandHandler { get; private set; }
        public PermissionHandler PermissionHandler { get; private set; }
        public TextHandler TextHandler { get; private set; }
        public ShipHandler ShipHandler { get; private set; }
        public BotHandler BotHandler { get; private set; }

        //Guild Handlers
        private Dictionary<ulong, GuildHandler> guilds = new Dictionary<ulong, GuildHandler>();

        //One-time Functions
        private ForumUpdater forumUpdater;

        public MainHandler(DiscordSocketClient Discord)
        {
            Client = Discord;
            ConfigHandler = new Handlers.ConfigHandler();
            CommandHandler = new CommandHandler();
            PermissionHandler = new PermissionHandler(this);
            TextHandler = new TextHandler(this);
            ShipHandler = new ShipHandler(this);
            BotHandler = new BotHandler();
            forumUpdater = new ForumUpdater(this);
        }

        public async Task InitializeEarly(IDependencyMap map)
        {
            await ConfigHandler.Initialize();
            await CommandHandler.Install(this, map);
            TextHandler.Initialize(ConfigHandler.getSwearString());
            await ShipHandler.Initialize();
            await BotHandler.Initialize();
        }

        public async Task InitializeLater()
        {
            forumUpdater.Initialize();
            await Task.CompletedTask;
        }

        internal async Task GuildAvailableEvent(SocketGuild guild)
        {
            GuildHandler gh = new GuildHandler(this, guild);
            await gh.Initialize();
            guilds[guild.Id] = gh;
        }

        public async Task ReloadGuild(SocketGuild guild)
        {
            GuildHandler gh = new GuildHandler(this, guild);
            await gh.Initialize();
            guilds[guild.Id] = gh;
        }

        public DatabaseHandler GuildDatabaseHandler(IGuild guild)
        {
            if (guild == null)
                return null;
            return guilds[guild.Id].DatabaseHandler;
        }

        public TagHandler GuildTagHandler(IGuild guild)
        {
            if (guild == null)
                return null;
            return guilds[guild.Id].TagHandler;
        }

        public IgnoreHandler GuildIgnoreHandler(IGuild guild)
        {
            if (guild == null)
                return null;
            return guilds[guild.Id].IgnoreHandler;
        }

        public MusicHandler GuildMusicHandler(IGuild guild)
        {
            if (guild == null)
                return null;
            return guilds[guild.Id].MusicHandler;
        }

        public TitleHandler GuildTitleHandler(IGuild guild)
        {
            if (guild == null)
                return null;
            return guilds[guild.Id].TitleHandler;
        }

        public GuildHandlers.ConfigHandler GuildConfigHandler(IGuild guild)
        {
            if (guild == null)
                return null;
            return guilds[guild.Id].ConfigHandler;
        }

        public PersonalityHandler GuildPersonalityHandler(IGuild guild)
        {
            if (guild == null)
                return null;
            return guilds[guild.Id].PersonalityHandler;
        }

        public string getCommandPrefix(IChannel channel)
        {
            if (channel is ITextChannel)
                return GuildConfigHandler((channel as ITextChannel).Guild).getCommandPrefix();
            return ConfigHandler.getDefaultCommandPrefix();
        }
    }
}
