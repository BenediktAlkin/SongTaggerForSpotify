using System.Linq;

namespace Backend.Entities.GraphNodes
{
    public class DeduplicateNode : GraphNode
    {
        protected override bool CanAddInput(GraphNode input) => !Inputs.Any();
        protected override void MapInputToOutput() => OutputResult = InputResult[0].Distinct().ToList();
    }
}
