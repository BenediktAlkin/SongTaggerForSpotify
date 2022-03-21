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
        }

        #region load GraphNodes
        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set => SetProperty(ref isLoading, value, nameof(IsLoading));
        }
        public async Task LoadGraphNodes()
        {
            Log.Information($"loading GraphNodes from {GraphGeneratorPage?.Name}");
            IsLoading = true;

            var loadTagsTask = BaseViewModel.DataContainer.LoadTagGroups();
            var loadSourcePlaylistsTask = DataContainer.LoadSourcePlaylists();

            var nodes = await Task.Run(() => DatabaseOperations.GetGraphNodes(GraphGeneratorPage));
            var nodeViewModels = nodes.Select(gn => new GraphNodeViewModel(gn, GraphNodeVMs));
            foreach (var nodeViewModel in nodeViewModels)
                GraphNodeVMs.Add(nodeViewModel);
            foreach (var nodeVM in GraphNodeVMs)
            {
                nodeVM.CanvasWidth = CanvasWidth;
                nodeVM.CanvasHeight = CanvasHeight;
            }

            await Task.WhenAll(loadTagsTask, loadSourcePlaylistsTask);
            IsLoading = false;
            Log.Information($"loaded {GraphNodeVMs.Count} GraphNodes from {GraphGeneratorPage?.Name}");
        }
        #endregion

        #region GraphNode calculations
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
                try
                {
                    foreach (var nodeVM in GraphNodeVMs)
                        nodeVM.GraphNode.CalculateInputResult();
                }
                catch (Exception e)
                {
                    Log.Error($"Error calculating InputResult {e.Message}");
                }
                Log.Information("Updated InputResults");
            });
            await RefreshInputResultsTask;
        }
        #endregion


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
            if (DatabaseOperations.AddGraphNode(newNode, GraphGeneratorPage))
            {
                // add in ui
                newNode.GraphGeneratorPage = GraphGeneratorPage;
                GraphGeneratorPage.GraphNodes.Add(newNode);
                var nodeVM = new GraphNodeViewModel(newNode, GraphNodeVMs);
                nodeVM.CanvasWidth = CanvasWidth;
                nodeVM.CanvasHeight = CanvasHeight;
                GraphNodeVMs.Add(nodeVM);
                // calculate input result
                newNode.CalculateInputResult();
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
            if (to == null) return;
            // store in db
            if (DatabaseOperations.AddGraphNodeConnection(ClickedNodeViewModel.GraphNode, to.GraphNode))
            {
                // update in ui
                ClickedNodeViewModel.GraphNode.AddOutput(to.GraphNode);
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
                // remove from db
                if (DatabaseOperations.DeleteGraphNode(nodeVM.GraphNode))
                {
                    // update graphNode
                    var inputs = nodeVM.GraphNode.Inputs.ToList();
                    var outputs = nodeVM.GraphNode.Outputs.ToList();
                    foreach (var input in inputs)
                        nodeVM.GraphNode.RemoveInput(input);
                    foreach (var output in outputs)
                        nodeVM.GraphNode.RemoveOutput(output);

                    // update ui
                    GraphNodeVMs.Remove(nodeVM);
                    foreach (var input in inputs)
                        GraphNodeVMs.First(nodeVM => nodeVM.GraphNode == input).GenerateArrows();

                    // update input results
                    foreach (var output in outputs)
                        output.PropagateForward(gn => gn.ClearResult());
                    await RefreshInputResults();
                }
            }
            if (SelectedObject is GraphNodeArrowViewModel arrowVM)
            {
                // remove from db
                if (DatabaseOperations.DeleteGraphNodeConnection(arrowVM.FromNode, arrowVM.ToNode))
                {
                    // update ui
                    arrowVM.FromNode.RemoveOutput(arrowVM.ToNode);
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

        #region special GraphNode updates
        public void PlaylistOutputNode_SetName(PlaylistOutputNode node, string newName)
        {
            // update in db
            if (DatabaseOperations.EditPlaylistOutputNodeName(node, newName))
            {
                // update in ui
                //node.PlaylistName = newName; // not required as binding is directly to the node
            }
        }
        public async Task RemoveNode_SwapSets(RemoveNode node)
        {
            // update in db
            if (DatabaseOperations.SwapRemoveNodeSets(node))
            {
                // update in ui
                node.SwapSets();
                await RefreshInputResults();
            }
        }
        public async Task FilterRangeNode_Edit(FilterRangeNode node, int? yearFrom, int? yearTo)
        {
            // update in db
            if (DatabaseOperations.EditFilterRangeNode(node, yearFrom, yearTo))
            {
                // update in ui
                //node.ValueFrom = yearFrom; // not required as binding is directly to the node
                //node.ValueTo = yearTo; // not required as binding is directly to the node
                await RefreshInputResults();
            }
        }

        public async Task AssignTagNode_TagChanged(AssignTagNode node, Tag tag)
        {
            if (tag == null) return;
            // update in db
            if (DatabaseOperations.EditAssignTagNode(node, tag))
            {
                // update in ui
                //node.Tag = tag; // not required as binding is directly to the node
                await RefreshInputResults();
            }
        }
        public async Task FilterTagNode_TagChanged(FilterTagNode node, Tag tag)
        {
            if (tag == null) return;
            // update in db
            if (DatabaseOperations.EditFilterTagNode(node, tag))
            {
                // update in ui
                //node.Tag = tag; // not required as binding is directly to the node
                await RefreshInputResults();
            }
        }
        public async Task FilterArtistNode_ArtistChanged(FilterArtistNode node, Artist artist)
        {
            if (artist == null) return;
            // update in db
            if (DatabaseOperations.EditFilterArtistNode(node, artist))
            {
                // update in ui
                //node.Artist = artist; // not required as binding is directly to the node
                await RefreshInputResults();
            }
        }
        public async Task PlaylistInputNode_PlaylistChanged(PlaylistInputNode node, Playlist playlist)
        {
            if (playlist == null) return;
            // update in db
            if (DatabaseOperations.EditPlaylistInputNode(node, playlist))
            {
                // update in ui
                //node.Playlist = playlist; // not required as binding is directly to the node
                await RefreshInputResults();
            }
        }

        #endregion

    }
}
