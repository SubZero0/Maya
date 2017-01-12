/*
 * Code from chatter-bot-api
 * Credits to: pierredavidbelanger
 * Link: https://github.com/pierredavidbelanger/chatter-bot-api
 */
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Maya.Chatterbot
{
    internal class Cleverbot : ChatterBot
    {
        private readonly int endIndex;
        private readonly string baseUrl;
        private readonly string url;

        public Cleverbot(string baseUrl, string url, int endIndex)
        {
            this.baseUrl = baseUrl;
            this.url = url;
            this.endIndex = endIndex;
        }

        public ChatterBotSession CreateSession()
        {
            return new CleverbotSession(baseUrl, url, endIndex);
        }
    }

    internal class CleverbotSession : ChatterBotSession
    {
        private int endIndex;
        private string url;
        private string baseUrl;
        private IDictionary<string, string> vars;
        private CookieCollection cookies;

        public CleverbotSession(string baseUrl, string url, int endIndex)
        {
            this.url = url;
            this.baseUrl = baseUrl;
            this.endIndex = endIndex;
        }

        public async Task Initialize()
        {
            vars = new Dictionary<string, string>();
            //vars["start"] = "y";
            vars["stimulus"] = "";
            vars["islearning"] = "1";
            vars["icognoid"] = "wsf";
            //vars["fno"] = "0";
            //vars["sub"] = "Say";
            //vars["cleanslate"] = "false";
            cookies = await ChatterBotUtils.GetCookiesAsync(baseUrl);
        }

        public async Task<ChatterBotThought> Think(ChatterBotThought thought)
        {
            vars["stimulus"] = thought.Text;

            var formData = ChatterBotUtils.ParametersToWWWFormURLEncoded(vars);
            var formDataToDigest = formData.Substring(9, endIndex);
            var formDataDigest = ChatterBotUtils.MD5(formDataToDigest);
            vars["icognocheck"] = formDataDigest;

            var response = await ChatterBotUtils.PostAsync(url, vars, cookies);

            var responseValues = response.Split('\r');

            //vars[""] = Utils.StringAtIndex(responseValues, 0); ??
            vars["sessionid"] = ChatterBotUtils.StringAtIndex(responseValues, 1);
            vars["logurl"] = ChatterBotUtils.StringAtIndex(responseValues, 2);
            vars["vText8"] = ChatterBotUtils.StringAtIndex(responseValues, 3);
            vars["vText7"] = ChatterBotUtils.StringAtIndex(responseValues, 4);
            vars["vText6"] = ChatterBotUtils.StringAtIndex(responseValues, 5);
            vars["vText5"] = ChatterBotUtils.StringAtIndex(responseValues, 6);
            vars["vText4"] = ChatterBotUtils.StringAtIndex(responseValues, 7);
            vars["vText3"] = ChatterBotUtils.StringAtIndex(responseValues, 8);
            vars["vText2"] = ChatterBotUtils.StringAtIndex(responseValues, 9);
            vars["prevref"] = ChatterBotUtils.StringAtIndex(responseValues, 10);
            //vars[""] = Utils.StringAtIndex(responseValues, 11); ??
            //            vars["emotionalhistory"] = Utils.StringAtIndex(responseValues, 12);
            //            vars["ttsLocMP3"] = Utils.StringAtIndex(responseValues, 13);
            //            vars["ttsLocTXT"] = Utils.StringAtIndex(responseValues, 14);
            //            vars["ttsLocTXT3"] = Utils.StringAtIndex(responseValues, 15);
            //            vars["ttsText"] = Utils.StringAtIndex(responseValues, 16);
            //            vars["lineRef"] = Utils.StringAtIndex(responseValues, 17);
            //            vars["lineURL"] = Utils.StringAtIndex(responseValues, 18);
            //            vars["linePOST"] = Utils.StringAtIndex(responseValues, 19);
            //            vars["lineChoices"] = Utils.StringAtIndex(responseValues, 20);
            //            vars["lineChoicesAbbrev"] = Utils.StringAtIndex(responseValues, 21);
            //            vars["typingData"] = Utils.StringAtIndex(responseValues, 22);
            //            vars["divert"] = Utils.StringAtIndex(responseValues, 23);

            var responseThought = new ChatterBotThought();

            responseThought.Text = ChatterBotUtils.StringAtIndex(responseValues, 0);

            return responseThought;
        }

        public async Task<string> Think(string text)
        {
            return (await Think(new ChatterBotThought { Text = text })).Text;
        }
    }
}
