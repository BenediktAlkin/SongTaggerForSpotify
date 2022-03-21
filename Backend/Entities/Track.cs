using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Util;

namespace Backend.Entities
{
    public class Track : NotifyPropertyChangedBase, IEquatable<Track>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int DurationMs { get; set; }
        public string DurationString => TimeSpan.FromMilliseconds(DurationMs).ToString(@"mm\:ss");
        public bool IsLiked { get; set; }


        public string AlbumId { get; set; }
        public Album Album { get; set; }

        public List<Artist> Artists { get; set; }
        public List<Playlist> Playlists { get; set; } = new();
        public string AudioFeaturesId { get; set; }
        public AudioFeatures AudioFeatures { get; set; }

        public IList<Tag> Tags { get; set; } = new ObservableCollection<Tag>();


        public string ArtistsString => Artists == null ? string.Empty : string.Join(", ", Artists.Select(a => a.Name));
        public string TagsString => Tags == null ? string.Empty : string.Join(", ", Tags.Select(t => t.Name));
        public string ArtistsGenresString => Artists == null ? string.Empty : string.Join(", ", Artists.SelectMany(a => a.Genres).Select(g => g.Name).Distinct().OrderBy(g => g));


        public override bool Equals(object obj) => obj is Track other ? Equals(other) : false;
        public bool Equals(Track other) => Id == other.Id;
        public override int GetHashCode() => Id.GetHashCode();
    }
}
