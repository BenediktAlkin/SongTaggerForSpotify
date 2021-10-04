using System.Linq;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes
{
    public class ConcatNode : GraphNode
    {
        protected override Task MapInputToOutput()
        {
            OutputResult = InputResult.SelectMany(ir => ir).ToList();
            return Task.CompletedTask;
        }

    }
}
