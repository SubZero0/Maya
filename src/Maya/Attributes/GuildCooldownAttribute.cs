using Discord;
using Discord.Commands;
using Maya.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Maya.Attributes
{
    public class GuildCooldownAttribute : PreconditionAttribute, Ignore
    {
        private Dictionary<IGuild, Timer> cd;
        private int _cooldown; //seconds
        public GuildCooldownAttribute(int cooldown = 60)
        {
            cd = new Dictionary<IGuild, Timer>();
            _cooldown = cooldown;
        }

        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider map)
        {
            if (cd.ContainsKey(context.Guild))
                return Task.FromResult(PreconditionResult.FromError(SearchResult.FromError(CommandError.UnknownCommand, "Unknown command.")));
            var timer = new Timer(Timer_Reset, context.Guild, _cooldown * 1000, Timeout.Infinite);
            cd.Add(context.Guild, timer);
            return Task.FromResult(PreconditionResult.FromSuccess());
        }

        private void Timer_Reset(object state)
        {
            cd[(IGuild)state].Dispose();
            cd.Remove((IGuild)state);
        }
    }
}
