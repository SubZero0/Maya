using Discord;
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
    public class ConfigHandler
    {
        private GuildHandler GuildHandler;
        private JObject config;
        public ConfigHandler(GuildHandler GuildHandler)
        {
            this.GuildHandler = GuildHandler;
            config = null;
        }

        public async Task Initialize()
        {
            await load();
        }

        public async Task load()
        {
            await Task.Run(() =>
            {
                config = JObject.Parse(File.ReadAllText($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{GuildHandler.Guild.Id}{Path.DirectorySeparatorChar}config.json"));
            });
        }

        public string getCommandPrefix()
        {
            return (string)config["CommandPrefix"];
        }

        public bool isChannelAllowed(IChannel channel)
        {
            return Utils.isChannelListed(channel, config["AllowedChannels"].ToObject<List<string>>());
        }

        public WrapperNF getNotifications()
        {
            return new WrapperNF((JObject)config["Notifications"]);
        }

        public WrapperNF getForumUpdates()
        {
            return new WrapperNF((JObject)config["ForumUpdates"]);
        }

        public WrapperM getMusic()
        {
            return new WrapperM((JObject)config["Music"]);
        }

        public WrapperCS getChatterBot()
        {
            return new WrapperCS((JObject)config["ChatterBot"]);
        }

        public WrapperCS getSwearJar()
        {
            return new WrapperCS((JObject)config["SwearJar"]);
        }
    }
    public class WrapperNF
    {
        public WrapperNF(JObject obj) { isEnabled = (bool)obj["Enabled"]; TextChannel = (string)obj["TextChannel"]; }
        public bool isEnabled { get; private set; }
        public string TextChannel { get; private set; }
    }
    public class WrapperM
    {
        public WrapperM(JObject obj) { isEnabled = (bool)obj["Enabled"]; TextChannels = obj["TextChannels"].ToObject<List<string>>(); VoiceChannel = (string)obj["VoiceChannel"]; }
        public bool isEnabled { get; private set; }
        public List<string> TextChannels { get; private set; }
        public string VoiceChannel { get; private set; }
    }
    public class WrapperCS
    {
        public WrapperCS(JObject obj) { isEnabled = (bool)obj["Enabled"]; TextChannels = obj["TextChannels"].ToObject<List<string>>(); }
        public bool isEnabled { get; private set; }
        public List<string> TextChannels { get; private set; }
    }
}
