using System.Linq;

namespace Backend.Entities.GraphNodes
{
    public abstract class FilterRangeNode : GraphNode
    {
        private int? valueFrom;
        public int? ValueFrom
        {
            get => valueFrom;
            set
            {
                if (value == valueFrom) return;
                SetProperty(ref valueFrom, value, nameof(ValueFrom));
                OutputResult = null;
                PropagateForward(gn => gn.ClearResult(), applyToSelf: false);
            }
        }
        private int? valueTo;
        public int? ValueTo
        {
            get => valueTo;
            set
            {
                if (value == valueTo) return;
                SetProperty(ref valueTo, value, nameof(ValueTo));
                OutputResult = null;
                PropagateForward(gn => gn.ClearResult(), applyToSelf: false);
            }
        }

        protected override bool CanAddInput(GraphNode input) => !Inputs.Any();

        protected override void MapInputToOutput()
        {
            if (ValueFrom != null && ValueTo != null)
                OutputResult = InputResult[0].Where(t => ValueFrom <= GetValue(t) && GetValue(t) <= ValueTo).ToList();
            else if (ValueFrom != null)
                OutputResult = InputResult[0].Where(t => ValueFrom <= GetValue(t)).ToList();
            else if (ValueTo != null)
                OutputResult = InputResult[0].Where(t => GetValue(t) <= ValueTo).ToList();
            else
                OutputResult = InputResult[0];
        }
        protected abstract double? GetValue(Track t);
        public override bool IsValid => true;
    }
}
