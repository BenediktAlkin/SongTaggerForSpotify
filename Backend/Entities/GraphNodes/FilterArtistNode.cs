using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
                OutputResult = null;
                PropagateForward(gn => gn.ClearResult(), applyToSelf: false);
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
                OutputResult = null;
                PropagateForward(gn => gn.ClearResult(), applyToSelf: false);
            }
        }

        private List<Artist> validArtists;
        [NotMapped]
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
                ValidArtists = InputResult[0].SelectMany(track => track.Artists).Distinct().OrderBy(tag => tag.Name).ToList();
        }

        protected override bool CanAddInput(GraphNode input) => !Inputs.Any();

        protected override Task MapInputToOutput()
        {
            if (Artist != null)
                OutputResult = InputResult[0].Where(t => t.Artists.Contains(Artist)).ToList();
            return Task.CompletedTask;
        }

        public override bool IsValid => ArtistId != null || Artist != null;
        public override bool RequiresArtists => true;
    }
}
