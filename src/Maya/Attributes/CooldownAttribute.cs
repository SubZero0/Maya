using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Maya.Attributes
{
    public class CooldownAttribute : PreconditionAttribute
    {
        private Timer timer;
        private bool cd;
        private int _cooldown; //seconds
        public CooldownAttribute(int cooldown = 60)
        {
            cd = false;
            _cooldown = cooldown;
            timer = new Timer(Timer_Reset, null, Timeout.Infinite, Timeout.Infinite);
        }

        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            if (cd)
                return Task.FromResult(PreconditionResult.FromError(SearchResult.FromError(CommandError.UnknownCommand, "Unknown command.")));
            cd = true;
            timer.Change(_cooldown * 1000, Timeout.Infinite);
            return Task.FromResult(PreconditionResult.FromSuccess());
        }

        private void Timer_Reset(object state)
        {
            cd = false;
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}
