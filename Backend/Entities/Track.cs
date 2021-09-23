using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Util;

namespace Backend.Entities
{
    public class Track : NotifyPropertyChangedBase, IEquatable<Track>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int DurationMs { get; set; }
        public DateTime AddedAt { get; set; }


        public string AlbumId { get; set; }
        public Album Album { get; set; }

        public List<Artist> Artists { get; set; }
        public List<Playlist> Playlists { get; set; }


        public ObservableCollection<Tag> Tags { get; set; } = new ObservableCollection<Tag>();


        public string ArtistsString => string.Join(", ", Artists.Select(a => a.Name));


        public override bool Equals(object obj) => obj is Track other ? Equals(other) : false;
        public bool Equals(Track other) => Id == other.Id;
        public override int GetHashCode() => Id.GetHashCode();
    }
}
