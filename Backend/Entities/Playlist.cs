using Backend.Entities.GraphNodes;
using System.Collections.Generic;

namespace Backend.Entities
{
    public class Playlist
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public List<Track> Tracks { get; set; }
        public List<PlaylistInputNode> PlaylistInputNodes { get; set; }

        public override string ToString() => Name;
    }
}
