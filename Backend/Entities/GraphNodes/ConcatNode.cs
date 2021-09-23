using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes
{
    public class ConcatNode : GraphNode
    {
        public override async Task<List<Track>> GetInput()
        {
            if (Inputs == null)
                return new List<Track>();

            var concated = new List<Track>();
            foreach (var input in Inputs)
                concated.AddRange(await input.GetResult());
            return concated;
        }
    }
}
