using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes
{
    public class FilterArtistNode : GraphNode
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

        private List<Artist> validArtists;
        public List<Artist> ValidArtists
        {
            get => validArtists;
            private set => SetProperty(ref validArtists, value, nameof(ValidArtists));
        }
        protected override void OnInputResultChanged()
        {
            if (InputResult == null)
                ValidArtists = null;
            else
                ValidArtists = InputResult.SelectMany(track => track.Artists).Distinct().OrderBy(tag => tag.Name).ToList();
        }

        protected override bool CanAddInput(GraphNode input) => !Inputs.Any();

        protected override Task MapInputToOutput()
        {
            OutputResult = InputResult.Where(t => t.Artists.Contains(Artist)).ToList();
            return Task.CompletedTask;
        }

        public override bool IsValid => ArtistId != null || Artist != null;
        public override bool RequiresArtists => true;
    }
}
