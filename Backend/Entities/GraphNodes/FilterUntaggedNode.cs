using Serilog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes
{
    public class FilterUntaggedNode : GraphNode
    {
        protected override bool CanAddInput(GraphNode input) => !Inputs.Any();
        protected override void MapInputToOutput()
        {
            if (InputResult != null && InputResult.Count > 0)
                OutputResult = InputResult[0].Where(t => t.Tags == null || t.Tags.Count == 0).ToList();
        }
        public override bool RequiresTags => true;
    }
}
