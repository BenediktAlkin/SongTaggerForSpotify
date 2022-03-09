using Backend.Entities.GraphNodes;
using System;
using System.Collections.Generic;

namespace Backend.Entities
{
    public class Playlist : IEquatable<Playlist>
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public List<Track> Tracks { get; set; }
        public List<PlaylistInputNode> PlaylistInputNodes { get; set; }

        public override string ToString() => Name;

        public override bool Equals(object obj) => obj is Playlist other ? Equals(other) : false;
        public bool Equals(Playlist other) => Id == other.Id;
        public override int GetHashCode() => Id.GetHashCode();
    }
}
