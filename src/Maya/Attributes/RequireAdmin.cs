using Discord.Commands;
using Maya.Enums;
using Maya.ModulesAddons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Maya.Enums.Administration;

namespace Maya.Attributes
{
    public class RequireAdmin : PreconditionAttribute
    {
        private AdminLevel _level;
        public RequireAdmin(AdminLevel level = AdminLevel.ADMIN)
        {
            _level = level;
        }

        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider map)
        {
            if (!(context is MayaCommandContext))
                return PreconditionResult.FromError("null");
            MayaCommandContext con = context as MayaCommandContext;
            if (await con.MainHandler.PermissionHandler.IsAtLeastAsync(context.User, _level))
                return PreconditionResult.FromSuccess();
            return PreconditionResult.FromError($"You must have at least **{ToFriendlyString(_level)}** rights to use this command.");
        }
    }
}
