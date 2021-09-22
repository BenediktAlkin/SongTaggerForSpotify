using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes
{
    public class DeduplicateNode : GraphNode
    {
        protected override bool CanAddInput(GraphNode input) => Inputs.Count() < 1;

        public override async Task<List<Track>> GetInput()
        {
            if (Inputs == null || Inputs.Count() == 0)
                return new List<Track>();

            return await Inputs.First().GetResult();
        }
        public override async Task<List<Track>> GetResult()
        {
            var inputs = await GetInput();
            return inputs.Distinct().ToList();
        }
    }
}
