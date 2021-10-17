using System.Linq;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes
{
    public class PlaylistOutputNode : GraphNode, IRunnableGraphNode
    {
        private string playlistName;
        public string PlaylistName
        {
            get => playlistName;
            set => SetProperty(ref playlistName, value, nameof(PlaylistName));
        }
        public string GeneratedPlaylistId { get; set; }

        protected override bool CanAddInput(GraphNode input) => !Inputs.Any();
        protected override bool CanAddOutput(GraphNode output) => false;
        protected override void MapInputToOutput() => OutputResult = InputResult[0];

        public override bool IsValid => !string.IsNullOrEmpty(PlaylistName);

        public async Task<bool> Run() => await SpotifyOperations.SyncPlaylistOutputNode(this);
    }
}
