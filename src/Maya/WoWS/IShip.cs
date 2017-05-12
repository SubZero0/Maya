using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Maya.WoWS
{
    public interface IShip
    {
        string GetHeadStats();
        Task<string> GetSimpleStatsAsync();
        Task UpdateDataAsync();
        string GetName();
        ulong GetId();
        string GetImageUrl();
    }
}
