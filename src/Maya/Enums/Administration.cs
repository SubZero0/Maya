using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Maya.Enums
{
    public class Administration
    {
        public enum AdminLevel
        {
            USER = 0,
            MODERATOR = 1,
            ADMIN = 2,
            OWNER = 3
        };

        public static string ToFriendlyString(AdminLevel al)
        {
            switch (al)
            {
                case AdminLevel.USER:
                    return "User";
                case AdminLevel.MODERATOR:
                    return "Moderator";
                case AdminLevel.ADMIN:
                    return "Administrator";
                case AdminLevel.OWNER:
                    return "Owner";
            }
            return "";
        }
    }
}
