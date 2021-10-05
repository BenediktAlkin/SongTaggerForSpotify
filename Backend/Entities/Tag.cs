using Backend.Entities.GraphNodes;
using System.Collections.Generic;
using Util;

namespace Backend.Entities
{
    public class Tag : NotifyPropertyChangedBase
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
    }
}
