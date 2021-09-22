using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes
{
    public class ArtistFilterNode : GraphNode
    {
        private int? artistId;
        public int? ArtistId
        {
            get => artistId;
            set
            {
                SetProperty(ref artistId, value, nameof(ArtistId));
                GraphGeneratorPage.NotifyIsValidChanged();
            }
        }
        private Artist artist;
        public Artist Artist
        {
            get => artist;
            set
            {
                SetProperty(ref artist, value, nameof(Artist));
                GraphGeneratorPage.NotifyIsValidChanged();
            }
        }

        protected override bool CanAddInput(GraphNode input) => Inputs.Count() < 1;
        public override async Task<List<Track>> GetInput()
        {
            if (Inputs == null || !Inputs.Any())
                return new List<Track>();

            return await Inputs.First().GetResult();
        }
        public override async Task<List<Track>> GetResult()
        {
            var tracks = await GetInput();

            // populate tracks with tags
            var dbTracks = await ConnectionManager.Instance.Database.Tracks.Include(t => t.Tags).ToDictionaryAsync(t => t.Id, t => t);
            foreach (var track in tracks)
            {
                if (dbTracks.TryGetValue(track.Id, out var dbTrack))
                    track.Tags = dbTrack.Tags;
            }

            // filter
            //return tracks.Where(t => t.Artists.Select(a => a.Name).Contains(Artist.Name)).ToList();
            return tracks.Where(t => t.Artists.Contains(Artist)).ToList();
        }
        public override bool IsValid => ArtistId != null || Artist != null;
    }
}
