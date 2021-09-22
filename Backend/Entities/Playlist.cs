using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Backend.Entities
{
    public class Playlist
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public List<Track> Tracks { get; set; }

        public override string ToString() => Name;
    }
}
