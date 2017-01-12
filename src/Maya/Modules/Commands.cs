using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Maya.Attributes;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json.Linq;
using System.IO;
using static Maya.Enums.Administration;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis;
using System.Reflection;
using Maya.Roslyn;
using Maya.WoWS;
using Maya.Handlers;
using System.Globalization;
using Maya.Music;
using Maya.ModulesAddons;
using Maya.GuildHandlers;

namespace Maya.Modules.Commands
{
    public class GeneralCommands : ModuleCommand
    {
        [Command("image")]
        [Alias("i")]
        public async Task Image([Required, Remainder] string search = null)
        {
            using (var httpClient = new HttpClient())
            {
                var link = $"https://api.cognitive.microsoft.com/bing/v5.0/images/search?q={search}&count=10&offset=0&mkt=en-us&safeSearch=Moderate";
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "...");
                var res = await httpClient.GetAsync(link);
                if (!res.IsSuccessStatusCode)
                {
                    await ReplyAsync($"An error occurred: {res.ReasonPhrase}");
                    return;
                }
                JObject result = JObject.Parse(await res.Content.ReadAsStringAsync());
                JArray arr = (JArray)result["value"];
                if (arr.Count == 0)
                {
                    await ReplyAsync("No results found.");
                    return;
                }
                JObject image = (JObject)arr[0];
                EmbedBuilder eb = new EmbedBuilder();
                eb.Title = $"Image: {search}";
                eb.Color = Utils.getRandomColor();
                eb.Description = $"[Image link]({(string)image["contentUrl"]})";
                eb.ImageUrl = (string)image["contentUrl"];
                await ReplyAsync("", false, eb);
            }
        }

        [Group("title")]
        [Summary("Main command for adding, deleting, and viewing titles")]
        [RequireContext(ContextType.Guild)]
        public class TitleModule : ModuleCommand
        {
            [Command]
            [Summary("Show titles")]
            public async Task Title(IGuildUser user = null)
            {
                if (user == null)
                    user = Context.User as IGuildUser;
                List<string> titles = Context.MainHandler.GuildTitleHandler(Context.Guild).getTitles(user);
                if (titles.Count == 0)
                {
                    await ReplyAsync($"**{user.Nickname ?? user.Username}** doesn't have any titles.");
                    return;
                }
                EmbedBuilder eb = new EmbedBuilder();
                eb.Title = $"{user.Nickname ?? user.Username}'s Titles";
                eb.Color = Utils.getRandomColor();
                eb.Description = $"『{String.Join("』 『", titles)}』";
                await ReplyAsync("", false, eb);
            }

            [Command("add")]
            [Summary("Add a new title")]
            [RequireAdmin]
            public async Task Add([Required] IGuildUser user = null, [Required, Remainder] string title = null)
            {
                if (Context.MainHandler.GuildTitleHandler(Context.Guild).containsTitle(user, title))
                {
                    await ReplyAsync($"{user.Nickname ?? user.Username} already has 『{title}』.");
                    return;
                }
                if (title.Length > 150)
                {
                    await ReplyAsync("Title exceeds length limit (> 150).");
                    return;
                }
                Context.MainHandler.GuildTitleHandler(Context.Guild).addTitle(user, title);
                await ReplyAsync($":grey_exclamation: **{user.Nickname ?? user.Username}** got a new title! 『{title}』!");
            }

            [Command("delete")]
            [Summary("Delete an existing title")]
            [RequireAdmin]
            public async Task Delete([Required] IGuildUser user = null, [Required, Remainder] string title = null)
            {
                if (!Context.MainHandler.GuildTitleHandler(Context.Guild).containsTitle(user, title))
                {
                    await ReplyAsync("Title not found.");
                    return;
                }
                Context.MainHandler.GuildTitleHandler(Context.Guild).removeTitle(user, title);
                await ReplyAsync($":grey_exclamation: **{user.Nickname ?? user.Username}** lost the title 『{title}』!");
            }
        }

        [Command("swearjar")]
        [Summary("Show how much money the swear jar currently has")]
        [RequireContext(ContextType.Guild)]
        public async Task Swearjar()
        {
            if (!Context.MainHandler.GuildDatabaseHandler(Context.Guild).isDbReady())
            {
                await ReplyAsync("Loading...");
                return;
            }
            await ReplyAsync($"The swear jar currently has {Convert.ToDecimal(Context.MainHandler.GuildDatabaseHandler(Context.Guild).getSwearJar()).ToString("C", new CultureInfo("en-US"))}.");
        }

        [Command("marry")]
        [Summary("Make a proposal")]
        [RequireContext(ContextType.Guild)]
        [Cooldown(120)]
        public async Task Marry()
        {
            if (!Context.MainHandler.GuildPersonalityHandler(Context.Guild).isReady())
            {
                await ReplyAsync("Loading...");
                return;
            }
            await ReplyAsync(Utils.getRandomWeightedChoice(Context.MainHandler.GuildPersonalityHandler(Context.Guild).getMarryAnswers()));
        }

