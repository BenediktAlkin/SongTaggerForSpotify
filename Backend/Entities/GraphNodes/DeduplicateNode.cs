using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes
{
    public class DeduplicateNode : GraphNode
    {
        protected override bool CanAddInput(GraphNode input) => !Inputs.Any();
        protected override Task MapInputToOutput()
        {
            OutputResult = InputResult.Distinct().ToList();
            return Task.CompletedTask;
        }
    }
}
