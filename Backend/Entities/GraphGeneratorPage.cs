using Backend.Entities.GraphNodes;
using Serilog;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Util;

namespace Backend.Entities
{
    public class GraphGeneratorPage : NotifyPropertyChangedBase
    {
        public GraphGeneratorPage()
        {
            GraphNodes.CollectionChanged += (sender, e) => NotifyPropertyChanged(nameof(IsValid));
        }

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


        private bool isRunning;
        [NotMapped]
        public bool IsRunning
        {
            get => isRunning;
            set => SetProperty(ref isRunning, value, nameof(IsRunning));
        }
        public async Task Run()
        {
            var playlistOutputNodes = GraphNodes.Where(gn => gn is PlaylistOutputNode).Cast<PlaylistOutputNode>();
            var assignTagNodes = GraphNodes.Where(gn => gn is AssignTagNode).Cast<AssignTagNode>();
            Log.Information($"Run page {Name}");
            IsRunning = true;
            foreach (var playlistOutputNode in playlistOutputNodes)
                await SpotifyOperations.SyncPlaylistOutputNode(playlistOutputNode);
            foreach (var assignTagNode in assignTagNodes)
                await DatabaseOperations.AssignTags(assignTagNode);
            IsRunning = false;
            Log.Information($"Finished page {Name}");
        }
    }
}
