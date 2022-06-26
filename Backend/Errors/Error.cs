using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Errors
{
    public abstract class Error
    {
        public int ErrorCode { get; init; }


        public override bool Equals(object obj) => obj is Error e && ErrorCode == e.ErrorCode;
        public override int GetHashCode() => HashCode.Combine(this.GetType().GetHashCode(), ErrorCode.GetHashCode());
    }
}
