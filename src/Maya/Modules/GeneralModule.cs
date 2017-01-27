using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Maya.Attributes;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json.Linq;
using static Maya.Enums.Administration;
using Microsoft.CodeAnalysis;
using Maya.Handlers;
using System.Globalization;
using Maya.ModulesAddons;

namespace Maya.Modules
{
    [Name("General")]
    public class GeneralModule : ModuleBase<MayaCommandContext>
    {
        [Command("image")]
        [Alias("i")]
        public async Task Image([Required, Remainder] string search = null)
        {
            using (var httpClient = new HttpClient())
            {
                var link = $"https://api.cognitive.microsoft.com/bing/v5.0/images/search?q={search}&count=10&offset=0&mkt=en-us&safeSearch=Moderate";
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", MiscHandler.Load().BingAPIKey);
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

        [Command("swearjar")]
        [Summary("Show how much money the swear jar currently has")]
        [RequireContext(ContextType.Guild)]
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
        [Cooldown(120)]
        public async Task Marry()
        {
            if (!Context.MainHandler.GuildPersonalityHandler(Context.Guild).IsReady())
            {
                await ReplyAsync("Loading...");
                return;
            }
            await ReplyAsync(Utils.GetRandomWeightedChoice(Context.MainHandler.GuildPersonalityHandler(Context.Guild).GetMarryAnswers()));
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
                eb.Title = $"Meaning: {text}";//TODO: Add urban img?
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
            eb.Author = new EmbedAuthorBuilder().WithName($"{author.Username}#{author.Discriminator}").WithIconUrl(author.AvatarUrl);
            eb.Color = Utils.getRandomColor();
            eb.Description = String.Join("\n", ordered_msgs.Select(x => x.Content));
            eb.Timestamp = older;
            await ReplyAsync("", false, eb);
        }

        [Command("info")]
        [Summary("Get the bot's application info")]
        public async Task Info()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            EmbedBuilder eb = new EmbedBuilder();
            IGuildUser bot = await Context.Guild.GetCurrentUserAsync();
            eb.Author = new EmbedAuthorBuilder().WithName(bot.Nickname ?? bot.Username).WithIconUrl(Context.Client.CurrentUser.AvatarUrl);
            eb.ThumbnailUrl = Context.Client.CurrentUser.AvatarUrl;
            eb.Color = Utils.getRandomColor();
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

        [Command("Userinfo"), Summary("Userinfo @Username"), Remarks("Displays information about a User"), Alias("UI")]
        public async Task UserInfo(IUser user = null)
        {
            if (user == null)
                throw new ArgumentException("You must mention a user!");
            var usr = user as IGuildUser ?? Context.Message.Author as IGuildUser;
            var embed = new EmbedBuilder();
            embed.Color = Utils.getRandomColor();

            if (!string.IsNullOrWhiteSpace(usr.AvatarUrl))
                embed.ThumbnailUrl = usr.AvatarUrl;

            var N = usr.Nickname ?? usr.Nickname;
            var D = usr.DiscriminatorValue;
            var I = usr.Id;
            var B = usr.IsBot;
            var S = usr.Status;
            var G = usr.Game.Value.Name;
            var C = usr.CreatedAt;
            var J = usr.JoinedAt;
            var P = usr.GuildPermissions;

            embed.Title = $"{usr.Username} Information";
            embed.Description = $"**Nickname: **{N}\n**Discriminator: **{D}\n**ID: **{I}\n**Is Bot: **{B}\n**Status: **{S}\n**Game: **{G}\n" +
                $"**Created At: **{C}\n**Joined At: **{J}\n**Guild Permissions: **{P}";

            await ReplyAsync("", false, embed);
        }

        [Command("GuildInfo"), Summary("GI"), Remarks("Displays information about a guild"), Alias("GI")]
        public async Task GuildInfo()
        {
            var embed = new EmbedBuilder();
            embed.Color = Utils.getRandomColor();
            var gld = Context.Guild;
            if (!string.IsNullOrWhiteSpace(gld.IconUrl))
                embed.ThumbnailUrl = gld.IconUrl;
            var I = gld.Id; //.ToString
            var O = gld.GetOwnerAsync().GetAwaiter().GetResult().Mention;
            var D = gld.GetDefaultChannelAsync().GetAwaiter().GetResult().Mention;
            var V = gld.VoiceRegionId;
            var C = gld.CreatedAt;
            var A = gld.Available;
            var N = gld.DefaultMessageNotifications;
            var E = gld.IsEmbeddable;
            var L = gld.MfaLevel;
            var R = gld.Roles;
            var VL = gld.VerificationLevel;
            embed.Title = $"{gld.Name} Information";
            embed.Description = $"**Guild ID: **{I}\n**Guild Owner: **{O}\n**Default Channel: **{D}\n**Voice Region: **{V}\n**Created At: **{C}\n**Available? **{A}\n" +
                $"**Default Msg Notif: **{N}\n**Embeddable? **{E}\n**MFA Level: **{L}\n**Verification Level: **{VL}\n";
            await ReplyAsync("", false, embed);
        }

        [Command("Listroles"), Summary("ListRoles"), Remarks("Shows all the roles a guild has"), Alias("LI")]
        public async Task ListRoles()
        {
            var gld = Context.Guild;
            var chn = Context.Channel;
            var msg = Context.Message;

            var grp = gld.Roles;
            if (grp == null)
                return;
            var embed = new EmbedBuilder();
            embed.Title = "Role List";
            embed.Description = string.Format("Listing of all {0:#,##0} role{1} in this Guild.", grp.Count, grp.Count > 1 ? "s" : "");
            embed.Color = Utils.getRandomColor();
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Role list";
                x.Value = string.Join(", ", grp.Select(xr => string.Concat("**", xr.Name, "**")));
            });
            await chn.SendMessageAsync("", false, embed);
        }

