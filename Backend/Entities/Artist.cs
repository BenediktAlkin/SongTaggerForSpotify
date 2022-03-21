using System;
using System.Collections.Generic;

namespace Backend.Entities
{
    public class Artist : IEquatable<Artist>
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public List<Track> Tracks { get; set; }
        public List<Genre> Genres { get; set; }

        public override string ToString() => Name;
        public override bool Equals(object obj) => obj is Artist other ? Equals(other) : false;
        public bool Equals(Artist other) => Id == other.Id;
        public override int GetHashCode() => Id.GetHashCode();

    }
}
