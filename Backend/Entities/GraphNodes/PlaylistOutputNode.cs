using System.Linq;

namespace Backend.Entities.GraphNodes
{
    public class PlaylistOutputNode : GraphNode
    {
        private string playlistName;
        public string PlaylistName
        {
            get => playlistName;
            set
            {
                SetProperty(ref playlistName, value, nameof(PlaylistName));
                GraphGeneratorPage?.NotifyIsValidChanged();
            }
        }
        public string GeneratedPlaylistId { get; set; }

        protected override bool CanAddInput(GraphNode input) => !Inputs.Any();
        protected override bool CanAddOutput(GraphNode output) => false;

        public override bool IsValid => !string.IsNullOrEmpty(PlaylistName);
    }
}
