using Discord;
using Maya.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Maya.GuildHandlers
{
    public class TagHandler : IGuildHandler
    {
        private GuildHandler GuildHandler;
        private Dictionary<string, Tag> tags = null;
        private Timer timer;
        public TagHandler(GuildHandler GuildHandler)
        {
            this.GuildHandler = GuildHandler;
        }

        public async Task InitializeAsync()
        {
            await LoadTagsAsync();
            timer = new Timer(Timer_Elapsed, null, Timeout.Infinite, Timeout.Infinite);
        }

        public async Task Close()
        {
            await SaveTagsAsync();
            timer.Dispose();
        }

        private async void Timer_Elapsed(object state)
        {
            await SaveTagsAsync();
        }

        public async Task SaveTagsAsync()
        {
            await Task.Run(() =>
            {
                Dictionary<string, Tag> temp = new Dictionary<string, Tag>(tags);
                File.WriteAllText($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{GuildHandler.Guild.Id}{Path.DirectorySeparatorChar}tags.json", JsonConvert.SerializeObject(temp));
            });
        }

        public async Task LoadTagsAsync()
        {
            await Task.Run(() =>
            {
                Dictionary<string, Tag> temp;
                temp = JsonConvert.DeserializeObject<Dictionary<string, Tag>>(File.ReadAllText($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{GuildHandler.Guild.Id}{Path.DirectorySeparatorChar}tags.json"));
                tags = temp;
            });
        }

        public void CreateTag(IUser u, string tag, string text)
        {
            tags[tag.ToLower()] = new Tag(tag, text, u.Id, DateTime.Now);
            timer.Change(300000, Timeout.Infinite);
        }

        public void RemoveTag(string tag)
        {
            tag = tag.ToLower();
            if (tags.ContainsKey(tag))
                tags.Remove(tag);
            timer.Change(300000, Timeout.Infinite);
        }

        public void EditTag(string tag, string text)
        {
            tag = tag.ToLower();
            if (!tags.ContainsKey(tag))
                return;
            tags[tag].text = text;
            timer.Change(300000, Timeout.Infinite);
        }

        public bool ContainsTag(string tag)
        {
            return tags.ContainsKey(tag.ToLower());
        }

        public Tag GetTag(string tag)
        {
            tag = tag.ToLower();
            if (!tags.ContainsKey(tag))
                return null;
            return tags[tag];
        }
    }
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
}
