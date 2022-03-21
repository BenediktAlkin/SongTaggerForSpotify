using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entities
{
    public class Genre : IEquatable<Genre>
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<Artist> Artists { get; set; }

        public override string ToString() => Name;
        public override bool Equals(object obj) => obj is Genre other ? Equals(other) : false;
        public bool Equals(Genre other) => Id == other.Id;
        public override int GetHashCode() => Id.GetHashCode();
    }
}
