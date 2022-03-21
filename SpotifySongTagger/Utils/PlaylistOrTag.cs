using Backend.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifySongTagger.Utils
{
    public class PlaylistOrTag : IEquatable<PlaylistOrTag>
    {
        public Playlist Playlist { get; }
        public Tag Tag { get; }
        public PlaylistOrTag(Playlist playlist) => Playlist = playlist;
        public PlaylistOrTag(Tag tag) => Tag = tag;

        public override string ToString()
        {
            if (Playlist != null)
                return $"Playlist {Playlist}";
            return $"Tag {Tag}";
        }

        public override bool Equals(object obj) => obj is PlaylistOrTag other ? Equals(other) : false;
        public bool Equals(PlaylistOrTag other) => Playlist == other.Playlist && Tag == other.Tag;
        public override int GetHashCode() => Playlist != null ? Playlist.GetHashCode() : Tag.GetHashCode();
    }
}
