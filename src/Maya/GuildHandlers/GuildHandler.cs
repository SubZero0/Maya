using Discord;
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
        public TagHandler TagHandler { get; private set; }
        public IgnoreHandler IgnoreHandler { get; private set; }
        public MusicHandler MusicHandler { get; private set; }
        public TitleHandler TitleHandler { get; private set; }
        public PersonalityHandler PersonalityHandler { get; private set; }
        public GuildHandler(MainHandler MainHandler, IGuild Guild)
        {
            this.MainHandler = MainHandler;
            this.Guild = Guild;
            ConfigHandler = new ConfigHandler(this);
            DatabaseHandler = new DatabaseHandler(this);
            TagHandler = new TagHandler(this);
            IgnoreHandler = new IgnoreHandler(this);
            MusicHandler = new MusicHandler(this);
            TitleHandler = new TitleHandler(this);
            PersonalityHandler = new PersonalityHandler(this);
        }

        public async Task Initialize()
        {
            await Check();
            await initialize();
        }

        public async Task Check()
        {
            if (!Directory.Exists($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{Guild.Id}"))
            {
                Directory.CreateDirectory($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{Guild.Id}");
                foreach (string newPath in Directory.GetFiles($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}Default", "*.*", SearchOption.TopDirectoryOnly))
                    File.Copy(newPath, newPath.Replace($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}Default", $"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{Guild.Id}"));
            }
            await Task.CompletedTask;
        }

        private async Task initialize()
        {
            await ConfigHandler.Initialize();
            await DatabaseHandler.Initialize();
            await TagHandler.Initialize();
            IgnoreHandler.Initialize();
            MusicHandler.Initialize();
            await TitleHandler.Initialize();
            await PersonalityHandler.Initialize();
        }
    }
}
