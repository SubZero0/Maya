/*
 * Code from chatter-bot-api
 * Credits to: pierredavidbelanger
 * Link: https://github.com/pierredavidbelanger/chatter-bot-api
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Maya.Chatterbot
{
    internal class Pandorabots : ChatterBot
    {
        private readonly string botid;

        public Pandorabots(string botid)
        {
            this.botid = botid;
        }

        public ChatterBotSession CreateSession()
        {
            return new PandorabotsSession(botid);
        }
    }

    internal class PandorabotsSession : ChatterBotSession
    {
        private readonly IDictionary<string, string> vars;

        public PandorabotsSession(string botid)
        {
            vars = new Dictionary<string, string>();
            vars["botid"] = botid;
            vars["custid"] = Guid.NewGuid().ToString();
        }

        public async Task Initialize()
        {
            await Task.CompletedTask;
        }

        public async Task<ChatterBotThought> Think(ChatterBotThought thought)
        {
            vars["input"] = thought.Text;

            var response = await ChatterBotUtils.PostAsync("https://www.pandorabots.com/pandora/talk-xml", vars, null);

            var responseThought = new ChatterBotThought();
            responseThought.Text = ChatterBotUtils.XPathSearch(response, "//result/that/text()");

            return responseThought;
        }

        public async Task<string> Think(string text)
        {
            return (await Think(new ChatterBotThought { Text = text })).Text;
        }
    }
}
