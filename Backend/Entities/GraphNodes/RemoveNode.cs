using System.Linq;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes
{
    public class RemoveNode : GraphNode
    {
        private GraphNode baseSet;
        public GraphNode BaseSet
        {
            get => baseSet;
            set => SetProperty(ref baseSet, value, nameof(BaseSet));
        }
        private GraphNode removeSet;
        public GraphNode RemoveSet
        {
            get => removeSet;
            set => SetProperty(ref removeSet, value, nameof(RemoveSet));
        }

        public void SwapSets()
        {
            var temp = BaseSet;
            BaseSet = RemoveSet;
            RemoveSet = temp;
            OutputResult = null;
        }

        protected override bool CanAddInput(GraphNode input) => Inputs.Count() < 2;
        protected override void OnAddInput(GraphNode input)
        {
            if (BaseSet == null)
            {
                BaseSet = input;
                return;
            }
            if (RemoveSet == null) 
            {
                RemoveSet = input;
                return;
            }
        }
        protected override void OnRemoveInput(GraphNode input)
        {
            if (BaseSet == input)
                BaseSet = null;
            if (RemoveSet == input)
                RemoveSet = null;
        }

        protected override Task MapInputToOutput()
        {
            if (BaseSet == null)
            {
                OutputResult = new();
                return Task.CompletedTask;
            }
            if (RemoveSet == null)
            {
                OutputResult = BaseSet.OutputResult;
                return Task.CompletedTask;
            }
                

            OutputResult = BaseSet.OutputResult.Except(RemoveSet.OutputResult).ToList();
            return Task.CompletedTask;
        }
    }
}
