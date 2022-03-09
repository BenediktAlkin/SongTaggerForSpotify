using Backend.Entities.GraphNodes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using Util;

namespace Backend.Entities
{
    public class TagGroup : NotifyPropertyChangedBase, IEquatable<TagGroup>
    {
        public int Id { get; set; }

        private string name;
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value, nameof(Name));
        }

        public int? Order { get; set; }

        public IList<Tag> Tags { get; set; } = new ObservableCollection<Tag>();

        public override string ToString() => Name;

        public override bool Equals(object obj) => obj is TagGroup other ? Equals(other) : false;
        public bool Equals(TagGroup other) => Id == other.Id;
        public override int GetHashCode() => Id.GetHashCode();
    }
}