        [Command("kill")]
        [Summary("Stabs someone or something")]
        [RequireContext(ContextType.Guild)]
        public async Task Kill([Required, Remainder] string who = null)
        {
            if (!Context.MainHandler.GuildPersonalityHandler(Context.Guild).isReady())
            {
                await ReplyAsync("Loading...");
                return;
            }
            if (who.Equals("me", StringComparison.OrdinalIgnoreCase) && Context.User.Id == (await Context.Client.GetApplicationInfoAsync()).Owner.Id)
                await ReplyAsync("I can't kill you :(");
            else if (who.Equals("paulo", StringComparison.OrdinalIgnoreCase) && Context.User.Id == (await Context.Client.GetApplicationInfoAsync()).Owner.Id)
                await ReplyAsync("I can't kill you :(");
            else if (who.Equals("paulo", StringComparison.OrdinalIgnoreCase) && Context.User.Id != (await Context.Client.GetApplicationInfoAsync()).Owner.Id)
                await ReplyAsync("I can't kill Paulo...\n* stabs you *");
            else if (who.Equals(Context.MainHandler.GuildPersonalityHandler(Context.Guild).getName(), StringComparison.OrdinalIgnoreCase) && await Context.MainHandler.PermissionHandler.isAtLeast(Context.User, AdminLevel.ADMIN))
            {
                await ReplyAsync(Context.MainHandler.GuildPersonalityHandler(Context.Guild).getOfflineString());
                await Task.Delay(1000);
                Environment.Exit(0);
            }
            else if (who.Equals("myself", StringComparison.OrdinalIgnoreCase) || who == Context.MainHandler.GuildPersonalityHandler(Context.Guild).getName().ToLower())
                await ReplyAsync("I won't kill myself...\n* stabs you *");
            else if (who.Equals("me", StringComparison.OrdinalIgnoreCase))
                await ReplyAsync($"It's my pleasure! ^-^\n* stabs {Context.User.Mention} *");
            else
                await ReplyAsync($"It's my pleasure! ^-^\n* stabs {who} *");
        }

        [Command("joke")]
        [Summary("Show a joke")]
        public async Task Joke()
        {
            IMessage m = await ReplyAsync("Processing...");
            using (var httpClient = new HttpClient())
            {
                int isImg = new Random().Next(2);
                string link = $"http://www.amazingjokes.com/{(isImg == 1 ? "image" : "jokes")}/random";
                var res = await httpClient.GetAsync(link);
                if (!res.IsSuccessStatusCode)
                {
                    await m.DeleteAsync();
                    await ReplyAsync($"An error occurred: {res.ReasonPhrase}");
                    return;
                }
                string html = await res.Content.ReadAsStringAsync();
                EmbedBuilder eb = new EmbedBuilder();
                if (isImg == 1)
                {
                    string[] ps = html.Split(new string[] { "]\">" }, StringSplitOptions.None)[1].Split(new string[] { "</h1>" }, StringSplitOptions.None);
                    eb.Author = new EmbedAuthorBuilder().WithName($"**{ps[0]}**");
                    eb.ImageUrl = ps[1].Split(new string[] { "<img src=\"" }, StringSplitOptions.None)[1].Split('"')[0];
                }
                else
                {
                    string[] ps = html.Split(new string[] { "<div class=\"thumbnail\">" }, StringSplitOptions.None)[1].Split(new string[] { "\">" }, StringSplitOptions.None)[1].Split(new string[] { "</h1>" }, StringSplitOptions.None);
                    eb.Author = new EmbedAuthorBuilder().WithName($"**{ps[0]}**");
                    eb.Description = Utils.StripTags(WebUtility.HtmlDecode(ps[1].Split(new string[] { " </div>" }, StringSplitOptions.None)[0].Trim()));
                }
                await ReplyAsync("", false, eb);
            }
            await m.DeleteAsync();
        }

        [Command("8ball")]
        [Summary("Answer a question")]
        public async Task Eightball([Required, Remainder] string question = null)
        {
            string[] ans = new string[] {
                        "It is certain",
                        "It is decidedly so",
                        "Without a doubt",
                        "Yes, definitely",
                        "You may rely on it",
                        "As I see it, yes",
                        "Most likely",
                        "Outlook good",
                        "Yes",
                        "Signs point to yes",
                        "Reply hazy try again",
                        "Ask again later",
                        "Better not tell you now",
                        "Cannot predict now",
                        "Concentrate and ask again",
                        "Don't count on it",
                        "My reply is no",
                        "My sources say no",
                        "Outlook not so good",
                        "Very doubtful"
                    };
            EmbedBuilder eb = new EmbedBuilder();
            eb.Author = new EmbedAuthorBuilder().WithName($"Question: {question}");
            eb.Description = ans[new Random().Next(ans.Length)];
            await ReplyAsync("", false, eb);
        }

        [Command("rate")]
        [Summary("Rate what you typed")]
        public async Task Rate([Required, Remainder] string text = null)
        {
            await ReplyAsync($":thinking: I would rate '{text}' a {new Random().Next(11)}/10");
        }

        [Command("meaning")]
        [Summary("Returns the meaning of the text")]
        public async Task Meaning([Required, Remainder] string text = null)
        {
            using (var httpClient = new HttpClient())
            {
                string link = $"http://www.urbandictionary.com/define.php?term={text}";
                var res = await httpClient.GetAsync(link);
                if (!res.IsSuccessStatusCode)
                {
                    await ReplyAsync($"An error occurred: {res.ReasonPhrase}");
                    return;
                }
                string html = await res.Content.ReadAsStringAsync();
                string result = html.Split(new string[] { "<div class='meaning'>" }, StringSplitOptions.None)[1].Split(new string[] { "</div>" }, StringSplitOptions.None)[0];
                if (result.Contains("<p>There aren't any definitions for"))
                {
                    await ReplyAsync($"No definitions for '{text}'.");
                    return;
                }
                string output = Utils.StripTags(WebUtility.HtmlDecode(result));
                if (output.Length > 1980)
                    for (int i = 1980; i > 100; i--)
                        if (output[i] == ' ')
                        {
                            output = output.Substring(0, i) + "...";
                            break;
                        }
                EmbedBuilder eb = new EmbedBuilder();
                eb.Title = $"Meaning: {text}";//TODO: Add a imagem do urban?
                eb.Color = Utils.getRandomColor();
                eb.Description = output;
                await ReplyAsync("", false, eb);
            }
        }

