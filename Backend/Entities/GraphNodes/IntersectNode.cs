using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes
{
    public class IntersectNode : GraphNode
    {
        protected override Task MapInputToOutput()
        {
            if (InputResult.Count == 0)
            {
                OutputResult = new();
                return Task.CompletedTask;
            }
            if (InputResult.Count == 1)
            {
                OutputResult = InputResult[0];
                return Task.CompletedTask;
            }
            var output = InputResult[0].Intersect(InputResult[1]);
            for (var i = 2; i < InputResult.Count; i++)
                output = output.Intersect(InputResult[i]);
            OutputResult = output.ToList();
            return Task.CompletedTask;
        }
    }
}
