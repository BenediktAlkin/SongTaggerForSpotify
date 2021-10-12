using Backend;
using Backend.Entities;
using Backend.Entities.GraphNodes;
using Serilog;
using SpotifySongTagger.Utils;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using static SpotifySongTagger.Utils.GeometryUtil;

namespace SpotifySongTagger.ViewModels.Controls
{
    public class GraphEditorViewModel : BaseViewModel
    {
        public GraphEditorViewModel(GraphGeneratorPage ggp)
        {
            GraphGeneratorPage = ggp;
            var nodeViewModels = ggp.GraphNodes.Select(gn => new GraphNodeViewModel(gn, GraphNodeVMs));
            foreach (var nodeViewModel in nodeViewModels)
                GraphNodeVMs.Add(nodeViewModel);
        }

        private bool isUpdatingInputResults;
        public bool IsUpdatingInputResults
        {
            get => isUpdatingInputResults;
            set => SetProperty(ref isUpdatingInputResults, value, nameof(IsUpdatingInputResults));
        }

        public void ClearAllInputResults()
        {
            foreach (var nodeVM in GraphNodeVMs)
                nodeVM.GraphNode.ClearResult();
        }
        private Task RefreshInputResultsTask { get; set; }
        public async Task RefreshInputResults()
        {
            if (RefreshInputResultsTask != null && !RefreshInputResultsTask.IsCompleted)
                await RefreshInputResultsTask;

            RefreshInputResultsTask = Task.Run(() =>
            {
                IsUpdatingInputResults = true;
                try
                {
                    foreach (var nodeVM in GraphNodeVMs)
                        nodeVM.GraphNode.CalculateInputResult();
                }catch(Exception e)
                {
                    Log.Error($"Error calculating InputResult {e.Message}");
                }
                Log.Information("Updated InputResults");
                IsUpdatingInputResults = false;
            });
            await RefreshInputResultsTask;
        }

        private GraphGeneratorPage graphGeneratorPage;
        public GraphGeneratorPage GraphGeneratorPage
        {
            get => graphGeneratorPage;
            set => SetProperty(ref graphGeneratorPage, value, nameof(GraphGeneratorPage));
        }
        public ObservableCollection<GraphNodeViewModel> GraphNodeVMs { get; } = new();


        public Point LastMousePos { get; set; }
        private MouseButton? pressedMouseButton;
        public MouseButton? PressedMouseButton
        {
            get => pressedMouseButton;
            set => SetProperty(ref pressedMouseButton, value, nameof(PressedMouseButton));
        }
        public void SaveMovedGraphNodePosition()
        {
            var newX = ClickedNodeViewModel.X / CanvasWidth;
            var newY = ClickedNodeViewModel.Y / CanvasHeight;

            // update in db
            if (DatabaseOperations.EditGraphNode(ClickedNodeViewModel.GraphNode, newX, newY))
            {
                // update ui
                ClickedNodeViewModel.GraphNode.X = newX;
                ClickedNodeViewModel.GraphNode.Y = newY;
            }
        }
        public void AddGraphNode(NodeType nodeType, Point pos)
        {
            // add in db
            var newNode = (GraphNode)Activator.CreateInstance(nodeType.Type);
            newNode.X = pos.X / CanvasWidth;
            newNode.Y = pos.Y / CanvasHeight;
            newNode.GraphGeneratorPage = GraphGeneratorPage;
            if (DatabaseOperations.AddGraphNode(newNode))
            {
                // add in ui
                var nodeVM = new GraphNodeViewModel(newNode, GraphNodeVMs);
                nodeVM.CanvasWidth = CanvasWidth;
                nodeVM.CanvasHeight = CanvasHeight;
                GraphNodeVMs.Add(nodeVM);
            }
        }


        #region new arrow
        public Rect? NewArrowStartNodeRect { get; set; }
        private Geometry newArrow;
        public Geometry NewArrow
        {
            get => newArrow;
            set => SetProperty(ref newArrow, value, nameof(NewArrow));
        }
        public void UpdateNewArrow(Point curPos)
        {
            if (NewArrowStartNodeRect == null) return;
            var nearestAnchor = GeometryUtil.GetNearestAnchor(curPos, NewArrowStartNodeRect.Value);
            NewArrow = GeometryUtil.GetArrow(nearestAnchor, new Anchor(GeometryUtil.OppositeLocation(nearestAnchor.Location), curPos));
        }
        public GraphNodeViewModel GetHoveredGraphNodeViewModel(Point pos)
        {
            foreach (var nodeVM in GraphNodeVMs)
            {
                if (pos.X >= nodeVM.X &&
                    pos.Y >= nodeVM.Y &&
                    pos.X <= nodeVM.X + nodeVM.Width &&
                    pos.Y <= nodeVM.Y + nodeVM.Height)
                    return nodeVM;
            }
            return null;
        }
        public async Task AddConnection(GraphNodeViewModel to)
        {
            // store in db
            if (DatabaseOperations.AddGraphNodeConnection(ClickedNodeViewModel.GraphNode, to.GraphNode))
            {
                // update in ui
                ClickedNodeViewModel.GenerateArrows();
                ClickedNodeViewModel.UpdateArrows(false);

                // update input results
                await RefreshInputResults();
            }
        }
        #endregion

        #region selected object
        public GraphNodeViewModel ClickedNodeViewModel { get; set; }
        private ISelectable selectedObject;
        public ISelectable SelectedObject
        {
            get => selectedObject;
            set
            {
                if (selectedObject != null)
                    selectedObject.IsSelected = false;
                SetProperty(ref selectedObject, value, nameof(SelectedObject));
                if (value != null)
                    selectedObject.IsSelected = true;
            }
        }
        public async Task DeleteSelected()
        {
            if (SelectedObject == null) return;

            if (SelectedObject is GraphNodeViewModel nodeVM)
            {
                var successorNodes = nodeVM.GraphNode.Outputs;
                // remove from db
                if (DatabaseOperations.DeleteGraphNode(nodeVM.GraphNode))
                {
                    // update ui
                    GraphNodeVMs.Remove(nodeVM);
                    foreach (var prevNode in nodeVM.GraphNode.Inputs)
                        GraphNodeVMs.First(nodeVM => nodeVM.GraphNode == prevNode).GenerateArrows();

                    // update input results
                    foreach (var successorNode in successorNodes)
                        successorNode.PropagateForward(gn => gn.ClearResult());
                    await RefreshInputResults();
                }
            }
            if (SelectedObject is GraphNodeArrowViewModel arrowVM)
            {
                // remove from db
                if(DatabaseOperations.DeleteGraphNodeConnection(arrowVM.FromNode, arrowVM.ToNode))
                {
                    // update ui
                    GraphNodeVMs.First(nodeVM => nodeVM.GraphNode == arrowVM.FromNode).GenerateArrows();

                    // update input results
                    arrowVM.ToNode.PropagateForward(gn => gn.ClearResult());
                    await RefreshInputResults();
                }
            }
        }
        #endregion

        #region canvas size
        private double CanvasWidth { get; set; }
        private double CanvasHeight { get; set; }
        public void UpdateCanvasSize(double newWidth, double newHeight)
        {
            CanvasWidth = newWidth;
            CanvasHeight = newHeight;
            foreach (var nodeVM in GraphNodeVMs)
            {
                nodeVM.CanvasWidth = newWidth;
                nodeVM.CanvasHeight = newHeight;
            }
            foreach (var nodeVM in GraphNodeVMs)
                nodeVM.UpdateArrows(false);
        }
        #endregion

    }
}
