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
using System.Globalization;
using Maya.Music;
using Maya.ModulesAddons;
using Maya.GuildHandlers;

namespace Maya.Modules.Commands
{
    [Name("General")]
    public class GeneralCommands : ModuleBase<MayaCommandContext>
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
                eb.Color = Utils.GetRandomColor();
                eb.Description = $"[Image link]({(string)image["contentUrl"]})";
                eb.ImageUrl = (string)image["contentUrl"];
                await ReplyAsync("", false, eb);
            }
        }

        [Group("title")]
        [Summary("Main command for adding, deleting, and viewing titles")]
        [RequireContext(ContextType.Guild)]
        public class TitleModule : ModuleBase<MayaCommandContext>
        {
            [Command]
            [Summary("Show titles")]
            public async Task Title(IGuildUser user = null)
            {
                if (user == null)
                    user = Context.User as IGuildUser;
                List<string> titles = Context.MainHandler.GuildTitleHandler(Context.Guild).GetTitles(user);
                if (titles.Count == 0)
                {
                    await ReplyAsync($"**{user.Nickname ?? user.Username}** doesn't have any titles.");
                    return;
                }
                EmbedBuilder eb = new EmbedBuilder();
                eb.Title = $"{user.Nickname ?? user.Username}'s Titles";
                eb.Color = Utils.GetRandomColor();
                eb.Description = $"『{String.Join("』 『", titles)}』";
                await ReplyAsync("", false, eb);
            }

            [Command("add"), Priority(1)]
            [Summary("Add a new title")]
            [RequireAdmin]
            public async Task Add([Required] IGuildUser user = null, [Required, Remainder] string title = null)
            {
                if (Context.MainHandler.GuildTitleHandler(Context.Guild).ContainsTitle(user, title))
                {
                    await ReplyAsync($"{user.Nickname ?? user.Username} already has 『{title}』.");
                    return;
                }
                if (title.Length > 150)
                {
                    await ReplyAsync("Title exceeds length limit (> 150).");
                    return;
                }
                Context.MainHandler.GuildTitleHandler(Context.Guild).AddTitle(user, title);
                await ReplyAsync($"❕ **{user.Nickname ?? user.Username}** got a new title! 『{title}』!");
            }

            [Command("delete"), Priority(1)]
            [Summary("Delete an existing title")]
            [RequireAdmin]
            public async Task Delete([Required] IGuildUser user = null, [Required, Remainder] string title = null)
            {
                if (!Context.MainHandler.GuildTitleHandler(Context.Guild).ContainsTitle(user, title))
                {
                    await ReplyAsync("Title not found.");
                    return;
                }
                Context.MainHandler.GuildTitleHandler(Context.Guild).RemoveTitle(user, title);
                await ReplyAsync($"❕ **{user.Nickname ?? user.Username}** lost the title 『{title}』!");
            }
        }

        [Command("choose")]
        [Summary("Choose one of the choices")]
        public async Task Choose([Required] params string[] choices)
        {
            await ReplyAsync(choices[new Random().Next(choices.Length)]);
        }

        [Command("swearjar")]
        [Summary("Show how much money the swear jar currently has")]
		[RequireContext(ContextType.Guild)]
        [RequireSwearjar]
        public async Task Swearjar()
        {
            if (!Context.MainHandler.GuildDatabaseHandler(Context.Guild).IsReady())
            {
                await ReplyAsync("Loading...");
                return;
            }
            await ReplyAsync($"The swear jar currently has {Convert.ToDecimal(Context.MainHandler.GuildDatabaseHandler(Context.Guild).GetSwearJar()).ToString("C", new CultureInfo("en-US"))}.");
        }

        [Command("marry")]
        [Summary("Make a proposal")]
        [RequireContext(ContextType.Guild)]
        [Cooldown]
        public async Task Marry()
        {
            if (!Context.MainHandler.GuildPersonalityHandler(Context.Guild).IsReady())
            {
                await ReplyAsync("Loading...");
                return;
            }
            await ReplyAsync(Utils.GetRandomWeightedChoice(Context.MainHandler.GuildPersonalityHandler(Context.Guild).GetMarryAnswers()));
        }

        [Command("kill"), Priority(1)]
        [Summary("Stabs someone or something")]
        [RequireContext(ContextType.Guild)]
        public async Task Kill([Required, Remainder] IGuildUser who = null)
        {
            if (!Context.MainHandler.GuildPersonalityHandler(Context.Guild).IsReady())
            {
                await ReplyAsync("Loading...");
                return;
            }
            if (who.Id == (await Context.Client.GetApplicationInfoAsync()).Owner.Id && Context.User.Id == (await Context.Client.GetApplicationInfoAsync()).Owner.Id)
                await ReplyAsync("I can't kill you :(");
            else if (who.Id == (await Context.Client.GetApplicationInfoAsync()).Owner.Id && Context.User.Id != (await Context.Client.GetApplicationInfoAsync()).Owner.Id)
                await ReplyAsync("I can't kill Paulo...\n* stabs you *");
            else if (who.Id == Context.Client.CurrentUser.Id && await Context.MainHandler.PermissionHandler.IsAtLeastAsync(Context.User, AdminLevel.OWNER))
            {
                await ReplyAsync(Context.MainHandler.GuildPersonalityHandler(Context.Guild).GetOfflineString());
                await Task.Delay(1000);
                Environment.Exit(0);
            }
            else if (who.Id == Context.Client.CurrentUser.Id)
                await ReplyAsync("I won't kill myself...\n* stabs you *");
            else if (who.Id == Context.User.Id)
                await ReplyAsync($"It's my pleasure! ^-^\n* stabs {Context.User.Mention} *");
            else
                await ReplyAsync($"It's my pleasure! ^-^\n* stabs {who.Mention} *");
        }

        [Command("kill")]
        [Summary("Stabs someone or something")]
        [RequireContext(ContextType.Guild)]
        public async Task Kill([Required, Remainder] string who = null)
        {
            if (!Context.MainHandler.GuildPersonalityHandler(Context.Guild).IsReady())
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
            else if (who.Equals(Context.MainHandler.GuildPersonalityHandler(Context.Guild).GetName(), StringComparison.OrdinalIgnoreCase) && await Context.MainHandler.PermissionHandler.IsAtLeastAsync(Context.User, AdminLevel.ADMIN))
            {
                await ReplyAsync(Context.MainHandler.GuildPersonalityHandler(Context.Guild).GetOfflineString());
                await Task.Delay(1000);
                Environment.Exit(0);
            }
            else if (who.Equals("myself", StringComparison.OrdinalIgnoreCase) || who == Context.MainHandler.GuildPersonalityHandler(Context.Guild).GetName().ToLower())
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
            using (var typing = Context.Channel.EnterTypingState())
            {
                using (var httpClient = new HttpClient())
                {
                    int isImg = new Random().Next(2);
                    string link = $"http://www.amazingjokes.com/{(isImg == 1 ? "image" : "jokes")}/random";
                    var res = await httpClient.GetAsync(link);
                    if (!res.IsSuccessStatusCode)
                    {
                        await ReplyAsync($"An error occurred: {res.ReasonPhrase}");
                        typing.Dispose();
                        return;
                    }
                    string html = await res.Content.ReadAsStringAsync();
                    EmbedBuilder eb = new EmbedBuilder();
                    if (isImg == 1)
                    {
                        string[] ps = html.Split(new string[] { "og:title\" content=\"" }, StringSplitOptions.None)[1].Split(new string[] { "\">" }, StringSplitOptions.None);
                        eb.Author = new EmbedAuthorBuilder().WithName($"{ps[0]}");
                        eb.ImageUrl = ps[1].Split(new string[] { "og:image\" content=\"" }, StringSplitOptions.None)[1].Split('"')[0];
                    }
                    else
                    {
                        string[] ps = html.Split(new string[] { "<div class=\"thumbnail\">" }, StringSplitOptions.None)[1].Split(new string[] { "\">" }, StringSplitOptions.None)[1].Split(new string[] { "</h1>" }, StringSplitOptions.None);
                        eb.Author = new EmbedAuthorBuilder().WithName($"{ps[0]}");
                        eb.Description = Utils.StripTags(WebUtility.HtmlDecode(ps[1].Split(new string[] { " </div>" }, StringSplitOptions.None)[0].Trim()));
                    }
                    await ReplyAsync("", false, eb);
                }
            }
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
            eb.Description = $"**Question: {question}**\n{ans[new Random().Next(ans.Length)]}";
            await ReplyAsync("", false, eb);
        }

        [Command("rate")]
        [Summary("Rate what you typed")]
        public async Task Rate([Required, Remainder] string text = null)
        {
            await ReplyAsync($"🤔 I would rate '{text}' a {new Random().Next(11)}/10");
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
                string result;
                if (html.Contains("<div class=\"meaning\">"))
                    result = html.Split(new string[] { "<div class=\"meaning\">" }, StringSplitOptions.None)[1].Split(new string[] { "</div>" }, StringSplitOptions.None)[0];
                else
                    result = html.Split(new string[] { "<div class='meaning'>" }, StringSplitOptions.None)[1].Split(new string[] { "</div>" }, StringSplitOptions.None)[0];
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
                eb.Title = $"Meaning: {text}";//TODO: Add urban img?
                eb.Color = Utils.GetRandomColor();
                eb.Description = output;
                await ReplyAsync("", false, eb);
            }
        }

        [Command("mywtr")]
        [Summary("Show a random answer to how good you are")]
        [RequireContext(ContextType.Guild)]
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
            await ReplyAsync(Utils.GetRandomWeightedChoice(a));
        }

        [Command("quote")]
        [Summary("Quote user messages")]
        public async Task Quote([Required] params string[] message_ids)
        {
            List<IMessage> msgs = new List<IMessage>();
            IUser author = null;
            foreach (string ids in message_ids)
            {
                try
                {
                    ulong id = ulong.Parse(ids);
                    IMessage m = await Context.Channel.GetMessageAsync(id);
                    if (m.Content.Length == 0)
                    {
                        await ReplyAsync($"The message '{m.Id}' doesn't have any text.");
                        return;
                    }
                    if (author == null)
                        author = m.Author;
                    else if (author != m.Author)
                    {
                        await ReplyAsync($"The message '{m.Id}' doesn't belong to the same user ({author.Username}).");
                        return;
                    }
                    msgs.Add(m);
                }
                catch (Exception) { }
            }
            if (msgs.Count == 0)
            {
                await ReplyAsync("No messages to quote.");
                return;
            }
            DateTime older, newer;
            older = newer = msgs.First().Timestamp.DateTime;
            foreach (IMessage m in msgs.Skip(1))
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
            eb.Author = new EmbedAuthorBuilder().WithName($"{author.Username}#{author.Discriminator}").WithIconUrl(author.GetAvatarUrl());
            eb.Color = Utils.GetRandomColor();
            eb.Description = String.Join("\n", ordered_msgs.Select(x => x.Content));
            eb.Timestamp = older;
            await ReplyAsync("", false, eb);
        }

        [Group("tag")]
        [Summary("Main command for creating, deleting, editing, and viewing tags")]
        [RequireContext(ContextType.Guild)]
        public class TagModule : ModuleBase<MayaCommandContext>
        {
            [Command]
            [Summary("Show a tag")]
            public async Task Tag([Required("tag/create/edit/delete"), Remainder] string tag = null)
            {
                if (!Context.MainHandler.GuildTagHandler(Context.Guild).ContainsTag(tag))
                {
                    await ReplyAsync("Tag not found.");
                    return;
                }
                Tag _tag = Context.MainHandler.GuildTagHandler(Context.Guild).GetTag(tag);
                EmbedBuilder eb = new EmbedBuilder();
                IUser user = await Context.Client.GetUserAsync(_tag.creator);
                eb.Color = Utils.GetRandomColor();
                eb.Footer = new EmbedFooterBuilder().WithText($"Created by: {user.Username}#{user.Discriminator}").WithIconUrl(user.GetAvatarUrl());
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
                    eb.Description = $"**Tag: {_tag.tag}**\n{_tag.text}";
                }
                else
                    eb.Description = $"**Tag: {_tag.tag}**\n{_tag.text}";
                await ReplyAsync(text, false, eb);
            }

            [Command("create"), Priority(1)]
            [Summary("Create a new tag")]
            public async Task Create([Required] string tag = null, [Required, Remainder] string text = null)
            {
                if (Context.MainHandler.GuildTagHandler(Context.Guild).ContainsTag(tag) || tag.Equals("create", StringComparison.OrdinalIgnoreCase) || tag.Equals("delete", StringComparison.OrdinalIgnoreCase) || tag.Equals("edit", StringComparison.OrdinalIgnoreCase) || tag.Equals("info", StringComparison.OrdinalIgnoreCase) || tag.Equals("help", StringComparison.OrdinalIgnoreCase))
                {
                    await ReplyAsync("This tag already exists.");
                    return;
                }
                if (text.Length > 1900)
                {
                    await ReplyAsync("Text exceeds limit (> 1900).");
                    return;
                }
                Context.MainHandler.GuildTagHandler(Context.Guild).CreateTag(Context.User, tag, text);
                await ReplyAsync($"Tag {tag} created!");
            }

            [Command("delete"), Priority(1)]
            [Summary("Delete an existing tag")]
            public async Task Delete([Required] string tag = null)
            {
                if (!Context.MainHandler.GuildTagHandler(Context.Guild).ContainsTag(tag))
                {
                    await ReplyAsync("Tag not found.");
                    return;
                }
                Tag _tag = Context.MainHandler.GuildTagHandler(Context.Guild).GetTag(tag);
                if (!await Context.MainHandler.PermissionHandler.IsAdminAsync(Context.User) && _tag.creator != Context.User.Id)
                {
                    await ReplyAsync("This tag isn't yours.");
                    return;
                }
                Context.MainHandler.GuildTagHandler(Context.Guild).RemoveTag(tag);
                await ReplyAsync($"Tag {tag} deleted.");
            }

            [Command("edit"), Priority(1)]
            [Summary("Edit an existing tag")]
            public async Task Edit([Required] string tag = null, [Required, Remainder] string text = null)
            {
                if (!Context.MainHandler.GuildTagHandler(Context.Guild).ContainsTag(tag))
                {
                    await ReplyAsync("Tag not found.");
                    return;
                }
                Tag _tag = Context.MainHandler.GuildTagHandler(Context.Guild).GetTag(tag);
                if (!await Context.MainHandler.PermissionHandler.IsAdminAsync(Context.User) && _tag.creator != Context.User.Id)
                {
                    await ReplyAsync("This tag isn't yours.");
                    return;
                }
                Context.MainHandler.GuildTagHandler(Context.Guild).EditTag(tag, text);
                await ReplyAsync($"Tag {tag} edited.");
            }
        }

        [Command("info")]
        [Summary("Get the bot's application info")]
        public async Task Info()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            EmbedBuilder eb = new EmbedBuilder();
            IGuildUser bot = await Context.Guild.GetCurrentUserAsync();
            eb.Author = new EmbedAuthorBuilder().WithName(bot.Nickname ?? bot.Username).WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
            eb.ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl();
            eb.Color = Maya.Utils.GetRandomColor();
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

    [Name("Admin")]
    [RequireAdmin(AdminLevel.MODERATOR)]
    public class AdminCommands : ModuleBase<MayaCommandContext>
    {
        [Command("eval", RunMode = RunMode.Async)]
        [RequireAdmin(AdminLevel.OWNER)]
        public async Task Eval([Required, Remainder] string code = null)
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    var references = new List<MetadataReference>();
                    var referencedAssemblies = Assembly.GetEntryAssembly().GetReferencedAssemblies();
                    foreach (var referencedAssembly in referencedAssemblies)
                        references.Add(MetadataReference.CreateFromFile(Assembly.Load(referencedAssembly).Location));
                    var scriptoptions = ScriptOptions.Default.WithReferences(references);
                    Globals globals = new Globals { Context = Context, Guild = Context.Guild as SocketGuild };
                    object o = await CSharpScript.EvaluateAsync(@"using System;using System.Linq;using System.Threading.Tasks;using Discord.WebSocket;using Discord;" + @code, scriptoptions, globals);
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
        }

        [Command("clean")]
        public async Task Clean(int messages = 30)
        {
            var msgs = await Context.Channel.GetMessagesAsync(messages).Flatten();
            msgs = msgs.Where(x => x.Author.Id == Context.MainHandler.Client.CurrentUser.Id);
            await Context.Channel.DeleteMessagesAsync(msgs);
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
        [RequireAdmin(AdminLevel.ADMIN)]
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
        [RequireAdmin(AdminLevel.ADMIN)]
        public async Task Henge([Required, Remainder] IGuildUser user = null)
        {
            MemoryStream imgStream = null;
            try
            {
                using (var http = new HttpClient())
                {
                    using (var sr = await http.GetStreamAsync(user.GetAvatarUrl()))
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
        [RequireAdmin(AdminLevel.ADMIN)]
        [RequireContext(ContextType.Guild)]
        public async Task Forcesave()
        {
            await Context.MainHandler.GuildTitleHandler(Context.Guild).SaveTitlesAsync();
            await Context.MainHandler.GuildTagHandler(Context.Guild).SaveTagsAsync();
            await Context.MainHandler.GuildDatabaseHandler(Context.Guild).SaveAsync();
            await ReplyAsync("Saved tags, titles, and DB.");
        }

        [Command("reload")]
        [RequireAdmin(AdminLevel.ADMIN)]
        public async Task Reload([Required("shipcache/ships/shipsplayers/answers/config/guild/localconfig/localdb/localmusic"), Remainder] string what = null)
        {
            switch (what)
            {
                case "shipcache":
                    {
                        Context.MainHandler.ShipHandler.ClearCache();
                        break;
                    }
                case "ships":
                    {
                        await Context.MainHandler.ShipHandler.InitializeAsync();
                        break;
                    }
                case "shipsplayers":
                    {
                        Context.MainHandler.ShipHandler.shipPlayers.Clear();
                        break;
                    }
                case "answers":
                    {
                        Context.MainHandler.TextHandler.ResetInterferenceTime();
                        //await Answers.initialize(); TODO: Here...
                        break;
                    }
                case "config":
                    {
                        await Context.MainHandler.ConfigHandler.LoadAsync();
                        break;
                    }
                case "guild":
                    {
                        if (!(Context.Channel is ITextChannel))
                        {
                            await ReplyAsync("You aren't in a guild channel!");
                            return;
                        }
                        await Context.MainHandler.ReloadGuildAsync((Context.Channel as ITextChannel).Guild as SocketGuild);
                        break;
                    }
                case "localconfig":
                    {
                        if (!(Context.Channel is ITextChannel))
                        {
                            await ReplyAsync("You aren't in a guild channel!");
                            return;
                        }
                        await Context.MainHandler.GuildConfigHandler(Context.Guild).LoadAsync();
                        break;
                    }
                case "localdb":
                    {
                        if (!(Context.Channel is ITextChannel))
                        {
                            await ReplyAsync("You aren't in a guild channel!");
                            return;
                        }
                        await Context.MainHandler.GuildDatabaseHandler(Context.Guild).LoadAsync();
                        break;
                    }
                case "localmusic":
                    {
                        if (!(Context.Channel is ITextChannel))
                        {
                            await ReplyAsync("You aren't in a guild channel!");
                            return;
                        }
                        await Context.MainHandler.GuildMusicHandler(Context.Guild).ResetAsync();
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
        [RequireAdmin(AdminLevel.ADMIN)]
        [RequireContext(ContextType.Guild)]
        public async Task Personality([Required] string name = null, bool change_avatar = false)
        {
            if (!Context.MainHandler.GuildPersonalityHandler(Context.Guild).ExistsPersonality(name))
            {
                await ReplyAsync("Personality not found!");
                return;
            }
            await Context.MainHandler.GuildPersonalityHandler(Context.Guild).LoadPersonalityAsync(name);
            if (change_avatar)
            {
                String url = Context.MainHandler.GuildPersonalityHandler(Context.Guild).GetAvatarUrl();
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

        [Command("shutup")]
        [RequireContext(ContextType.Guild)]
        public async Task Shutup()
        {
            Context.MainHandler.GuildPersonalityHandler(Context.Guild).DisablePersonalityMessage();
            await ReplyAsync($":x");
        }

        [Command("ignore")]
        [RequireContext(ContextType.Guild)]
        public async Task Ignore([Required, Remainder] IUser user = null)
        {
            if (Context.MainHandler.GuildIgnoreHandler(Context.Guild).Contains(user.Id))
            {
                await ReplyAsync($"{user.Username}#{user.Discriminator} is already being ignored.");
                return;
            }
            Context.MainHandler.GuildIgnoreHandler(Context.Guild).Add(user.Id);
            await ReplyAsync($"Ignoring {user.Username}#{user.Discriminator}...");
        }

        [Command("unignore")]
        [RequireContext(ContextType.Guild)]
        public async Task Unignore([Required, Remainder] IUser user = null)
        {
            if (!Context.MainHandler.GuildIgnoreHandler(Context.Guild).Contains(user.Id))
            {
                await ReplyAsync($"{user.Username}#{user.Discriminator} isn't being ignored.");
                return;
            }
            Context.MainHandler.GuildIgnoreHandler(Context.Guild).Remove(user.Id);
            await ReplyAsync($"Unignoring {user.Username}#{user.Discriminator}...");
        }

        [Command("voice", RunMode = RunMode.Async)]
        [RequireAdmin(AdminLevel.ADMIN)]
        [RequireContext(ContextType.Guild)]
        public async Task Voice([Required, Remainder] IVoiceChannel channel = null)
        {
            await Context.MainHandler.GuildMusicHandler(Context.Guild).JoinVoiceChannelAsync(channel);
            await ReplyAsync($"Joining {channel.Name}...");
        }
    }

    [Name("WoWS")]
    public class WowsCommands : ModuleBase<MayaCommandContext>
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
            if (Context.MainHandler.ShipHandler.IsReady() == null)
            {
                await ReplyAsync("It wasn't possible to establish a connection to the ship database.");
                return;
            }
            if (!Context.MainHandler.ShipHandler.IsReady().GetValueOrDefault())
            {
                await ReplyAsync("Loading ship database.");
                return;
            }
            if (ship.Length < 2)
            {
                await ReplyAsync("Type at least 2 characters.");
                return;
            }
            List<IShip> r = Context.MainHandler.ShipHandler.SearchShips(ship);
            if (r.Count() == 0)
                await ReplyAsync("No ships found with or containing that name.");
            else if (r.Count() > 1)
            {
                string r_string = r[0].GetName();
                for (int i = 1; i < r.Count(); i++)
                    r_string += ", " + r[i].GetName();
                await ReplyAsync("More than 1 result found: " + r_string);
            }
            else
            {
                using (Context.Channel.EnterTypingState())
                {
                    String ss = await r[0].GetSimpleStatsAsync();
                    EmbedBuilder eb = new EmbedBuilder()
                    {
                        Title = r[0].GetHeadStats(),
                        Color = Utils.GetRandomColor(),
                        Description = ss,
                        ThumbnailUrl = r[0].GetImageUrl()
                    };
                    //eb.Footer = new EmbedFooterBuilder().WithText("[!] Warning: WoWS ship profile is broken (stock values everywhere)!");
                    await ReplyAsync("", false, eb);
                }
            }
        }

        [Command("ships")]
        [Summary("Show all ships available")]
        public async Task Ships()
        {
            if (Context.MainHandler.ShipHandler.IsReady() == null)
            {
                await ReplyAsync("It wasn't possible to establish a connection to the ship database.");
                return;
            }
            if (!Context.MainHandler.ShipHandler.IsReady().GetValueOrDefault())
            {
                await ReplyAsync("Loading ship database.");
                return;
            }
            var shipList = Context.MainHandler.ShipHandler.GetShipNameList();
            var ordShipList = shipList.OrderBy(x => x);
            var eb = new EmbedBuilder();
            eb.Title = $"Ship list ({shipList.Length}) - WILL FIX SOON:tm:";
            eb.AddInlineField("​", String.Join("\n", ordShipList.Take(10)));
            eb.AddInlineField("​", String.Join("\n", ordShipList.Skip(10).Take(10)));
            eb.AddInlineField("​", String.Join("\n", ordShipList.Skip(20).Take(10)));
            eb.Footer = new EmbedFooterBuilder().WithText($"Page 1/{Math.Ceiling(shipList.Length / 30.0)}");
            await ReplyAsync("", embed: eb);
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
            using (var typing = Context.Channel.EnterTypingState())
            {
                string account_id = null;
                using (var httpClient = new HttpClient())
                {
                    var jsonraw = await httpClient.GetStringAsync($"https://api.worldofwarships.com/wows/account/list/?application_id=ca60f30d0b1f91b195a521d4aa618eee&type=startswith&limit=1&search={username}");
                    JObject json = JObject.Parse(jsonraw);
                    if ((String)json["status"] != "ok")
                    {
                        await ReplyAsync("Something went wrong with the WoWS API.");
                        typing.Dispose();
                        return;
                    }
                    JArray data = (JArray)json["data"];
                    if (data.Count() == 0)
                    {
                        await ReplyAsync("No results found.");
                        typing.Dispose();
                        return;
                    }
                    JObject result = (JObject)data[0];
                    account_id = (String)result["account_id"];
                }
                try
                {
                    using (var image = new HttpClient())
                    {
                        string link = $"http://na.warshipstoday.com/signature/{account_id}/light.png"; //TODO: Download and upload at the same time?
                        File.WriteAllBytes($"Temp{Path.DirectorySeparatorChar}{account_id}.png", await image.GetByteArrayAsync(link));
                    }
                }
                catch (Exception)
                {
                    await ReplyAsync("Something went wrong while downloading the image.");
                    typing.Dispose();
                    return;
                }
                await Task.Delay(500);
                await Context.Channel.SendFileAsync($"Temp{Path.DirectorySeparatorChar}{account_id}.png");
                await Task.Delay(500);
                File.Delete($"Temp{Path.DirectorySeparatorChar}{account_id}.png");
            }
        }

        [Command("stats")]
        [Summary("Show the stats from a player")]
        public async Task Stats([Required] string username = null, [Remainder] string ship = null)
        {
            try
            {
                if (Regex.Match(username, "^[a-zA-Z0-9_]{3,24}$") == Match.Empty)
                {
                    await ReplyAsync("Invalid username.");
                    return;
                }
                using (var typing = Context.Channel.EnterTypingState())
                {
                    string account_id = null;
                    string account_name = null;
                    using (var httpClient = new HttpClient())
                    {
                        var jsonraw = await httpClient.GetStringAsync($"https://api.worldofwarships.com/wows/account/list/?application_id=ca60f30d0b1f91b195a521d4aa618eee&type=startswith&limit=1&search={username}");
                        JObject json = JObject.Parse(jsonraw);
                        if ((String)json["status"] != "ok")
                        {
                            await ReplyAsync("Something went wrong with the WoWS API.");
                            typing.Dispose();
                            return;
                        }
                        JArray data = (JArray)json["data"];
                        if (data.Count() == 0)
                        {
                            await ReplyAsync("No results found.");
                            typing.Dispose();
                            return;
                        }
                        JObject result = (JObject)data[0];
                        account_id = (String)result["account_id"];
                        account_name = (String)result["nickname"];
                    }
                    if (ship == null)
                    {
                        //user stats
                        JObject result;
                        using (var httpClient = new HttpClient())
                        {
                            var jsonraw = await httpClient.GetStringAsync($"https://api.worldofwarships.com/wows/account/info/?application_id=ca60f30d0b1f91b195a521d4aa618eee&account_id={account_id}");
                            JObject json = JObject.Parse(jsonraw);
                            if ((String)json["status"] != "ok")
                            {
                                await ReplyAsync("Something went wrong with the WoWS API.");
                                typing.Dispose();
                                return;
                            }
                            JObject data = (JObject)json["data"];
                            result = (JObject)data[$"{account_id}"];
                        }
                        EmbedBuilder eb = new EmbedBuilder();
                        eb.Title = $"Stats of {account_name}";
                        eb.Color = Utils.GetRandomColor();
                        string accInfo = $"**Created at**: {Utils.UnixTimestampToString((string)result["created_at"])}\n" +
                                         $"**Last battle**: {Utils.UnixTimestampToString((string)result["last_battle_time"])}\n" +
                                         $"**Last logout**: {Utils.UnixTimestampToString((string)result["logout_at"])}";
                        eb.AddField("Account info", accInfo);
                        string stats;
                        if ((bool)result["hidden_profile"])
                            stats = "This profile is hidden.";
                        else
                        {
                            JObject statistics = (JObject)result["statistics"];
                            JObject pvp = (JObject)statistics["pvp"];
                            JObject main = (JObject)pvp["main_battery"];
                            JObject secondary = (JObject)pvp["second_battery"];
                            JObject torps = (JObject)pvp["torpedoes"];
                            JObject ram = (JObject)pvp["ramming"];
                            JObject aircraft = (JObject)pvp["aircraft"];
                            IShip bestShipDmg = Context.MainHandler.ShipHandler.GetShipById((string)pvp["max_damage_dealt_ship_id"]);
                            eb.ThumbnailUrl = bestShipDmg?.GetImageUrl();
                            stats = $"**Battles**: {pvp["battles"]} ({pvp["wins"]}W/{pvp["losses"]}L/{pvp["draws"]}T - WR: {Utils.Percentage((float)pvp["wins"], (float)pvp["battles"])} - SR: {Utils.Percentage((float)pvp["survived_battles"], (float)pvp["battles"])})\n" +
                                    $"**Best ships**:\n" +
                                    $"- Damage: {bestShipDmg?.GetName()} ({pvp["max_damage_dealt"]})\n" +
                                    $"- XP: {Context.MainHandler.ShipHandler.GetShipById((string)pvp["max_xp_ship_id"])?.GetName()} ({pvp["max_xp"]})\n" +
                                    $"- Kills: {Context.MainHandler.ShipHandler.GetShipById((string)pvp["max_frags_ship_id"])?.GetName()} ({pvp["max_frags_battle"]})\n" +
                                    $"- Plane kills: {Context.MainHandler.ShipHandler.GetShipById((string)pvp["max_planes_killed_ship_id"])?.GetName()} ({pvp["max_planes_killed"]})\n" +
                                    $"**Main battery**:\n" +
                                    $"- Kills: {main["frags"]} (Max: {main["max_frags_battle"]} with {Context.MainHandler.ShipHandler.GetShipById((string)main["max_frags_ship_id"])?.GetName()})\n" +
                                    $"- Shots: {main["shots"]} (Accuracy: {Utils.Percentage((float)main["hits"], (float)main["shots"])})\n" +
                                    $"**Secondary battery**:\n" +
                                    $"- Kills: {secondary["frags"]} (Max: {secondary["max_frags_battle"]} with {Context.MainHandler.ShipHandler.GetShipById((string)secondary["max_frags_ship_id"])?.GetName()})\n" +
                                    $"- Shots: {secondary["shots"]} (Accuracy: {Utils.Percentage((float)secondary["hits"], (float)secondary["shots"])})\n" +
                                    $"**Torpedoes**:\n" +
                                    $"- Kills: {torps["frags"]} (Max: {torps["max_frags_battle"]} with {Context.MainHandler.ShipHandler.GetShipById((string)torps["max_frags_ship_id"])?.GetName()})\n" +
                                    $"- Shots: {torps["shots"]} (Accuracy: {Utils.Percentage((float)torps["hits"], (float)torps["shots"])})\n" +
                                    $"**Ramming**:\n" +
                                    $"- Kills: {ram["frags"]} (Max: {ram["max_frags_battle"]} with {Context.MainHandler.ShipHandler.GetShipById((string)ram["max_frags_ship_id"])?.GetName()})\n" +
                                    $"**Aircraft**:\n" +
                                    $"- Kills: {aircraft["frags"]} (Max: {aircraft["max_frags_battle"]} with {Context.MainHandler.ShipHandler.GetShipById((string)aircraft["max_frags_ship_id"])?.GetName()})";
                        }
                        eb.AddField("Statistics", stats);
                        eb.Footer = new EmbedFooterBuilder().WithText($"To know about a specific ship, type: {Context.MainHandler.GetCommandPrefix(Context.Channel)}stats {(String)result["nickname"]} [ship]");
                        await ReplyAsync("", embed: eb);
                    }
                    else
                    {
                        //ship stats from user
                        if (Context.MainHandler.ShipHandler.IsReady() == null)
                        {
                            await ReplyAsync("It wasn't possible to establish a connection to the ship database.");
                            typing.Dispose();
                            return;
                        }
                        if (!Context.MainHandler.ShipHandler.IsReady().GetValueOrDefault())
                        {
                            await ReplyAsync("Loading ship database.");
                            typing.Dispose();
                            return;
                        }
                        if (ship.Length < 2)
                        {
                            await ReplyAsync("Type at least 2 characters for the ship.");
                            typing.Dispose();
                            return;
                        }
                        List<IShip> r = Context.MainHandler.ShipHandler.SearchShips(ship);
                        if (r.Count() == 0)
                            await ReplyAsync("No ships found with or containing that name.");
                        else if (r.Count() > 1)
                        {
                            string r_string = r[0].GetName();
                            for (int i = 1; i < r.Count(); i++)
                                r_string += ", " + r[i].GetName();
                            await ReplyAsync("More than 1 result found: " + r_string);
                        }
                        else
                        {
                            JObject result;
                            using (var httpClient = new HttpClient())
                            {
                                var jsonraw = await httpClient.GetStringAsync($"https://api.worldofwarships.com/wows/ships/stats/?application_id=ca60f30d0b1f91b195a521d4aa618eee&account_id={account_id}&ship_id={r[0].GetId()}");
                                JObject json = JObject.Parse(jsonraw);
                                if ((String)json["status"] != "ok")
                                {
                                    await ReplyAsync("Something went wrong with the WoWS API.");
                                    typing.Dispose();
                                    return;
                                }
                                JObject data = (JObject)json["data"];
                                if (!data.HasValues)
                                {
                                    await ReplyAsync("Player data is hidden.");
                                    typing.Dispose();
                                    return;
                                }
                                try { result = (JObject)((JArray)data[$"{account_id}"])[0]; } catch { result = null; }
                            }
                            EmbedBuilder eb = new EmbedBuilder();
                            eb.Title = $"Stats of {account_name}'s {r[0].GetName()}";
                            eb.Color = Utils.GetRandomColor();
                            if (result == null)
                                eb.Description = $"{account_name} doesn't have stats for this ship.";
                            else
                            {
                                eb.ThumbnailUrl = r[0].GetImageUrl();
                                JObject pvp = (JObject)result["pvp"];
                                JObject main = (JObject)pvp["main_battery"];
                                JObject secondary = (JObject)pvp["second_battery"];
                                JObject torps = (JObject)pvp["torpedoes"];
                                JObject ram = (JObject)pvp["ramming"];
                                JObject aircraft = (JObject)pvp["aircraft"];
                                eb.Description = $"**Distance**: {result["distance"]} km\n" +
                                                 $"**Battles**: {pvp["battles"]} ({pvp["wins"]}W/{pvp["losses"]}L/{pvp["draws"]}T - WR: {Utils.Percentage((float)pvp["wins"], (float)pvp["battles"])} - SR: {Utils.Percentage((float)pvp["survived_battles"], (float)pvp["battles"])})\n" +
                                                 $"**Last battle**: {Utils.UnixTimestampToString((string)result["last_battle_time"])}\n" +
                                                 $"**Planes killed**: {pvp["planes_killed"]}\n" +
                                                 $"**Best scores**:\n" +
                                                 $"- Damage: {pvp["max_damage_dealt"]}\n" +
                                                 $"- XP: {pvp["max_xp"]}\n" +
                                                 $"- Kills: {pvp["max_frags_battle"]}\n" +
                                                 $"- Plane kills: {pvp["max_planes_killed"]}\n" +
                                                 $"**Main battery**:\n" +
                                                 $"- Kills: {main["frags"]} (Max: {main["max_frags_battle"]})\n" +
                                                 $"- Shots: {main["shots"]} (Accuracy: {Utils.Percentage((float)main["hits"], (float)main["shots"])})\n" +
                                                 $"**Secondary battery**:\n" +
                                                 $"- Kills: {secondary["frags"]} (Max: {secondary["max_frags_battle"]})\n" +
                                                 $"- Shots: {secondary["shots"]} (Accuracy: {Utils.Percentage((float)secondary["hits"], (float)secondary["shots"])})\n" +
                                                 $"**Torpedoes**:\n" +
                                                 $"- Kills: {torps["frags"]} (Max: {torps["max_frags_battle"]})\n" +
                                                 $"- Shots: {torps["shots"]} (Accuracy: {Utils.Percentage((float)torps["hits"], (float)torps["shots"])})\n" +
                                                 $"**Aircraft**:\n" +
                                                 $"- Kills: {torps["frags"]} (Max: {torps["max_frags_battle"]})\n" +
                                                 $"**Ramming**:\n" +
                                                 $"- Kills: {aircraft["frags"]} (Max: {aircraft["max_frags_battle"]})";
                            }
                            await ReplyAsync("", embed: eb);
                        }
                    }
                }
            }
            catch(Exception e) { Console.WriteLine(e); }
        }
    }

    [Name("Music")]
    [MusicContext]
    public class MusicCommands : ModuleBase<MayaCommandContext>
    {
        [Command("play", RunMode = RunMode.Async)]
        [Summary("Request to add a song to the music queue")]
        public async Task Play([Required("search terms/video url/video id"), Remainder] string search = null)
        {
            MusicContext context = new MusicContext(Context);
            MusicResult mr = Context.MainHandler.GuildMusicHandler(Context.Guild).CanUserAddToQueue(context, false);
            if (!mr.IsSuccessful)
            {
                await ReplyAsync(mr.Error);
                return;
            }
            await Context.MainHandler.GuildMusicHandler(Context.Guild).SearchAsync(context, search);
        }

        [Command("nowplaying")]
        [Alias("np")]
        [Summary("Show the information about the current song")]
        public async Task Nowplaying()
        {
            MusicContext current = Context.MainHandler.GuildMusicHandler(Context.Guild).GetCurrentSong();
            if (current == null)
                await ReplyAsync("No song playing right now.");
            else
            {
                EmbedBuilder eb = new EmbedBuilder();
                eb.Title = "**Now playing**";
                eb.Color = Utils.GetRandomColor();
                eb.ThumbnailUrl = $"http://img.youtube.com/vi/{current.Song.VideoId}/mqdefault.jpg";
                eb.Description = $"[**{current.Song.Title}**](https://www.youtube.com/watch?v={current.Song.VideoId})";
                eb.Description += $"\n**Duration**: ``[{current.Song.GetTimePlaying()}/{current.Song.Duration.GetValueOrDefault().ToString(@"mm\:ss")}]``";
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
            eb.Color = Utils.GetRandomColor();
            MusicContext current = Context.MainHandler.GuildMusicHandler(Context.Guild).GetCurrentSong();
            if (current != null)
            {
                eb.Title = "**Now playing**";
                eb.ThumbnailUrl = $"http://img.youtube.com/vi/{current.Song.VideoId}/mqdefault.jpg";
                eb.Description = $"[**{current.Song.Title}**](https://www.youtube.com/watch?v={current.Song.VideoId})";
                eb.Description += $"\n**Duration**: ``[{current.Song.GetTimePlaying()}/{current.Song.Duration.GetValueOrDefault().ToString(@"mm\:ss")}]``";
                if (current.AskedBy != null)
                    eb.Description += $"- **Requested by**: {current.AskedBy.Nickname ?? current.AskedBy.Username}";
            }
            int n = 1;
            var queue = Context.MainHandler.GuildMusicHandler(Context.Guild).GetMusicQueue().GetQueue();
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
        [RequireAdmin(AdminLevel.MODERATOR)]
        public async Task Volume(Nullable<int> volume = null)
        {
            if (volume == null)
            {
                await ReplyAsync($"Current volume: {Context.MainHandler.GuildMusicHandler(Context.Guild).GetVolume()}%.\nChange it with: {Context.MainHandler.GetCommandPrefix(Context.Channel)}volume [0-100]");
                return;
            }
            if (volume.Value < 0 || volume.Value > 100)
            {
                await ReplyAsync("The volume needs to be a number between 0 and 100.");
                return;
            }
            Context.MainHandler.GuildMusicHandler(Context.Guild).ChangeVolume(volume.Value);
            await ReplyAsync($"Changed volume to {volume.Value}%.");
        }

        [Command("skip", RunMode = RunMode.Async)]
        [Summary("Skip the current song")]
        [RequireAdmin(AdminLevel.MODERATOR)]
        public async Task Skip()
        {
            await ReplyAsync("Skipping...");
            Context.MainHandler.GuildMusicHandler(Context.Guild).Skip();
        }

        [Command("stop", RunMode = RunMode.Async)]
        [Summary("Skip the current song and erase the song queue")]
        [RequireAdmin(AdminLevel.MODERATOR)]
        public async Task Stop()
        {
            await ReplyAsync("Stopping...");
            Context.MainHandler.GuildMusicHandler(Context.Guild).Stop();
        }
    }

    [Name("Player-specific")]
    public class PlayerCommands : ModuleBase<MayaCommandContext>
    {
        [Command("marie_splatoon"), Alias("marie")]
        [Summary("Special command for marie_splatoon")]
        public async Task Marie()
        {
            await ReplyAsync("Did you mean: boobs?  /人◕‿‿◕人\\");
        }

        [Command("khaenn"), Alias("khaen", "khaenn35")]
        [Summary("Special command for Khaenn35")]
        public async Task Khaen()
        {
            await ReplyAsync("http://i.imgur.com/zLNEnPy.png");
        }

        [Command("ktcraft"), Alias("kt", "serrith")]
        [Summary("Special command for KTcraft")]
        public async Task Kt()
        {
            await ReplyAsync("http://i.imgur.com/UeJ0dqC.png");
        }

        [Command("battleship_60"), Alias("bb60")]
        [Summary("Special command for Battleship_60")]
        public async Task Bb60()
        {
            await ReplyAsync("* spams chat internally *");
        }

        [Command("nightmare_fluttershy"), Alias("sparks")]
        [Summary("Special command for Nightmare_Fluttershy")]
        public async Task Sparks()
        {
            await ReplyAsync("http://i.imgur.com/fEM0XAm.png");
        }

        [Command("flieger56"), Alias("flieger")]
        [Summary("Special command for Flieger56")]
        public async Task Flieger()
        {
            await ReplyAsync("http://i.imgur.com/Pw2K8y4.png");
        }
    }

    [Name("Simple")]
    public class SimpleCommands : ModuleBase<MayaCommandContext>
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

        [Command("ping")]
        public async Task Ping()
        {
            await ReplyAsync($"🏓 Pong! ``{(Context.Client as DiscordSocketClient).Latency}ms``");
        }

        [Command("credits")]
        [Summary("Show the credits for creating the bot")]
        public async Task Credits()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            EmbedBuilder eb = new EmbedBuilder();
            IGuildUser bot = await Context.Guild.GetCurrentUserAsync();
            eb.Author = new EmbedAuthorBuilder().WithName("Credits").WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
            eb.ThumbnailUrl = application.Owner.GetAvatarUrl();
            eb.Color = Maya.Utils.GetRandomColor();
            eb.Description = $"Created by: {application.Owner.Mention}\n" +
                             $"Suggestions? Tell him. 😄\n\n" +
                              "Source: https://github.com/SubZero0/Maya";
            await ReplyAsync("", false, eb);
        }
    }

    [Name("Help")]
    public class HelpCommand : ModuleBase<MayaCommandContext>
    {
        [Command("help")]
        [Summary("Shows the help command")]
        public async Task Help([Remainder] string command = null)
        {
            if (Context.Channel is ITextChannel && !Context.MainHandler.GuildPersonalityHandler(Context.Guild).IsReady())
            {
                await ReplyAsync("Loading...");
                return;
            }
            await Context.MainHandler.CommandHandler.ShowHelpCommand(Context, command);
        }
    }
}