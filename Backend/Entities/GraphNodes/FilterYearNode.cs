using System.Linq;

namespace Backend.Entities.GraphNodes
{
    public class FilterYearNode : FilterRangeNode
    {
        protected override int? GetValue(Track t) => t.Album.ReleaseYear;
        public override bool RequiresAlbums => true;
    }
}
