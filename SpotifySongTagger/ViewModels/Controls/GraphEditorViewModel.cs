using Backend;
using Backend.Entities;
using Backend.Entities.GraphNodes;
using SpotifySongTagger.Utils;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SpotifySongTagger.ViewModels.Controls
{
    public class GraphEditorViewModel : BaseViewModel
    {
        public GraphEditorViewModel(GraphGeneratorPage ggp)
        {
            GraphGeneratorPage = ggp;
            GraphNodeVMs = new ObservableCollection<GraphNodeViewModel>();
            var nodeViewModels = ggp.GraphNodes.Select(gn => new GraphNodeViewModel(gn, GraphNodeVMs));
            foreach (var nodeViewModel in nodeViewModels)
                GraphNodeVMs.Add(nodeViewModel);
        }

        public void ClearAllInputResults()
        {
            foreach (var nodeVM in GraphNodeVMs)
                nodeVM.GraphNode.ClearResult();
        }
        public async Task RefreshInputResults()
        {
            foreach (var nodeVM in GraphNodeVMs)
                await nodeVM.GraphNode.CalculateInputResult();
        }

        private GraphGeneratorPage graphGeneratorPage;
        public GraphGeneratorPage GraphGeneratorPage
        {
            get => graphGeneratorPage;
            set => SetProperty(ref graphGeneratorPage, value, nameof(GraphGeneratorPage));
        }
        public ObservableCollection<GraphNodeViewModel> GraphNodeVMs { get; }


        public Point LastMousePos { get; set; }
        private MouseButton? pressedMouseButton;
        public MouseButton? PressedMouseButton
        {
            get => pressedMouseButton;
            set => SetProperty(ref pressedMouseButton, value, nameof(PressedMouseButton));
        }
        public void SaveMovedGraphNodePosition()
        {
            ClickedNodeViewModel.GraphNode.X = ClickedNodeViewModel.X / CanvasWidth;
            ClickedNodeViewModel.GraphNode.Y = ClickedNodeViewModel.Y / CanvasHeight;
            ConnectionManager.Instance.Database.SaveChanges();
        }
        public void AddGraphNode(NodeType nodeType, Point pos)
        {
            // add in db
            var newNode = (GraphNode)Activator.CreateInstance(nodeType.Type);
            newNode.X = pos.X / CanvasWidth;
            newNode.Y = pos.Y / CanvasHeight;
            GraphGeneratorPage.GraphNodes.Add(newNode);
            ConnectionManager.Instance.Database.SaveChanges();

            // add in ui
            var nodeVM = new GraphNodeViewModel(newNode, GraphNodeVMs);
            nodeVM.CanvasWidth = CanvasWidth;
            nodeVM.CanvasHeight = CanvasHeight;
            GraphNodeVMs.Add(nodeVM);
        }


        #region new arrow
        public Point NewArrowStartPoint { get; set; }
        private Geometry newArrow;
        public Geometry NewArrow
        {
            get => newArrow;
            set => SetProperty(ref newArrow, value, nameof(NewArrow));
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
            if (to == null) return;

            // store in db
            ClickedNodeViewModel.GraphNode.AddOutput(to.GraphNode);
            ConnectionManager.Instance.Database.SaveChanges();

            // update in ui
            ClickedNodeViewModel.GenerateArrows();
            ClickedNodeViewModel.UpdateArrows(false);

            // update input results
            await RefreshInputResults();
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
                ConnectionManager.Instance.Database.GraphNodes.Remove(nodeVM.GraphNode);
                ConnectionManager.Instance.Database.SaveChanges();

                // update ui
                GraphNodeVMs.Remove(nodeVM);
                foreach (var prevNode in nodeVM.GraphNode.Inputs)
                    GraphNodeVMs.First(nodeVM => nodeVM.GraphNode == prevNode).GenerateArrows();

                // update input results
                foreach (var successorNode in successorNodes)
                    successorNode.PropagateForward(gn => gn.ClearResult());
                await RefreshInputResults();
            }
            if (SelectedObject is GraphNodeArrowViewModel arrowVM)
            {
                // remove from db
                arrowVM.FromNode.RemoveOutput(arrowVM.ToNode);
                ConnectionManager.Instance.Database.SaveChanges();

                // update ui
                GraphNodeVMs.First(nodeVM => nodeVM.GraphNode == arrowVM.FromNode).GenerateArrows();

                // update input results
                arrowVM.ToNode.PropagateForward(gn => gn.ClearResult());
                await RefreshInputResults();
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
