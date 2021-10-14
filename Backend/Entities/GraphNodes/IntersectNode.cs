using System.Linq;

namespace Backend.Entities.GraphNodes
{
    public class IntersectNode : GraphNode
    {
        protected override void MapInputToOutput()
        {
            if (InputResult.Count == 0)
            {
                OutputResult = new();
                return;
            }
            if (InputResult.Count == 1)
            {
                OutputResult = InputResult[0];
                return;
            }
            var output = InputResult[0].Intersect(InputResult[1]);
            for (var i = 2; i < InputResult.Count; i++)
                output = output.Intersect(InputResult[i]);
            OutputResult = output.ToList();
        }
    }
}
