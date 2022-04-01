using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Backend.Entities.GraphNodes
{
    public class FilterGenreNode : GraphNode
    {
        private int? genreId;
        public int? GenreId
        {
            get => genreId;
            set
            {
                if (value == genreId) return;
                SetProperty(ref genreId, value, nameof(GenreId));
                OutputResult = null;
                PropagateForward(gn => gn.ClearResult(), applyToSelf: false);
            }
        }
        private Genre genre;
        public Genre Genre
        {
            get => genre;
            set
            {
                if (value == genre) return;
                SetProperty(ref genre, value, nameof(Genre));
                OutputResult = null;
                PropagateForward(gn => gn.ClearResult(), applyToSelf: false);
            }
        }

        private List<Genre> validGenres;
        [NotMapped]
        public List<Genre> ValidGenres
        {
            get => validGenres;
            private set => SetProperty(ref validGenres, value, nameof(ValidGenres));
        }
        protected override void OnInputResultChanged()
        {
            if (InputResult == null || InputResult.Count == 0)
                ValidGenres = null;
            else
                ValidGenres = InputResult[0].SelectMany(track => track.Artists).SelectMany(artist => artist.Genres).Distinct().OrderBy(genre => genre.Name).ToList();
            Logger.Information($"OnInputResultChanged for {this} (ValidGenresCount={ValidGenres?.Count})");
        }

        protected override bool CanAddInput(GraphNode input) => !Inputs.Any();

        protected override void MapInputToOutput()
            => OutputResult = InputResult[0].Where(t => t.Artists.SelectMany(artist => artist.Genres).Contains(Genre)).ToList();

        public override bool IsValid => GenreId != null || Genre != null;
        public override bool RequiresArtists => true;
        public override bool RequiresGenres => true;
    }
}
