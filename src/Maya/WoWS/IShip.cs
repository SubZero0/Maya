using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Maya.WoWS
{
    public interface IShip
    {
        string getHeadStats();
        Task<string> getSimpleStats();
        Task updateData();
        string getName();
        string getImageUrl();
    }
}
