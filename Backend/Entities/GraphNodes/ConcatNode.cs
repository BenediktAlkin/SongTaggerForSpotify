using System.Linq;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes
{
    public class ConcatNode : GraphNode
    {
        protected override void MapInputToOutput() => OutputResult = InputResult.SelectMany(ir => ir).ToList();

    }
}
