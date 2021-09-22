using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
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


        public override string ToString() => Name;
    }
}
