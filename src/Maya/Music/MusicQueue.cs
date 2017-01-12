using Maya.GuildHandlers;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace Maya.Music
{
    public class MusicQueue
    {
        private MusicHandler MusicHandler;
        private ConcurrentQueue<MusicContext> queue;
        public MusicQueue(MusicHandler MusicHandler)
        {
            this.MusicHandler = MusicHandler;
            queue = new ConcurrentQueue<MusicContext>();
        }

        public async Task Enqueue(MusicContext song)
        {
            MusicDownloader downloader = null;
            if (MusicHandler.shouldDownload())
                downloader = new MusicDownloader(song);
            queue.Enqueue(song);
            if (downloader != null)
                await downloader.Run();
        }

        public ConcurrentQueue<MusicContext> getQueue()
        {
            return queue;
        }

        public MusicContext getNextSong()
        {
            if (queue.Count == 0)
                return null;
            MusicContext r;
            while (!queue.TryDequeue(out r)) ;
            return r;
        }

        public void Clear()
        {
            queue = new ConcurrentQueue<MusicContext>();
            var files = Directory.GetFiles("Temp");
            foreach (string f in files)
                File.Delete(f);
        }
    }
}
