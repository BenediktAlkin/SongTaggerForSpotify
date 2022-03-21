using System.Linq;

namespace Backend.Entities.GraphNodes
{
    public class FilterYearNode : FilterRangeNode
    {
        protected override double? GetValue(Track t) => t.Album.ReleaseYear;
        public override bool RequiresAlbums => true;
    }
}
