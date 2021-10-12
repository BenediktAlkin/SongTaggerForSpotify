using Backend.Entities.GraphNodes;
using System;
using System.Collections.Generic;
using Util;

namespace Backend.Entities
{
    public class Tag : NotifyPropertyChangedBase, IEquatable<Tag>
    {
        public int Id { get; set; }

        private string name;
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value, nameof(Name));
        }

        public List<Track> Tracks { get; set; }
        public List<AssignTagNode> AssignTagNodes { get; set; }
        public List<FilterTagNode> FilterTagNodes { get; set; }


        public override string ToString() => Name;

        public override bool Equals(object obj) => obj is Tag other ? Equals(other) : false;
        public bool Equals(Tag other) => Id == other.Id;
        public override int GetHashCode() => Id.GetHashCode();
    }
}
