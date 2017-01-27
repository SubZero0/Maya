using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Maya.Attributes;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.IO;
using Microsoft.CodeAnalysis;
using Maya.WoWS;
using Maya.ModulesAddons;

namespace Maya.Modules
{
    [Name("WoWS")]
    public class WowsModule : ModuleBase<MayaCommandContext>
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
                IMessage m = await ReplyAsync("Processing...");
                String ss = await r[0].GetSimpleStatsAsync();
                await m.DeleteAsync();
                EmbedBuilder eb = new EmbedBuilder();
                eb.Title = r[0].GetHeadStats();
                eb.Color = Utils.getRandomColor();
                eb.Description = ss;
                eb.ThumbnailUrl = r[0].GetImageUrl();
                eb.Footer = new EmbedFooterBuilder().WithText("[!] Warning: WoWS ship profile is broken (stock values everywhere)!");
                await ReplyAsync("", false, eb);
            }
            return;
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
            await ReplyAsync($"Ship list: {String.Join(", ", Context.MainHandler.ShipHandler.GetShipList())}");
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
}
