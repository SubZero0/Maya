using Discord.Commands;
using Maya.ModulesAddons;
using System;
using System.Threading.Tasks;

namespace Maya.Attributes
{
    public class RequireSwearjar : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider map)
        {
            if (!(context is MayaCommandContext))
                return Task.FromResult(PreconditionResult.FromError(SearchResult.FromError(CommandError.UnknownCommand, "Unknown command.")));
            MayaCommandContext con = context as MayaCommandContext;
            if (!con.MainHandler.GuildConfigHandler(context.Guild).GetSwearJar().IsEnabled)
                return Task.FromResult(PreconditionResult.FromError(SearchResult.FromError(CommandError.UnknownCommand, "Unknown command.")));
            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
