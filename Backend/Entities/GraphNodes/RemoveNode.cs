using System.Linq;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes
{
    public class RemoveNode : GraphNode
    {
        public int? BaseSetId{ get; set; }
        private GraphNode baseSet;
        public GraphNode BaseSet
        {
            get => baseSet;
            set => SetProperty(ref baseSet, value, nameof(BaseSet));
        }
        public int? RemoveSetId { get; set; }
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
            PropagateForward(gn => gn.ClearResult(), applyToSelf: false);
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

        protected override void MapInputToOutput()
        {
            if (BaseSet == null)
            {
                OutputResult = new();
                return;
            }
            if (RemoveSet == null)
            {
                OutputResult = BaseSet.OutputResult;
                return;
            }
                
            OutputResult = BaseSet.OutputResult.Except(RemoveSet.OutputResult).ToList();
        }
    }
}
