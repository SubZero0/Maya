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

        public async Task EnqueueAsync(MusicContext song)
        {
            MusicDownloader downloader = null;
            if (MusicHandler.ShouldDownload())
                downloader = new MusicDownloader(song);
            queue.Enqueue(song);
            if (downloader != null)
                await downloader.RunAsync();
        }

        public ConcurrentQueue<MusicContext> GetQueue()
        {
            return queue;
        }

        public MusicContext GetNextSong()
        {
            MusicContext r = null;
            while (queue.Count != 0 && !queue.TryDequeue(out r)) ;
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
