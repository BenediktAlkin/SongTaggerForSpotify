using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entities
{
    public class AudioFeatures
    {
        public string Id { get; set; }
        public Track Track { get; set; }


        // 0.0 <= Acousticness <= 1.0
        public float Acousticness { get; set; }
        // 0.0 <= Danceability <= 1.0
        public float Danceability { get; set; }
        // 0.0 <= Energy <= 1.0
        public float Energy { get; set; }
        // 0.0 <= Instrumentalness <= 1.0
        public float Instrumentalness { get; set; }
        // -1 <= Key <= 11 (-1 = no key found)
        public int Key { get; set; }
        // TODO I think this is bounded but not sure 0.0 <= Liveness <= 1.0
        public float Liveness { get; set; }
        // not bounded (dB)
        public float Loudness { get; set; }
        // minor=0 major=1
        public int Mode { get; set; }
        // 0.0 <= Speechiness <= 1.0
        public float Speechiness { get; set; }
        // not bounded
        public float Tempo { get; set; }
        // 3 <= TimeSignature <= 7 (3/4 to 7/4)
        public int TimeSignature { get; set; }
        // 0.0 <= Valence <= 1.0
        public float Valence { get; set; }
    }
}
