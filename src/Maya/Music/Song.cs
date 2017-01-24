using Discord;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Maya.Music.MusicEnums;

namespace Maya.Music
{
    public class Song
    {
        public string Title { get; set; }
        public string VideoId { get; set; }
        public TimeSpan? Duration { get; set; }
        public DateTime? StartTime { get; set; }
        public MusicStatus Status { get; set; }
        public MusicProvider Provider { get; set; }
        public Song()
        {
            Title = VideoId = null;
            Duration = null;
            StartTime = null;
            Status = MusicStatus.NEW;
        }

        public int GetTimeUntilEnd()
        {
            if (Duration == null)
                return 0;
            else if (StartTime == null)
                return (int)Duration?.TotalSeconds;
            else
                return (int)new TimeSpan(0, 0, (int)Duration?.TotalSeconds - ((int)DateTime.Now.Subtract(StartTime.GetValueOrDefault()).TotalSeconds)).TotalSeconds;
        }

        public string GetTimePlaying()
        {
            if (StartTime  == null || Duration == null)
                return "0:00";
            TimeSpan t = DateTime.Now.Subtract(StartTime.GetValueOrDefault());
            if (t.TotalSeconds >= Duration?.TotalSeconds)
                return $"{Duration?.Minutes}:{(Duration?.Seconds < 10 ? $"0{Duration?.Seconds}" : $"{Duration?.Seconds}")}";
            return $"{t.Minutes}:{(t.Seconds < 10 ? $"0{t.Seconds}" : $"{t.Seconds}")}";
        }
    }
}
