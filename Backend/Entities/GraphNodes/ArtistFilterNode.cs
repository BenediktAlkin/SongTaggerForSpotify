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
                GraphGeneratorPage?.NotifyIsValidChanged();
            }
        }
        private Artist artist;
        public Artist Artist
        {
            get => artist;
            set
            {
                SetProperty(ref artist, value, nameof(Artist));
                GraphGeneratorPage?.NotifyIsValidChanged();
            }
        }

        protected override bool CanAddInput(GraphNode input) => !Inputs.Any();
        public override async Task<List<Track>> GetResult()
        {
            var tracks = await GetInput();
            return tracks.Where(t => t.Artists.Contains(Artist)).ToList();
        }
        public override bool IsValid => ArtistId != null || Artist != null;
        public override bool RequiresArtists => true;
    }
}
