using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Maya
{
    public class Program
    {
        public static void Main(string[] args) => new MayaBot().RunAsync().GetAwaiter().GetResult();
    }
}
