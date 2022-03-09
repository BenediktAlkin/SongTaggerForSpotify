using System;
using System.Collections.Generic;

namespace Backend.Entities
{
    public class Album : IEquatable<Album>
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string ReleaseDate { get; set; }
        public string ReleaseDatePrecision { get; set; }

        public int? ReleaseYear => string.IsNullOrWhiteSpace(ReleaseDate[0..4]) ? null : int.Parse(ReleaseDate[0..4]);

        public List<Track> Tracks { get; set; }

        public override bool Equals(object obj) => obj is Album other ? Equals(other) : false;
        public bool Equals(Album other) => Id == other.Id;
        public override int GetHashCode() => Id.GetHashCode();
    }
}
