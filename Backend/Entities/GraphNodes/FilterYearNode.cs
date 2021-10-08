using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes
{
    public class FilterYearNode : GraphNode
    {
        private int? yearFrom;
        public int? YearFrom
        {
            get => yearFrom;
            set
            {
                SetProperty(ref yearFrom, value, nameof(YearFrom));
                GraphGeneratorPage?.NotifyIsValidChanged();
                OutputResult = null;
                PropagateForward(gn => gn.ClearResult(), applyToSelf: false);
            }
        }
        private int? yearTo;
        public int? YearTo
        {
            get => yearTo;
            set
            {
                SetProperty(ref yearTo, value, nameof(YearTo));
                GraphGeneratorPage?.NotifyIsValidChanged();
                OutputResult = null;
                PropagateForward(gn => gn.ClearResult(), applyToSelf: false);
            }
        }
        
        protected override bool CanAddInput(GraphNode input) => !Inputs.Any();

        protected override void MapInputToOutput()
        {
            if (YearFrom != null && YearTo != null)
                OutputResult = InputResult[0].Where(t => YearFrom <= t.Album.ReleaseYear && t.Album.ReleaseYear <= YearTo).ToList();
            else if (YearFrom != null)
                OutputResult = InputResult[0].Where(t => YearFrom <= t.Album.ReleaseYear).ToList();
            else if (YearTo != null)
                OutputResult = InputResult[0].Where(t => t.Album.ReleaseYear <= YearTo).ToList();
        }

        public override bool IsValid => true;
        public override bool RequiresAlbums => true;
    }
}
