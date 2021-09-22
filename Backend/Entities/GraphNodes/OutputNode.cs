using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes
{
    public class OutputNode : GraphNode
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

        protected override bool CanAddInput(GraphNode input) => Inputs.Count() < 1;
        protected override bool CanAddOutput(GraphNode output) => false;

        public override async Task<List<Track>> GetResult()
        {
            if (Inputs == null || Inputs.Count() == 0)
                return new List<Track>();

            return await Inputs.First().GetResult();
        }

        public override string ToString() => $"{base.ToString()} {PlaylistName}";
        public override bool IsValid => !string.IsNullOrEmpty(PlaylistName);
    }
}
