using Backend.Entities.GraphNodes;
using System.Collections.ObjectModel;
using System.Linq;
using Util;

namespace Backend.Entities
{
    public class GraphGeneratorPage : NotifyPropertyChangedBase
    {
        public int Id { get; set; }
        private string name;
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value, nameof(Name));
        }
        public ObservableCollection<GraphNode> GraphNodes { get; } = new();

        public void NotifyIsValidChanged() => NotifyPropertyChanged(nameof(IsValid));
        public bool IsValid => GraphNodes.All(gn => gn.IsValid);

        public GraphGeneratorPage()
        {
            GraphNodes.CollectionChanged += (sender, e) => NotifyPropertyChanged(nameof(IsValid));
        }
    }
}
