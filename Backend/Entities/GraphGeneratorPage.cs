using Backend.Entities.GraphNodes;
using Serilog;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
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
        public List<GraphNode> GraphNodes { get; set; } = new();

        
        private bool isRunning;
        [NotMapped]
        public bool IsRunning
        {
            get => isRunning;
            set => SetProperty(ref isRunning, value, nameof(IsRunning));
        }
        public async Task Run()
        {
            Log.Information($"Run page {Name}");
            IsRunning = true;

            var dbGraphNodes = DatabaseOperations.GetGraphNodes(this);
            var playlistOutputNodes = dbGraphNodes.Where(gn => gn is PlaylistOutputNode).Cast<PlaylistOutputNode>();
            var assignTagNodes = dbGraphNodes.Where(gn => gn is AssignTagNode).Cast<AssignTagNode>();
            foreach (var playlistOutputNode in playlistOutputNodes)
                await SpotifyOperations.SyncPlaylistOutputNode(playlistOutputNode);
            foreach (var assignTagNode in assignTagNodes)
                await DatabaseOperations.AssignTags(assignTagNode);

            IsRunning = false;
            Log.Information($"Finished page {Name}");
        }
    }
}
