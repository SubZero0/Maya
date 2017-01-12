using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Maya.Music
{
    public class MusicResult
    {
        public bool IsSuccessful { get; private set; }
        public string Error { get; private set; }
        public MusicResult(string error = null)
        {
            if (error == null)
                IsSuccessful = true;
            Error = error;
        }
    }
}
