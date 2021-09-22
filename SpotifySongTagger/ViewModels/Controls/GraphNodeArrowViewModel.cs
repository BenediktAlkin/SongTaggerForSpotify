using Backend.Entities.GraphNodes;
using SpotifySongTagger.Utils;
using System.Windows.Media;

namespace SpotifySongTagger.ViewModels.Controls
{
    public class GraphNodeArrowViewModel : BaseViewModel, ISelectable
    {
        public GraphNode FromNode { get; set; }
        public GraphNode ToNode { get; set; }
        public GraphNodeArrowViewModel(GraphNode fromNode, GraphNode toNode)
        {
            FromNode = fromNode;
            ToNode = toNode;
        }

        private Geometry geometry;
        public Geometry Geometry
        {
            get => geometry;
            set => SetProperty(ref geometry, value, nameof(Geometry));
        }
        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value, nameof(IsSelected));
        }
    }
}
