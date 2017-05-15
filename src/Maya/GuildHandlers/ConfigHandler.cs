using Discord;
using Maya.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Maya.GuildHandlers
{
    public class ConfigHandler : IGuildHandler
    {
        private GuildHandler GuildHandler;
        private JObject config;
        public ConfigHandler(GuildHandler GuildHandler)
        {
            this.GuildHandler = GuildHandler;
            config = null;
        }

        public async Task InitializeAsync()
        {
            await LoadAsync();
        }

        public Task Close()
        {
            return Task.CompletedTask;
        }

        public async Task LoadAsync()
        {
            await Task.Run(() =>
            {
                config = JObject.Parse(File.ReadAllText($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{GuildHandler.Guild.Id}{Path.DirectorySeparatorChar}config.json"));
            });
        }

        public string GetCommandPrefix()
        {
            return (string)config["CommandPrefix"];
        }

        public bool IsChannelAllowed(IChannel channel)
        {
            return channel.IsChannelListed(config["AllowedChannels"].ToObject<List<string>>());
        }

        public WrapperNF GetNotifications()
        {
            return new WrapperNF((JObject)config["Notifications"]);
        }

        public WrapperNF GetForumUpdates()
        {
            return new WrapperNF((JObject)config["ForumUpdates"]);
        }

        public WrapperM GetMusic()
        {
            return new WrapperM((JObject)config["Music"]);
        }

        public WrapperCSA GetChatterBot()
        {
            return new WrapperCSA((JObject)config["ChatterBot"]);
        }

        public WrapperCSA GetSwearJar()
        {
            return new WrapperCSA((JObject)config["SwearJar"]);
        }

        public WrapperCSA GetAutoResponse()
        {
            return new WrapperCSA((JObject)config["AutoResponse"]);
        }
    }
    public class WrapperNF
    {
        public WrapperNF(JObject obj) { IsEnabled = (bool)obj["Enabled"]; TextChannel = (string)obj["TextChannel"]; }
        public bool IsEnabled { get; private set; }
        public string TextChannel { get; private set; }
    }
    public class WrapperM
    {
        public WrapperM(JObject obj) { IsEnabled = (bool)obj["Enabled"]; TextChannels = obj["TextChannels"].ToObject<List<string>>(); VoiceChannel = (string)obj["VoiceChannel"]; }
        public bool IsEnabled { get; private set; }
        public List<string> TextChannels { get; private set; }
        public string VoiceChannel { get; private set; }
    }
    public class WrapperCSA
    {
        public WrapperCSA(JObject obj) { IsEnabled = (bool)obj["Enabled"]; TextChannels = obj["TextChannels"].ToObject<List<string>>(); }
        public bool IsEnabled { get; private set; }
        public List<string> TextChannels { get; private set; }
    }
}