        [Command("mywtr")]
        [Summary("Show a random answer to how good you are")]
        [Cooldown]
        public async Task Mywtr()
        {
            string[,] a = {
                        {"SUPER-UNICUM!!!111!!","1"},
                        {"UNICUM!","3"},
                        {"l2p scrublord","10"},
                        {"As good as WARBEASTY","15"},
                        {"Calling you a noob would be too good...","15"},
                        {"Average","15"},
                        {"Above average","15"},
                        {"Below average","15"},
                        {"Average","15"},
                        {"Bad","15"},
                        {"Very bad","15"},
                        {"Tomato","15"},
                        {"Average","15"},
                        {"... better not to tell you so I don't hurt your feelings...","15"},
                        {"Here's a tip: press your left mouse button to shoot things.","15"},
                        {"With that aim, I'm impressed you could even hit the battle button.","15"},
                        {"Please, look at the screen when you are playing.","15"},
                        {"You know WoWS isn't turn based combat, right? When they shoot at you, feel free to shoot back.","15"},
                        {"You would make a perfect stormtrooper.","15"},
                        {"Not good not bad...","15"},
                        {"Oh! You play really well!","10"},
                        {"Wow! Are you aiming to be like iChase?","10"},
                        {"You are good! Congrats~!","10"},
                        {"Fruit salad","10"},
                        {"Potato","10"}
                    };
            await ReplyAsync(Utils.getRandomWeightedChoice(a));
        }

        [Command("quote")]
        [Summary("Quote user messages")]
        public async Task ForceSave([Required] params string[] message_ids)
        {
            List<IMessage> msgs = new List<IMessage>();
            IUser author = null;
            foreach (string ids in message_ids)
            {
                try
                {
                    ulong id = ulong.Parse(ids);
                    IMessage m = await Context.Channel.GetMessageAsync(id);
                    if(m.Content.Length == 0)
                    {
                        await ReplyAsync($"The message '{m.Id}' doesn't have any text.");
                        return;
                    }
                    if (author == null)
                        author = m.Author;
                    else if(author != m.Author)
                    {
                        await ReplyAsync($"The message '{m.Id}' doesn't belong to the same user ({author.Username}).");
                        return;
                    }
                    msgs.Add(m);
                }
                catch (Exception) { }
            }
            if(msgs.Count==0)
            {
                await ReplyAsync("No messages to quote.");
                return;
            }
            DateTime older, newer;
            older = newer = msgs.First().Timestamp.DateTime;
            foreach(IMessage m in msgs.Skip(1))
            {
                if (older.CompareTo(m.Timestamp.DateTime) > 0)
                    older = m.Timestamp.DateTime;
                else if (newer.CompareTo(m.Timestamp.DateTime) < 0)
                    newer = m.Timestamp.DateTime;
            }
            if ((newer - older).TotalMinutes > 10)
            {
                await ReplyAsync("The time between messages is too big (> 10 minutes).");
                return;
            }
            var ordered_msgs = msgs.OrderBy(x => x.Timestamp);
            EmbedBuilder eb = new EmbedBuilder();
            eb.Author = new EmbedAuthorBuilder().WithName($"{author.Username}#{author.Discriminator}").WithIconUrl(author.AvatarUrl);
            eb.Color = Utils.getRandomColor();
            eb.Description = String.Join("\n", ordered_msgs.Select(x => x.Content));
            eb.Timestamp = older;
            await ReplyAsync("", false, eb);
        }

        [Group("tag")]
        [Summary("Main command for creating, deleting, editing, and viewing tags")]
        [RequireContext(ContextType.Guild)]
        public class TagModule : ModuleCommand
        {
            [Command]
            [Summary("Show a tag")]
            public async Task Tag([Required("[tag/create/edit/delete]"), Remainder] string tag = null)
            {
                if (!Context.MainHandler.GuildTagHandler(Context.Guild).containsTag(tag))
                {
                    await ReplyAsync("Tag not found.");
                    return;
                }
                Tag _tag = Context.MainHandler.GuildTagHandler(Context.Guild).getTag(tag);
                EmbedBuilder eb = new EmbedBuilder();
                IUser user = await Context.Client.GetUserAsync(_tag.creator);
                eb.Title = $"Tag: {_tag.tag}";
                eb.Color = Utils.getRandomColor();
                eb.Footer = new EmbedFooterBuilder().WithText($"Created by: {user.Username}#{user.Discriminator}").WithIconUrl(user.AvatarUrl);
                eb.Timestamp = _tag.when;
                string text = "";
                Match m;
                if (Regex.Match(_tag.text, "(https?://)?((www.)?youtube.com|youtu.?be)/.+").Success)
                {
                    eb = null;
                    text = _tag.text;
                }
                else if ((m = Regex.Match(_tag.text, "(http)?s?:?(//[^\"']*\\.(?:png|jpg|jpeg|gif|png|svg))")).Success)
                {
                    eb.ImageUrl = m.Captures[0].Value;
                    eb.Description = _tag.text;
                }
                else
                    eb.Description = _tag.text;
                await ReplyAsync(text, false, eb);
            }

