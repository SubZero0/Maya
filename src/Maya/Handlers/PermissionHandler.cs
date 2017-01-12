using Discord;
using Maya.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Maya.Enums.Administration;

namespace Maya.Handlers
{
    public class PermissionHandler
    {
        private MainHandler MainHandler;
        public PermissionHandler(MainHandler MainHandler)
        {
            this.MainHandler = MainHandler;
        }

        public async Task<bool> isOwner(IUser user)
        {
            return user.Id == (await MainHandler.Client.GetApplicationInfoAsync()).Owner.Id;
        }

        public async Task<bool> isAdmin(IUser user)
        {
            return await Task.FromResult(false);
        }

        public async Task<bool> isMod(IUser user)
        {
            return await Task.FromResult(false);
        }

        public async Task<bool> isAtLeast(IUser user, AdminLevel level)
        {
            if (await isOwner(user) && AdminLevel.OWNER >= level)
                return await Task.FromResult(true);
            if (await isAdmin(user) && AdminLevel.ADMIN >= level)
                return await Task.FromResult(true);
            if (await isMod(user) && AdminLevel.MODERATOR >= level)
                return await Task.FromResult(true);
            if (AdminLevel.USER >= level)
                return await Task.FromResult(true);
            return false;
        }
    }
}