        [Command("Info"), Summary("Displays information about Maya"), Remarks("YEAHH!! This shit is written by me!")]
        public async Task Maya()
        {
            var Maya = Context.Client as DiscordSocketClient;
            var Gld = Context.Guild as SocketGuild;
            var embed = new EmbedBuilder();
            embed.Color = Utils.getRandomColor();
            embed.Title = Maya.CurrentUser.Username;
            embed.ThumbnailUrl = Maya.CurrentUser.AvatarUrl;

            var Servers = Gld.Discord.Guilds.Count();
            var Users = Maya.Guilds.Sum(g => g.Users.Count);
            var Lib = DiscordConfig.Version;
            var Heap = $"{Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2)}MB";
            var Lat = $"{Maya.Latency} MS";
            var Run = $"{RuntimeInformation.FrameworkDescription}{ RuntimeInformation.OSArchitecture}";
            var Up = $"{(DateTime.Now - Process.GetCurrentProcess().StartTime)}";
            var Ap = DiscordConfig.APIVersion.ToString();
            var Con = Maya.ConnectionState;
            var title = $"BLEEP BLOOP. BLOOP BEEP?";
            if (Gld != null)
            {
                embed.Title = title;
                embed.Description = $"**Servers: **{Servers}\n**Users: **{Users}\n**Library: **{Lib}\n**API Version: **{Ap}\n" +
                    $"**Latency: **{Lat}\n**Heap Size: **{Heap}\n**Runtime: **{Run}\n**Uptime: **{Up}\n**Connection State: **{Con}";
                await Context.Channel.SendMessageAsync("", false, embed);
            }
        }

        [Command("RHelp"), Alias("SO"), RequireContext(ContextType.Guild), Summary("SO"), Remarks("When you wanna fuck me.")]
        public async Task RequestOwnerAsync([Remainder] string reason = null)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Please provide a reason!");
            var owner = Context.User.Id == (await Context.Client.GetApplicationInfoAsync()).Owner.Id;
            await ReplyAsync("Uh..Ok M8. Executing ritual for summoning Lucifer...");
            var loggingChannel = await Context.Client.GetChannelAsync(MiscHandler.Load().ExeChannel) as ITextChannel;
            var invite = await (Context.Channel as IGuildChannel).CreateInviteAsync(maxUses: null);
            var embed = new EmbedBuilder();
            embed.Color = Utils.getRandomColor();
            embed.Title = $"**Summoned By: **{Context.User}";
            embed.Description = $"**Guild Name: **{Context.Guild.Name}\n**Reason: **{reason}\n**Invite: **{invite.Url}";
            await loggingChannel.SendMessageAsync("", false, embed);
        }
    }
}
