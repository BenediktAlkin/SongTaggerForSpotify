using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes
{
    public class ConcatNode : GraphNode
    {
        public override async Task<List<Track>> GetResult()
        {
            if (Inputs == null)
                return new List<Track>();

            var inputs = new List<List<Track>>();
            foreach (var input in Inputs)
                inputs.Add(await input.GetResult());

            return inputs.SelectMany(tracks => tracks).ToList();
        }
    }
}
