using Backend.Entities.GraphNodes;
using SpotifySongTagger.Utils;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace SpotifySongTagger.ViewModels.Controls
{
    public class GraphNodeViewModel : BaseViewModel, ISelectable
    {
        private ObservableCollection<GraphNodeViewModel> AllGraphNodeViewModels { get; }
        public GraphNodeViewModel(GraphNode graphNode, ObservableCollection<GraphNodeViewModel> allGraphNodeViewModels)
        {
            GraphNode = graphNode;
            AllGraphNodeViewModels = allGraphNodeViewModels;
        }

        private double canvasWidth = 1;
        public double CanvasWidth
        {
            set
            {
                canvasWidth = value;
                NotifyPropertyChanged(nameof(X));
            }
        }
        private double canvasHeight = 1;
        public double CanvasHeight
        {
            set
            {
                canvasHeight = value;
                NotifyPropertyChanged(nameof(Y));
            }
        }

        private GraphNode graphNode;
        public GraphNode GraphNode
        {
            get => graphNode;
            set
            {
                SetProperty(ref graphNode, value, nameof(GraphNode));
                X = graphNode.X;
                Y = graphNode.Y;
            }
        }

        private double width;
        public double Width
        {
            get => width;
            set
            {
                width = value;
                NotifyPropertyChanged(nameof(Width));
                UpdateArrows(true);
            }
        }
        private double height;
        public double Height
        {
            get => height;
            set
            {
                SetProperty(ref height, value, nameof(Height));
                UpdateArrows(true);
            }
        }

        private double x;
        public double X
        {
            get => x * canvasWidth;
            set
            {
                SetProperty(ref x, value / canvasWidth, nameof(X));
                UpdateArrows(true);
            }
        }
        private double y;
        public double Y
        {
            get => y * canvasHeight;
            set
            {
                SetProperty(ref y, value / canvasHeight, nameof(Y));
                UpdateArrows(true);
            }
        }

        public void UpdateArrows(bool updatePreviousNodes)
        {
            if (AllGraphNodeViewModels == null) return;

            var nodeToVM = AllGraphNodeViewModels.ToDictionary(vm => vm.GraphNode, vm => vm);

            var outputs = GraphNode.Outputs.ToList();
            for (var i = 0; i < outputs.Count; i++)
            {
                var nextNodeVM = nodeToVM[outputs[i]];

                // use shortest path between nodes
                var fromRect = new Rect(X, Y, Width, Height);
                var toRect = new Rect(nextNodeVM.X, nextNodeVM.Y, nextNodeVM.Width, nextNodeVM.Height);
                var (start, end) = GeometryUtil.GetShortestPathBetweenRectangles(fromRect, toRect);
                var geometry = GeometryUtil.GetArrow(start, end);

                OutgoingArrows[i].Geometry = geometry;
            }


            if (updatePreviousNodes && AllGraphNodeViewModels != null)
            {
                // update previous node as only the outgoing arrows are drawn
                foreach (var prevNode in GraphNode.Inputs)
                    nodeToVM[prevNode].UpdateArrows(false);
            }
        }
        public void GenerateArrows()
        {
            var arrowVMs = new ObservableCollection<GraphNodeArrowViewModel>();
            var nodeToVM = AllGraphNodeViewModels.ToDictionary(vm => vm.GraphNode, vm => vm);

            foreach (var nextNode in GraphNode.Outputs)
                arrowVMs.Add(new GraphNodeArrowViewModel(GraphNode, nextNode));

            OutgoingArrows = arrowVMs;
            UpdateArrows(false);
        }

        private ObservableCollection<GraphNodeArrowViewModel> outgoingArrows;
        public ObservableCollection<GraphNodeArrowViewModel> OutgoingArrows
        {
            get
            {
                if (outgoingArrows == null)
                    GenerateArrows();
                return outgoingArrows;
            }
            set => SetProperty(ref outgoingArrows, value, nameof(OutgoingArrows));
        }
        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value, nameof(IsSelected));
        }
    }
}
