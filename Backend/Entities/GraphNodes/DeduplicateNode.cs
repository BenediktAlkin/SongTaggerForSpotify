using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes
{
    public class DeduplicateNode : GraphNode
    {
        protected override bool CanAddInput(GraphNode input) => !Inputs.Any();

        public override async Task<List<Track>> GetResult()
        {
            var tracks = await GetInput();
            return tracks.Distinct().ToList();
        }
    }
}
