using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Maya.Music
{
    public class MusicEnums
    {
        public enum MusicStatus
        {
            NEW,
            DOWNLOADING,
            DOWNLOADED,
            BROKEN
        }

        public enum MusicProvider
        {
            YOUTUBE
        }
    }
}
