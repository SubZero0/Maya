using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Maya.Attributes;
using System.Collections.Generic;
using System.Net.Http;
using System.IO;
using static Maya.Enums.Administration;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis;
using System.Reflection;
using Maya.Roslyn;
using Maya.ModulesAddons;

namespace Maya.Modules
{
    [Name("Admin")]
    [RequireAdmin]
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
            await Context.MainHandler.GuildTitleHandler(Context.Guild).SaveTitlesAsync();
            await Context.MainHandler.GuildTagHandler(Context.Guild).SaveTagsAsync();
            await Context.MainHandler.GuildDatabaseHandler(Context.Guild).SaveAsync();
            await ReplyAsync("Saved tags, titles, and DB.");
        }

        [Command("reload")]
        public async Task Reload([Required("shipcache/ships/shipsplayers/answers/config/guild/localconfig/localdb/localmusic"), Remainder] string what = null)
        {
            switch (what)
            {
                case "shipcache":
                    {
                        Context.MainHandler.ShipHandler.torpedoesCache.Clear();
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
        [RequireContext(ContextType.Guild)]
        public async Task Voice([Required, Remainder] IVoiceChannel channel = null)
        {
            await Context.MainHandler.GuildMusicHandler(Context.Guild).JoinVoiceChannelAsync(channel);
            await ReplyAsync($"Joining {channel.Name}...");
        }

        [Command("Kick"), Summary("Kick @Username"), Remarks("KICK HIM FOR ONCE AND FOR ALL")]
        [RequireAdmin(AdminLevel.ADMIN)]
        public async Task KickAsync(SocketGuildUser user = null, [Remainder] string reason = null)
        {
            if (user == null)
                throw new ArgumentException("You must mention a user!");
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("You must provide a reason");

            var embed = new EmbedBuilder();
            embed.Title = $"**{user.Username}** was kicked from **{user.Guild.Name}**";
            embed.Description = $"**Kicked by: **{Context.User.Mention}!\n**Reason: **{reason}";
            embed.Color = Utils.getRandomColor();
            await user.KickAsync();
            await ReplyAsync("", false, embed);
        }

        [Command("Ban"), Summary("Ban @Username"), Remarks("Swing the ban hammer")]
        [RequireAdmin(AdminLevel.ADMIN)]
        public async Task BanAsync(SocketGuildUser user = null, [Remainder] string reason = null)
        {
            if (user == null)
                throw new ArgumentException("You must mention a user!");
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("You must provide a reason");
            var gld = Context.Guild as SocketGuild;
            var embed = new EmbedBuilder();
            embed.Title = $"**{user.Username}** was banned from **{user.Guild.Name}**";
            embed.Description = $"**Banned by: **{Context.User.Mention}!\n**Reason: **{reason}";
            embed.Color = Utils.getRandomColor();
            await gld.AddBanAsync(user);
            await ReplyAsync("", false, embed);

        }

        [Command("Delete"), Summary("Delete 10"), Remarks("Deletes X amount of messages"), Alias("Del")]
        [RequireAdmin(AdminLevel.MODERATOR)]
        [RequireBotPermission(GuildPermission.ManageMessages), RequireContext(ContextType.Guild)]
        public async Task DeleteAsync(int range = 0)
        {
            if (range <= 0)
                throw new ArgumentException("Enter a valid amount");

            var messageList = await Context.Channel.GetMessagesAsync(range).Flatten();
            await Context.Channel.DeleteMessagesAsync(messageList);
            var embed = new EmbedBuilder();
            embed.Title = "Messages Deleted";
            embed.Description = $"I've deleted {range} amount of messages.";
            embed.Color = Utils.getRandomColor();
            var x = await Context.Channel.SendMessageAsync("", false, embed);
            await Utils.AutoDeleteMsg(x, 5000);
        }
    }
}