            [Command("create")]
            [Summary("Create a new tag")]
            public async Task Create([Required] string tag = null, [Required, Remainder] string text = null)
            {
                if (Context.MainHandler.GuildTagHandler(Context.Guild).containsTag(tag) || tag.Equals("create",StringComparison.OrdinalIgnoreCase) || tag.Equals("delete", StringComparison.OrdinalIgnoreCase) || tag.Equals("edit", StringComparison.OrdinalIgnoreCase) || tag.Equals("info", StringComparison.OrdinalIgnoreCase) || tag.Equals("help", StringComparison.OrdinalIgnoreCase))
                {
                    await ReplyAsync("This tag already exists.");
                    return;
                }
                if (text.Length > 1900)
                {
                    await ReplyAsync("Text exceeds limit (> 1900).");
                    return;
                }
                Context.MainHandler.GuildTagHandler(Context.Guild).createTag(Context.User, tag, text);
                await ReplyAsync($"Tag {tag} created!");
            }

            [Command("delete")]
            [Summary("Delete an existing tag")]
            public async Task Delete([Required] string tag = null)
            {
                if (!Context.MainHandler.GuildTagHandler(Context.Guild).containsTag(tag))
                {
                    await ReplyAsync("Tag not found.");
                    return;
                }
                Tag _tag = Context.MainHandler.GuildTagHandler(Context.Guild).getTag(tag);
                if (!await Context.MainHandler.PermissionHandler.isAdmin(Context.User) && _tag.creator != Context.User.Id)
                {
                    await ReplyAsync("This tag isn't yours.");
                    return;
                }
                Context.MainHandler.GuildTagHandler(Context.Guild).removeTag(tag);
                await ReplyAsync($"Tag {tag} deleted.");
            }

            [Command("edit")]
            [Summary("Edit an existing tag")]
            public async Task Edit([Required] string tag = null, [Required, Remainder] string text = null)
            {
                if (!Context.MainHandler.GuildTagHandler(Context.Guild).containsTag(tag))
                {
                    await ReplyAsync("Tag not found.");
                    return;
                }
                Tag _tag = Context.MainHandler.GuildTagHandler(Context.Guild).getTag(tag);
                if (!await Context.MainHandler.PermissionHandler.isAdmin(Context.User) && _tag.creator != Context.User.Id)
                {
                    await ReplyAsync("This tag isn't yours.");
                    return;
                }
                Context.MainHandler.GuildTagHandler(Context.Guild).editTag(tag, text);
                await ReplyAsync($"Tag {tag} edited.");
            }

            /*[Command("help")] TODO: Remove?
            [Summary("Show the command help")]
            public async Task Help()
            {
                EmbedBuilder eb = new EmbedBuilder();
                eb.Author = new EmbedAuthorBuilder().WithName("Help: ?tag").WithIconUrl("http://i.imgur.com/8X3AIRN.png");
                eb.Color = new Color(100, 10, 200);
                eb.Description = 
                 "```?tag [tag]                 => Show a tag if it exists\n" +
                    "?tag create [tag] [text]   => Create a tag with the text\n" +
                    "?tag edit [tag] [text]     => Edit a tag (replace the text)\n" +
                    "?tag delete [tag]          => Delete a tag\n" +
                    "\n**[!] WARNING: Tag is 1-word only (no spaces allowed)!**";
                await ReplyAsync("", false, eb);
            }*/
        }

