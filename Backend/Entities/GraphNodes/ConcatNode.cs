using System.Linq;

namespace Backend.Entities.GraphNodes
{
    public class ConcatNode : GraphNode
    {
        protected override void MapInputToOutput() => OutputResult = InputResult.SelectMany(ir => ir).ToList();

    }
}
