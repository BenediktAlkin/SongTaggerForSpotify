using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
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


        public bool Equals(Track other) => Id == other.Id;
        public override int GetHashCode() => Name.GetHashCode();
    }
}
