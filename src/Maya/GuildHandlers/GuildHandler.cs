using Discord;
using Discord.WebSocket;
using Maya.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Maya.GuildHandlers
{
    public class GuildHandler
    {
        public MainHandler MainHandler { get; private set; }
        public IGuild Guild { get; private set; }
        public ConfigHandler ConfigHandler { get; private set; }
        public DatabaseHandler DatabaseHandler { get; private set; }
        public IgnoreHandler IgnoreHandler { get; private set; }
        public MusicHandler MusicHandler { get; private set; }
        public PersonalityHandler PersonalityHandler { get; private set; }
        public TitleHandler TitleHandler { get; private set; }
        public TagHandler TagHandler { get; private set; }
        public GuildHandler(MainHandler MainHandler, SocketGuild Guild)
        {
            this.MainHandler = MainHandler;
            this.Guild = Guild;
            ConfigHandler = new ConfigHandler(this);
            DatabaseHandler = new DatabaseHandler(this);
            IgnoreHandler = new IgnoreHandler(this);
            MusicHandler = new MusicHandler(this);
            PersonalityHandler = new PersonalityHandler(this);
            TitleHandler = new TitleHandler(this);
            TagHandler = new TagHandler(this);
        }

        public async Task InitializeAsync()
        {
            Check();
            await PrivateInitializeAsync();
        }

        private async Task PrivateInitializeAsync()
        {
            await ConfigHandler.InitializeAsync();
            await DatabaseHandler.InitializeAsync();
            await IgnoreHandler.InitializeAsync();
            await PersonalityHandler.InitializeAsync();
            await TitleHandler.InitializeAsync();
            await TagHandler.InitializeAsync();
            await MusicHandler.InitializeAsync();
        }

        public async Task Close()
        {
            await ConfigHandler.Close();
            await DatabaseHandler.Close();
            await IgnoreHandler.Close();
            await MusicHandler.Close();
            await PersonalityHandler.Close();
            await TitleHandler.Close();
            await TagHandler.Close();
        }

        public Task RenewGuildObject(SocketGuild Guild)
        {
            this.Guild = Guild;
            //await Guild.DownloadUsersAsync(); //TODO: Locking the thread
            return Task.CompletedTask;
        }
         
        public void Check()
        {
            if (!Directory.Exists($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{Guild.Id}"))
            {
                Directory.CreateDirectory($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{Guild.Id}");
                foreach (string newPath in Directory.GetFiles($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}Default", "*.*", SearchOption.TopDirectoryOnly))
                    File.Copy(newPath, newPath.Replace($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}Default", $"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{Guild.Id}"));
            }
        }

        public void DeleteGuildFolder()
        {
            if (!Directory.Exists($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{Guild.Id}"))
                Directory.Delete($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{Guild.Id}", true);
        }
    }
}
