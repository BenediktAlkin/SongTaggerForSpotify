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
        public int AcousticnessPercent => (int)(Acousticness * 100);
        // 0.0 <= Danceability <= 1.0
        public float Danceability { get; set; }
        public int DanceabilityPercent => (int)(Danceability * 100);
        // 0.0 <= Energy <= 1.0
        public float Energy { get; set; }
        public int EnergyPercent => (int)(Energy * 100);
        // 0.0 <= Instrumentalness <= 1.0
        public float Instrumentalness { get; set; }
        public int InstrumentalnessPercent => (int)(Instrumentalness * 100);
        // -1 <= Key <= 11 (-1 = no key found)
        public int Key { get; set; }
        // TODO I think this is bounded but not sure 0.0 <= Liveness <= 1.0
        public float Liveness { get; set; }
        public int LivenessPercent => (int)(Liveness * 100);
        // not bounded (dB)
        public float Loudness { get; set; }
        // minor=0 major=1
        public int Mode { get; set; }
        // 0.0 <= Speechiness <= 1.0
        public float Speechiness { get; set; }
        public int SpeechinessPercent => (int)(Speechiness * 100);
        // not bounded
        public float Tempo { get; set; }
        // 3 <= TimeSignature <= 7 (3/4 to 7/4)
        public int TimeSignature { get; set; }
        // 0.0 <= Valence <= 1.0
        public float Valence { get; set; }
        public int ValencePercent => (int)(Valence * 100);
    }
}
