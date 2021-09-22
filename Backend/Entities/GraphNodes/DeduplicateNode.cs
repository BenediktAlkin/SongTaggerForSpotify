using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes
{
    public class DeduplicateNode : GraphNode
    {
        protected override bool CanAddInput(GraphNode input) => Inputs.Count() < 1;

        public override async Task<List<Track>> GetResult()
        {
            if (Inputs == null || Inputs.Count() == 0) 
                return new List<Track>();

            var inputs = await Inputs.First().GetResult();
            return inputs.Distinct().ToList();
        }
    }
}
