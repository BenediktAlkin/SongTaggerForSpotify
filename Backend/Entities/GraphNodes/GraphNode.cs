using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading;
using Util;

namespace Backend.Entities.GraphNodes
{
    public abstract class GraphNode : NotifyPropertyChangedBase, IEquatable<GraphNode>
    {
        protected static ILogger Logger { get; } = Log.ForContext("SourceContext", "GN");

        #region navigation properties
        // only needed to change on delete behaviour
        public List<RemoveNode> RemoveNodeBaseSets { get; set; }
        // only needed to change on delete behaviour
        public List<RemoveNode> RemoveNodeRemoveSets { get; set; }
        #endregion

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
            if (!CanAddInput(input) || !input.CanAddOutput(this))
            {
                Logger.Information($"Connection {input} to {this} is invalid");
                return;
            }
            if (Inputs.Contains(input) || input.Outputs.Contains(this))
            {
                Logger.Information($"Connection {input} to {this} already exists");
                return;
            }
            ((List<GraphNode>)Inputs).Add(input);
            ((List<GraphNode>)input.Outputs).Add(this);

            if (HasCycles(this))
            {
                Logger.Information($"Connection {input} to {this} would introduce cycle");
                RemoveInput(input);
                input.RemoveOutput(this);
                return;
            }
            Logger.Information($"Connected {input} to {this}");
            PropagateBackward(gn => gn.OnConnectionAdded(input, this));
            PropagateForward(gn => gn.ClearResult());
            OnAddInput(input);
        }
        public void AddOutput(GraphNode output)
        {
            if (!CanAddOutput(output) || !output.CanAddInput(this))
            {
                Logger.Information($"Connection {this} to {output} is invalid");
                return;
            }
            if (Outputs.Contains(output) || output.Inputs.Contains(this))
            {
                Logger.Information($"Connection {this} to {output} already exists");
                return;
            }
            ((List<GraphNode>)Outputs).Add(output);
            ((List<GraphNode>)output.Inputs).Add(this);

            if (HasCycles(this))
            {
                Logger.Information($"Connection {this} to {output} would introduce cycle");
                RemoveOutput(output);
                output.RemoveInput(this);
                return;
            }
            Logger.Information($"Connected {this} to {output}");
            PropagateBackward(gn => gn.OnConnectionAdded(this, output));
            output.PropagateForward(gn => gn.ClearResult());
            output.OnAddInput(this);
        }

        protected virtual void OnAddInput(GraphNode input) { }
        protected virtual void OnRemoveInput(GraphNode input) { }

        public void RemoveInput(GraphNode input)
        {
            ((List<GraphNode>)Inputs).Remove(input);
            ((List<GraphNode>)input.Outputs).Remove(this);
            OnRemoveInput(input);
        }
        public void RemoveOutput(GraphNode output)
        {
            ((List<GraphNode>)Outputs).Remove(output);
            ((List<GraphNode>)output.Inputs).Remove(this);
            output.OnRemoveInput(this);
        }

        protected virtual bool CanAddInput(GraphNode input) => true;
        protected virtual bool CanAddOutput(GraphNode output) => true;

        protected virtual void OnConnectionAdded(GraphNode from, GraphNode to) { }
        #endregion

        #region validity checks
        private static bool HasCycles(GraphNode rootNode)
            => HasForwardCycles(rootNode, new()) || HasBackwardCycles(rootNode, new());
        private static bool HasForwardCycles(GraphNode rootNode, List<GraphNode> seenNodes, GraphNode curNode = null)
        {
            if (seenNodes.Contains(rootNode))
                return true;
            if (curNode == null)
                curNode = rootNode;
            else
                seenNodes.Add(curNode);

            foreach (var next in curNode.Outputs)
            {
                if (HasForwardCycles(rootNode, seenNodes, next))
                    return true;
            }
            return false;
        }
        private static bool HasBackwardCycles(GraphNode rootNode, List<GraphNode> seenNodes, GraphNode curNode = null)
        {
            if (seenNodes.Contains(rootNode))
                return true;
            if (curNode == null)
                curNode = rootNode;
            else
                seenNodes.Add(curNode);

            foreach (var prev in curNode.Inputs)
            {
                if (HasBackwardCycles(rootNode, seenNodes, prev))
                    return true;
            }
            return false;
        }
        #endregion


        private List<List<Track>> inputResult;
        [NotMapped]
        public List<List<Track>> InputResult
        {
            get => inputResult;
            set
            {
                SetProperty(ref inputResult, value, nameof(InputResult));
                OnInputResultChanged();
            }
        }
        private List<Track> outputResult;
        [NotMapped]
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
        public void CalculateInputResult()
        {
            if (InputResult != null) return;

            CalculateInputResultImpl();
                
        }
        protected virtual void CalculateInputResultImpl()
        {
            var newInputResults = new List<List<Track>>();
            var hasValidInput = false;
            foreach (var input in Inputs ?? Enumerable.Empty<GraphNode>())
            {
                input.CalculateOutputResult();
                if (input.OutputResult != null)
                {
                    newInputResults.Add(input.OutputResult);
                    hasValidInput = true;
                }
            }
            if (hasValidInput)
            {
                var inputResultCounts = string.Join(',', newInputResults.Select(ir => $"{ir.Count}"));
                Logger.Information($"Calculated InputResult for {this} (counts={inputResultCounts})");
                InputResult = newInputResults;
            }
            else
            {
                Logger.Information($"{this} has no valid Inputs --> InputResult = empty list");
                InputResult = new() { new() };
            }
        }
        protected virtual void MapInputToOutput() { }
        public void CalculateOutputResult()
        {
            if (OutputResult != null) return;

            if (InputResult == null)
                CalculateInputResult();

            if (InputResult.Count > 0 && IsValid)
            {
                MapInputToOutput();
                Logger.Information($"Calculated OutputResult for {this} (count={OutputResult.Count})");
            }
            else
            {
                OutputResult = new();
                Logger.Information($"{this} is invalid --> OutputResult = empty list");
            }
        }



        public virtual bool IsValid => true;
        public virtual bool RequiresTags => false;
        public virtual bool RequiresArtists => false;
        public virtual bool RequiresAlbums => false;


        public bool AnyForward(Func<GraphNode, bool> predicate) => predicate(this) || Outputs.Any(o => o.AnyForward(predicate));
        public bool AnyBackward(Func<GraphNode, bool> predicate) => predicate(this) || Inputs.Any(i => i.AnyBackward(predicate));
        public void PropagateForward(Action<GraphNode> action, bool applyToSelf = true)
        {
            if (applyToSelf)
                action(this);
            foreach (var output in Outputs)
                output.PropagateForward(action);
        }
        public void PropagateBackward(Action<GraphNode> action, bool applyToSelf = true)
        {
            if (applyToSelf)
                action(this);
            foreach (var input in Inputs)
                input.PropagateBackward(action);
        }

        public override string ToString() => GetType().Name;

        public override bool Equals(object obj) => obj is GraphNode other ? Equals(other) : false;
        public bool Equals(GraphNode other) => Id == other.Id;
        public override int GetHashCode() => Id.GetHashCode();
    }
}