        [Command("info")]
        [Summary("Get's the bot's application info")]
        public async Task Info()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            EmbedBuilder eb = new EmbedBuilder();
            IGuildUser bot = await Context.Guild.GetCurrentUserAsync();
            eb.Author = new EmbedAuthorBuilder().WithName(bot.Nickname ?? bot.Username).WithIconUrl(Context.Client.CurrentUser.AvatarUrl);
            eb.ThumbnailUrl = Context.Client.CurrentUser.AvatarUrl;
            eb.Color = Maya.Utils.getRandomColor();
            eb.Description = $"{Format.Bold("Info")}\n" +
                                $"- Author: {application.Owner.Username} (ID {application.Owner.Id})\n" +
                                $"- Library: Discord.Net ({DiscordConfig.Version})\n" +
                                $"- Runtime: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}\n" +
                                $"- Uptime: {(DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss")}\n\n" +

                                $"{Format.Bold("Stats")}\n" +
                                $"- Heap Size: {Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString()} MB\n" +
                                $"- Guilds: {(Context.Client as DiscordSocketClient).Guilds.Count}\n" +
                                $"- Channels: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Channels.Count)}\n" +
                                $"- Users: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Users.Count)}";
            await ReplyAsync("", false, eb);
        }
    }

    [RequireAdmin]
    public class AdminCommands : ModuleCommand
    {
        [Command("eval")]
        [RequireAdmin(AdminLevel.OWNER)]
        public async Task Eval([Required, Remainder] string code = null)
        {
            try
            {
                var references = new List<MetadataReference>();
                var referencedAssemblies = Assembly.GetEntryAssembly().GetReferencedAssemblies();
                foreach (var referencedAssembly in referencedAssemblies)
                    references.Add(MetadataReference.CreateFromFile(Assembly.Load(referencedAssembly).Location));
                var scriptoptions = ScriptOptions.Default.WithReferences(references);
                Globals globals = new Globals { Context = Context };
                object o = await CSharpScript.EvaluateAsync(@"using System;using System.Linq;" + @code, scriptoptions, globals);
                if (o == null)
                    await ReplyAsync("Done!");
                else
                    await ReplyAsync("", false, new EmbedBuilder().WithTitle("Result:").WithDescription(o.ToString()));
            }
            catch (Exception e)
            {
                await ReplyAsync("", false, new EmbedBuilder().WithTitle("Error:").WithDescription($"{e.GetType().ToString()}: {e.Message}\nFrom: {e.Source}"));
            }
        }

        [Command("nickname")]
        public async Task Nickname([Required, Remainder] string nickname = null)
        {
            if (nickname == "null")
                await (await Context.Guild.GetCurrentUserAsync()).ModifyAsync(x => x.Nickname = null);
            else
                await (await Context.Guild.GetCurrentUserAsync()).ModifyAsync(x => x.Nickname = nickname);
            await ReplyAsync("Nickname changed!");
        }

        [Command("avatar")]
        public async Task Avatar([Required, Remainder] string url = null)
        {
            MemoryStream imgStream = null;
            try
            {
                using (var http = new HttpClient())
                {
                    using (var sr = await http.GetStreamAsync(url))
                    {
                        imgStream = new MemoryStream();
                        await sr.CopyToAsync(imgStream);
                        imgStream.Position = 0;
                    }
                }
                await Context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image(imgStream));
                await ReplyAsync("Avatar changed!");
            }
            catch (Exception)
            {
                await ReplyAsync("Something went wrong while downloading the image.");
                return;
            }
        }

        [Command("変化の術")]
        public async Task Henge([Required, Remainder] IGuildUser user = null)
        {
            MemoryStream imgStream = null;
            try
            {
                using (var http = new HttpClient())
                {
                    using (var sr = await http.GetStreamAsync(user.AvatarUrl))
                    {
                        imgStream = new MemoryStream();
                        await sr.CopyToAsync(imgStream);
                        imgStream.Position = 0;
                    }
                }
            }
            catch (Exception)
            {
                await ReplyAsync("Something went wrong while downloading the image.");
                return;
            }
            await Context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image(imgStream));
            await (await Context.Guild.GetCurrentUserAsync()).ModifyAsync(x => x.Nickname = user.Nickname ?? user.Username);
            await ReplyAsync("変化の術!");
        }

        [Command("forcesave")]
        [RequireContext(ContextType.Guild)]
        public async Task Forcesave()
        {
            await Context.MainHandler.GuildTitleHandler(Context.Guild).saveTitles();
            await Context.MainHandler.GuildTagHandler(Context.Guild).saveTags();
            await Context.MainHandler.GuildDatabaseHandler(Context.Guild).save();
            await ReplyAsync("Saved tags, titles, and DB.");
        }

        [Command("reload")]
        public async Task Reload([Required("[shipcache/ships/shipsplayers/answers/config/guild/localconfig/localdb/localmusic]"), Remainder] string what = null)
        {
            switch (what)
            {
                case "shipcache":
                    {
                        Context.MainHandler.ShipHandler.torpedoes_cache.Clear();
                        break;
                    }
                case "ships":
                    {
                        await Context.MainHandler.ShipHandler.Initialize();
                        break;
                    }
                case "shipsplayers":
                    {
                        Context.MainHandler.ShipHandler.shipPlayers.Clear();
                        break;
                    }
                case "answers":
                    {
                        Context.MainHandler.TextHandler.resetInterferenceTime();
                        //await Answers.initialize(); TODO: Here...
                        break;
                    }
                case "config":
                    {
                        await Context.MainHandler.ConfigHandler.load();
                        break;
                    }
                case "guild":
                    {
                        if (!(Context.Channel is ITextChannel))
                        {
                            await ReplyAsync("You aren't in a guild channel!");
                            return;
                        }
                        await Context.MainHandler.ReloadGuild((Context.Channel as ITextChannel).Guild as SocketGuild);
                        break;
                    }
                case "localconfig":
                    {
                        if (!(Context.Channel is ITextChannel))
                        {
                            await ReplyAsync("You aren't in a guild channel!");
                            return;
                        }
                        await Context.MainHandler.GuildConfigHandler(Context.Guild).load();
                        break;
                    }
                case "localdb":
                    {
                        if(!(Context.Channel is ITextChannel))
                        {
                            await ReplyAsync("You aren't in a guild channel!");
                            return;
                        }
                        await Context.MainHandler.GuildDatabaseHandler(Context.Guild).load();
                        break;
                    }
                case "localmusic":
                    {
                        if (!(Context.Channel is ITextChannel))
                        {
                            await ReplyAsync("You aren't in a guild channel!");
                            return;
                        }
                        await Context.MainHandler.GuildMusicHandler(Context.Guild).Reset();
                        break;
                    }
                default:
                    {
                        await ReplyAsync("Option not found.");
                        return;
                    }
            }
            await ReplyAsync("Reloaded!");
        }

        [Command("personality")]
        [RequireContext(ContextType.Guild)]
        public async Task Personality([Required] string name = null, bool change_avatar = false)
        {
            if (!Context.MainHandler.GuildPersonalityHandler(Context.Guild).hasPersonality(name))
            {
                await ReplyAsync("Personality not found!");
                return;
            }
            await Context.MainHandler.GuildPersonalityHandler(Context.Guild).loadPersonality(name);
            if(change_avatar)
            {
                String url = Context.MainHandler.GuildPersonalityHandler(Context.Guild).getAvatarUrl();
                MemoryStream imgStream = null;
                try
                {
                    using (var http = new HttpClient())
                    {
                        using (var sr = await http.GetStreamAsync(url))
                        {
                            imgStream = new MemoryStream();
                            await sr.CopyToAsync(imgStream);
                            imgStream.Position = 0;
                        }
                    }
                    await Context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image(imgStream));
                }
                catch (Exception)
                {
                    await ReplyAsync("Something went wrong while downloading the image.");
                }
            }
            await ReplyAsync($"Personality '{name}' loaded!");
        }

        [Command("ignore")]
        [RequireContext(ContextType.Guild)]
        public async Task Ignore([Required, Remainder] IUser user = null)
        {
            if(Context.MainHandler.GuildIgnoreHandler(Context.Guild).contains(user.Id))
            {
                await ReplyAsync($"{user.Username}#{user.Discriminator} is already being ignored.");
                return;
            }
            Context.MainHandler.GuildIgnoreHandler(Context.Guild).add(user.Id);
            await ReplyAsync($"Ignoring {user.Username}#{user.Discriminator}...");
        }

        [Command("unignore")]
        [RequireContext(ContextType.Guild)]
        public async Task Unignore([Required, Remainder] IUser user = null)
        {
            if (!Context.MainHandler.GuildIgnoreHandler(Context.Guild).contains(user.Id))
            {
                await ReplyAsync($"{user.Username}#{user.Discriminator} isn't being ignored.");
                return;
            }
            Context.MainHandler.GuildIgnoreHandler(Context.Guild).remove(user.Id);
            await ReplyAsync($"Unignoring {user.Username}#{user.Discriminator}...");
        }

        [Command("voice", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        public async Task Voice([Required, Remainder] IVoiceChannel channel = null)
        {
            await Context.MainHandler.GuildMusicHandler(Context.Guild).JoinVoiceChannel(channel);
            await ReplyAsync($"Joining {channel.Name}...");
        }
    }

    public class WowsCommands : ModuleCommand
    {
        [Command("ship")]
        [Summary("Show information about a ship")]
        public async Task Ship([Required, Remainder] string ship = null)
        {
            if (Context.Message.MentionedUserIds.Count == 1)
            {
                //ship player
                ulong s1 = Context.Message.MentionedUserIds.First();
                if (Context.MainHandler.ShipHandler.shipPlayers.ContainsKey(s1))
                {
                    ShipPlayer sp = Context.MainHandler.ShipHandler.shipPlayers[s1];
                    if (((TimeSpan)(DateTime.Now - sp.date)).Days < 1)
                    {
                        await ReplyAsync($"Ship: <@{s1}>x<@{sp.userId}>");
                        return;
                    }
                }
                var users = await Context.Guild.GetUsersAsync();
                if (users.Count(x => x.Id == s1) == 1 && users.Count > 2)
                {
                    Random ra = new Random();
                    ulong s2 = s1;
                    while (s2 == s1)
                        s2 = users.ElementAt(ra.Next(users.Count)).Id;
                    Context.MainHandler.ShipHandler.shipPlayers[s1] = new ShipPlayer(s2);
                    await ReplyAsync($"Ship: <@{s1}>x<@{s2}>");
                    return;
                }
            }
            if (Context.MainHandler.ShipHandler.isReady() == null)
            {
                await ReplyAsync("It wasn't possible to establish a connection to the ship database.");
                return;
            }
            if (!Context.MainHandler.ShipHandler.isReady().GetValueOrDefault())
            {
                await ReplyAsync("Loading ship database.");
                return;
            }
            List<IShip> r = Context.MainHandler.ShipHandler.searchShips(ship);
            if (r.Count() == 0)
                await ReplyAsync("No ships found with or containing that name.");
            else if (r.Count() > 1)
            {
                string r_string = r[0].getName();
                for (int i = 1; i < r.Count(); i++)
                    r_string += ", " + r[i].getName();
                await ReplyAsync("More than 1 result found: " + r_string);
            }
            else
            {
                IMessage m = await ReplyAsync("Processing...");
                String ss = await r[0].getSimpleStats();
                await m.DeleteAsync();
                EmbedBuilder eb = new EmbedBuilder();
                eb.Title = r[0].getHeadStats();
                eb.Color = Utils.getRandomColor();
                eb.Description = ss;
                eb.ThumbnailUrl = r[0].getImageUrl();
                eb.Footer = new EmbedFooterBuilder().WithText("[!] Warning: WoWS ship profile is broken (stock values everywhere)!");
                await ReplyAsync("", false, eb);
            }
            return;
        }

        [Command("ships")]
        [Summary("Show all ships available")]
        public async Task Ships()
        {
            if (Context.MainHandler.ShipHandler.isReady() == null)
            {
                await ReplyAsync("It wasn't possible to establish a connection to the ship database.");
                return;
            }
            if (!Context.MainHandler.ShipHandler.isReady().GetValueOrDefault())
            {
                await ReplyAsync("Loading ship database.");
                return;
            }
            await ReplyAsync($"Ship list: {String.Join(", ", Context.MainHandler.ShipHandler.getShipList())}");
        }

        [Command("wtr")]
        [Summary("Show the WTR from a user")]
        public async Task Wtr([Required, Remainder] string username = null)
        {
            if (Regex.Match(username, "^[a-zA-Z0-9_]{3,24}$") == Match.Empty)
            {
                await ReplyAsync("Invalid username.");
                return;
            }
            IMessage m = await ReplyAsync("Processing...");
            string account_id = null;
            using (var httpClient = new HttpClient())
            {
                var jsonraw = await httpClient.GetStringAsync($"https://api.worldofwarships.com/wows/account/list/?application_id=ca60f30d0b1f91b195a521d4aa618eee&type=startswith&limit=1&search={username}");
                JObject json = JObject.Parse(jsonraw);
                if ((String)json["status"] != "ok")
                {
                    await m.DeleteAsync();
                    await ReplyAsync("Something went wrong with the WoWS API.");
                    return;
                }
                JArray data = (JArray)json["data"];
                if (data.Count() == 0)
                {
                    await m.DeleteAsync();
                    await ReplyAsync("No results found.");
                    return;
                }
                JObject result = (JObject)data[0];
                account_id = (String)result["account_id"];
            }
            try
            {
                using (var image = new HttpClient())
                {
                    string link = $"http://na.warshipstoday.com/signature/{account_id}/light.png";
                    File.WriteAllBytes($"Temp{Path.DirectorySeparatorChar}{account_id}.png", await image.GetByteArrayAsync(link));
                }
            }
            catch (Exception)
            {
                await m.DeleteAsync();
                await ReplyAsync("Something went wrong while downloading the image.");
                return;
            }
            await Task.Delay(500);
            await m.DeleteAsync();
            await Context.Channel.SendFileAsync($"Temp{Path.DirectorySeparatorChar}{account_id}.png");
            await Task.Delay(500);
            File.Delete($"Temp{Path.DirectorySeparatorChar}{account_id}.png");
        }
    }
    
    [MusicContext]
    public class MusicCommands : ModuleCommand
    {
        [Command("play", RunMode = RunMode.Async)]
        [Summary("Request to add a song to the music queue")]
        public async Task Play([Required("[search terms/video url/video id]"), Remainder] string search = null)
        {
            MusicContext context = new MusicContext(Context);
            MusicResult mr = Context.MainHandler.GuildMusicHandler(Context.Guild).canUserAddToQueue(context, false);
            if(!mr.IsSuccessful)
            {
                await ReplyAsync(mr.Error);
                return;
            }
            await Context.MainHandler.GuildMusicHandler(Context.Guild).Search(context, search);
        }

        [Command("nowplaying")]
        [Alias("np")]
        [Summary("Show the information about the current song")]
        public async Task Nowplaying()
        {
            MusicContext current = Context.MainHandler.GuildMusicHandler(Context.Guild).getCurrentSong();
            if (current == null)
                await ReplyAsync("No song playing right now.");
            else
            {
                EmbedBuilder eb = new EmbedBuilder();
                eb.Title = "**Now playing**";
                eb.Color = Utils.getRandomColor();
                eb.ThumbnailUrl = $"http://img.youtube.com/vi/{current.Song.VideoId}/mqdefault.jpg";
                eb.Description = $"[**{current.Song.Title}**](https://www.youtube.com/watch?v={current.Song.VideoId})";
                eb.Description += $"\n**Duration**: ``[{current.Song.timePlaying()}/{current.Song.Duration.GetValueOrDefault().ToString(@"mm\:ss")}]``";
                if (current.AskedBy != null)
                    eb.Description += $"- **Requested by**: {current.AskedBy.Nickname ?? current.AskedBy.Username}";
                await ReplyAsync("", false, eb);
            }
        }

        [Command("queue")]
        [Summary("Show the information about the current song and the song queue")]
        public async Task Queue()
        {
            EmbedBuilder eb = new EmbedBuilder();
            eb.Color = Utils.getRandomColor();
            MusicContext current = Context.MainHandler.GuildMusicHandler(Context.Guild).getCurrentSong();
            if (current != null)
            {
                eb.Title = "**Now playing**";
                eb.ThumbnailUrl = $"http://img.youtube.com/vi/{current.Song.VideoId}/mqdefault.jpg";
                eb.Description = $"[**{current.Song.Title}**](https://www.youtube.com/watch?v={current.Song.VideoId})";
                eb.Description += $"\n**Duration**: ``[{current.Song.timePlaying()}/{current.Song.Duration.GetValueOrDefault().ToString(@"mm\:ss")}]``";
                if (current.AskedBy != null)
                    eb.Description += $"- **Requested by**: {current.AskedBy.Nickname ?? current.AskedBy.Username}";
            }
            int n = 1;
            var queue = Context.MainHandler.GuildMusicHandler(Context.Guild).getMusicQueue().getQueue();
            if (queue.Count != 0)
                eb.AddField(efb =>
                {
                    efb.IsInline = true;
                    efb.Name = "**Queue**";
                    efb.Value = "";
                    foreach (MusicContext m in queue)
                    {
                        efb.Value += $"{(n != 1 ? "\n" : "")}``{n}-`` {m.Song.Title} ``[{m.Song.Duration.GetValueOrDefault().ToString(@"mm\:ss")}]``";
                        if (m.AskedBy != null)
                            efb.Value += $" - Requested by {m.AskedBy.Nickname ?? m.AskedBy.Username}";
                        n++;
                    }
                });
            if (current == null && n == 1)
                await ReplyAsync("Nothing playing or in the queue. =(");
            else
                await ReplyAsync("", false, eb);
        }

        [Command("volume", RunMode = RunMode.Async)]
        [Summary("Change the volume from the bot")]
        [RequireAdmin]
        public async Task Volume([Required("[volume(0-100)]")] int vol)
        {
            if (vol < 0 || vol > 100)
            {
                await ReplyAsync("The volume needs to be between 0 and 100.");
                return;
            }
            Context.MainHandler.GuildMusicHandler(Context.Guild).ChangeVolume(vol);
            await ReplyAsync($"Changed volume to {vol}%...");
        }

        [Command("skip", RunMode = RunMode.Async)]
        [Summary("Skip the current song")]
        [RequireAdmin]
        public async Task Skip()
        {
            await ReplyAsync("Skipping...");
            Context.MainHandler.GuildMusicHandler(Context.Guild).Skip();
        }

        [Command("stop", RunMode = RunMode.Async)]
        [Summary("Skip the current song and erase the song queue")]
        [RequireAdmin]
        public async Task Stop()
        {
            await ReplyAsync("Stopping...");
            Context.MainHandler.GuildMusicHandler(Context.Guild).Stop();
        }
    }

    public class PlayerCommands : ModuleCommand
    {
        [Command("marie")]
        [Summary("Special command for marie_splatoon")]
        public async Task Marie()
        {
            await ReplyAsync("Did you mean: boobs?  /人◕‿‿◕人\\");
        }

        [Command("khaenn"), Alias("khaen", "khaeen35")]
        [Summary("Special command for Khaenn35")]
        public async Task Khaen()
        {
            await ReplyAsync("http://i.imgur.com/zLNEnPy.png");
        }

        [Command("kt"), Alias("ktcraft", "serrith")]
        [Summary("Special command for KTcraft")]
        public async Task Kt()
        {
            await ReplyAsync("http://i.imgur.com/UeJ0dqC.png");
        }

        [Command("bb60"), Alias("battleship_60")]
        [Summary("Special command for Battleship_60")]
        public async Task Bb60()
        {
            await ReplyAsync("* spams chat internally *");
        }

        [Command("sparks"), Alias("nightmare_fluttershy")]
        [Summary("Special command for Nightmare_Fluttershy")]
        public async Task Sparks()
        {
            await ReplyAsync("http://i.imgur.com/fEM0XAm.png");
        }

        [Command("flieger"), Alias("flieger56")]
        [Summary("Special command for Flieger56")]
        public async Task Flieger()
        {
            await ReplyAsync("http://i.imgur.com/Pw2K8y4.png");
        }
    }

    public class SimpleCommands : ModuleCommand
    {
        [Command("hello"), Alias("hi", "hai")]
        [Summary("Says Hi")]
        public async Task Hi()
        {
            await ReplyAsync("Hi!");
        }

        [Command("onigiri")]
        [Summary("Show image of a onigiri")]
        public async Task Onigiri()
        {
            await ReplyAsync("", false, new EmbedBuilder().WithImageUrl("http://i.imgur.com/Rixlk2Y.png"));
        }

        [Command("wat")]
        [Summary("Show image of wat grandma")]
        public async Task Wat()
        {
            await ReplyAsync("", false, new EmbedBuilder().WithImageUrl("http://i.imgur.com/tAEmtB2.jpg"));
        }

        [Command("lewd")]
        [Summary("Show image of 'That's lewd'")]
        public async Task Lewd()
        {
            await ReplyAsync("", false, new EmbedBuilder().WithImageUrl("http://i.imgur.com/5VsznpP.png"));
        }

        [Command("wtf")]
        [Summary("Did you mean: wtr?")]
        public async Task Wtf()
        {
            await ReplyAsync("Did you mean: ?wtr?");
        }

        [Command("hug")]
        [Summary("Send a message saying * hugs *")]
        public async Task Hug()
        {
            await ReplyAsync($"* hugs {Context.User.Mention} * (✿◠‿◠)");
        }

        [Command("credits")]
        [Summary("Show the credits for creating the bot")]
        public async Task Credits()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            EmbedBuilder eb = new EmbedBuilder();
            IGuildUser bot = await Context.Guild.GetCurrentUserAsync();
            eb.Author = new EmbedAuthorBuilder().WithName("Credits").WithIconUrl(Context.Client.CurrentUser.AvatarUrl);
            eb.ThumbnailUrl = application.Owner.AvatarUrl;
            eb.Color = Maya.Utils.getRandomColor();
            eb.Description = $"Created by: {application.Owner.Mention}\n" +
                             $"Suggestions? Tell him. :smile:";
            await ReplyAsync("", false, eb);
        }
    }

    public class HelpCommand : ModuleCommand
    {
        [Command("help")] //TODO: Provavelmente reestruturar tudo para usar os sumários etc
        [Summary("Shows the help command")]
        public async Task Help()
        {
            string name = Context.MainHandler.Client.CurrentUser.Username;
            if (Context.Channel is ITextChannel)
            {
                if (!Context.MainHandler.GuildPersonalityHandler(Context.Guild).isReady())
                {
                    await ReplyAsync("Loading...");
                    return;
                }
                name = Context.MainHandler.GuildPersonalityHandler(Context.Guild).getName().ToLower();
            }
            await ReplyAsync("```Simple: ?hello, ?wat, ?lewd, ?onigiri, ?hug, ?swearjar, ?credits\n" +
                                "General: ?marry, ?mywtr, ?kill, ?meaning, ?tag, ?rate, ?8ball, ?joke, ?quote, ?title, ?image\n" +
                                "WoWS: ?ships, ?ship, ?wtr\n" +
                               $"Player-specific: ?{name}, ?marie, ?khaen, ?kt, ?bb60, ?sparks, ?flieger\n" +
                                "Music commands: ?play, ?skip, ?stop, ?volume, ?np, ?queue```");
        }

        [Command("admin")] //TODO: Provavelmente reestruturar para encontrar automatico
        [RequireAdmin]
        public async Task Admin()
        {
            await ReplyAsync("```Commands: ?eval, ?ignore, ?unignore, ?reload, ?voice, ?変化の術, ?avatar, ?nickname, ?forcesave, ?personality, ?resetmusic```");
        }
    }
}
