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
        protected override Task MapInputToOutput()
        {
            OutputResult = InputResult[0].Where(t => t.Tags == null || t.Tags.Count == 0).ToList();
            return Task.CompletedTask;
        }
        public override bool RequiresTags => true;
    }
}
