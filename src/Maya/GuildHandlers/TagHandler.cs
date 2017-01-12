using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Maya.GuildHandlers
{
    public class Tag
    {
        public string tag;
        public string text;
        public ulong creator;
        public DateTime when;
        public Tag(string tag, string text, ulong creator_id, DateTime when)
        {
            this.tag = tag;
            this.text = text;
            this.creator = creator_id;
            this.when = when;
        }
    }
    public class TagHandler
    {
        private GuildHandler GuildHandler;
        private static Dictionary<string, Tag> tags = null;
        private static Timer timer;
        public TagHandler(GuildHandler GuildHandler)
        {
            this.GuildHandler = GuildHandler;
        }

        public async Task Initialize()
        {
            await loadTags();
            timer = new Timer(Timer_Elapsed, null, Timeout.Infinite, Timeout.Infinite);
        }

        private async void Timer_Elapsed(object state)
        {
            await saveTags();
        }

        public async Task saveTags()
        {
            await Task.Run(() =>
            {
                Dictionary<string, Tag> temp = new Dictionary<string, Tag>(tags);
                File.WriteAllText($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{GuildHandler.Guild.Id}{Path.DirectorySeparatorChar}tags.json", JsonConvert.SerializeObject(temp));
            });
        }

        public async Task loadTags()
        {
            await Task.Run(() =>
            {
                Dictionary<string, Tag> temp;
                temp = JsonConvert.DeserializeObject<Dictionary<string, Tag>>(File.ReadAllText($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{GuildHandler.Guild.Id}{Path.DirectorySeparatorChar}tags.json"));
                tags = temp;
            });
        }

        public void createTag(IUser u, string tag, string text)
        {
            tags[tag.ToLower()] = new Tag(tag, text, u.Id, DateTime.Now);
            timer.Change(300000, Timeout.Infinite);
        }

        public void removeTag(string tag)
        {
            tag = tag.ToLower();
            if (tags.ContainsKey(tag))
                tags.Remove(tag);
            timer.Change(300000, Timeout.Infinite);
        }

        public void editTag(string tag, string text)
        {
            tag = tag.ToLower();
            if (!tags.ContainsKey(tag))
                return;
            tags[tag].text = text;
            timer.Change(300000, Timeout.Infinite);
        }

        public bool containsTag(string tag)
        {
            return tags.ContainsKey(tag.ToLower());
        }

        public Tag getTag(string tag)
        {
            tag = tag.ToLower();
            if (!tags.ContainsKey(tag))
                return null;
            return tags[tag];
        }
    }
}
