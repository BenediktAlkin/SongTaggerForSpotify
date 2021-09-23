using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Util;

namespace Backend.Entities.GraphNodes
{
    public abstract class GraphNode : NotifyPropertyChangedBase
    {
        public static DatabaseContext Db => ConnectionManager.Instance.Database;

        public int Id { get; set; }

        public int GraphGeneratorPageId { get; set; }
        public GraphGeneratorPage GraphGeneratorPage { get; set; }

        #region position
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
        #endregion

        #region inputs & outputs
        public IEnumerable<GraphNode> Inputs { get; set; } = new List<GraphNode>();
        public IEnumerable<GraphNode> Outputs { get; set; } = new List<GraphNode>();

        public void AddInput(GraphNode input)
        {

            if (CanAddInput(input) && input.CanAddOutput(this))
                ((List<GraphNode>)Inputs).Add(input);

            if (HasCycles(this))
                RemoveOutput(input);
            else
                PropagateBackward(gn => gn.OnConnectionAdded(input, this));
        }
        public void AddOutput(GraphNode output)
        {
            if (CanAddOutput(output) && output.CanAddInput(this))
                ((List<GraphNode>)Outputs).Add(output);

            if (HasCycles(this))
                RemoveOutput(output);
            else
                PropagateBackward(gn => gn.OnConnectionAdded(this, output));
        }

        public void RemoveInput(GraphNode input) => ((List<GraphNode>)Inputs).Remove(input);
        public void RemoveOutput(GraphNode output) => ((List<GraphNode>)Outputs).Remove(output);

        protected virtual bool CanAddInput(GraphNode input) => true;
        protected virtual bool CanAddOutput(GraphNode output) => true;

        protected virtual void OnConnectionAdded(GraphNode from, GraphNode to) { }
        #endregion

        #region validity checks
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
        #endregion


        private List<Track> inputResult;
        public List<Track> InputResult
        {
            get => inputResult;
            set
            {
                SetProperty(ref inputResult, value, nameof(InputResult));
                OnInputResultChanged();
            }
        }
        private List<Track> outputResult;
        public List<Track> OutputResult
        {
            get => outputResult;
            set => SetProperty(ref outputResult, value, nameof(OutputResult));
        }
        protected virtual void OnInputResultChanged() { }
        public void ClearResult()
        {
            InputResult = null;
            OutputResult = null;
        }
        public virtual async Task CalculateInputResult()
        {
            if (InputResult != null) return;

            if (Inputs == null || !Inputs.Any())
                InputResult = new List<Track>();
            else
            {
                var concated = new List<Track>();
                foreach (var input in Inputs)
                {
                    await input.CalculateOutputResult();
                    concated.AddRange(input.OutputResult);
                }
                InputResult = concated;
                Log.Information($"Calculated InputResult for {this} (count={InputResult?.Count})");
            }
        }
        protected virtual Task MapInputToOutput()
        {
            OutputResult = InputResult;
            return Task.CompletedTask;
        }
        public async Task CalculateOutputResult()
        {
            if (OutputResult != null) return;

            if (InputResult == null)
                await CalculateInputResult();
            await MapInputToOutput();
        }


        public virtual bool IsValid => true;
        public virtual bool RequiresTags => false;
        public virtual bool RequiresArtists => false;


        public bool AnyForward(Func<GraphNode, bool> predicate) => predicate(this) || Outputs.Any(o => o.AnyForward(predicate));
        public bool AnyBackward(Func<GraphNode, bool> predicate) => predicate(this) || Inputs.Any(i => i.AnyBackward(predicate));
        public void PropagateForward(Action<GraphNode> action)
        {
            action(this);
            foreach (var output in Outputs)
                output.PropagateForward(action);
        }
        public void PropagateBackward(Action<GraphNode> action)
        {
            action(this);
            foreach (var input in Inputs)
                input.PropagateBackward(action);
        }

        public override string ToString() => GetType().Name;
    }
}
