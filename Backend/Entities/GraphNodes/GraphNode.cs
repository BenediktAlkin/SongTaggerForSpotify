using System.Collections.Generic;
using System.Threading.Tasks;
using Util;

namespace Backend.Entities.GraphNodes
{
    public abstract class GraphNode : NotifyPropertyChangedBase
    {
        public int Id { get; set; }

        public int GraphGeneratorPageId { get; set; }
        public GraphGeneratorPage GraphGeneratorPage { get; set; }

        private double x = 0.7;
        public double X
        {
            get => x;
            set => SetProperty(ref x, value, nameof(X));
        }
        private double y = 0.3;
        public double Y
        {
            get => y;
            set => SetProperty(ref y, value, nameof(Y));
        }


        public IEnumerable<GraphNode> Inputs { get; set; } = new List<GraphNode>();
        public IEnumerable<GraphNode> Outputs { get; set; } = new List<GraphNode>();

        public void AddInput(GraphNode input)
        {

            if (CanAddInput(input) && input.CanAddOutput(this))
                ((List<GraphNode>)Inputs).Add(input);

            if (HasCycles(this))
                RemoveOutput(input);
        }
        public void RemoveInput(GraphNode input) => ((List<GraphNode>)Inputs).Remove(input);

        protected virtual bool CanAddInput(GraphNode input) => true;

        public void AddOutput(GraphNode output)
        {
            if (CanAddOutput(output) && output.CanAddInput(this))
                ((List<GraphNode>)Outputs).Add(output);

            if (HasCycles(this))
                RemoveOutput(output);
        }
        public void RemoveOutput(GraphNode output) => ((List<GraphNode>)Outputs).Remove(output);
        protected virtual bool CanAddOutput(GraphNode output) => true;


        private static bool HasCycles(GraphNode curNode, List<int> seenNodes = null) => HasForwardCycles(curNode) || HasBackwardCycles(curNode);
        private static bool HasForwardCycles(GraphNode curNode, List<int> seenNodes = null)
        {
            if (seenNodes == null)
                seenNodes = new List<int>();

            if (seenNodes.Contains(curNode.Id))
                return true;
            seenNodes.Add(curNode.Id);

            foreach (var next in curNode.Outputs)
            {
                if (HasForwardCycles(next, seenNodes))
                    return true;
            }
            return false;
        }
        private static bool HasBackwardCycles(GraphNode curNode, List<int> seenNodes = null)
        {
            if (seenNodes == null)
                seenNodes = new List<int>();

            if (seenNodes.Contains(curNode.Id))
                return true;
            seenNodes.Add(curNode.Id);

            foreach (var prev in curNode.Inputs)
            {
                if (HasBackwardCycles(prev, seenNodes))
                    return true;
            }
            return false;
        }


        public abstract Task<List<Track>> GetResult();
        public virtual bool IsValid => true;

        public override string ToString() => GetType().Name;
    }
}
