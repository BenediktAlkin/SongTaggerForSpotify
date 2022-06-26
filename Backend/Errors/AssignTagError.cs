using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Errors
{
    public class AssignTagError : Error
    {
        public static readonly SyncPlaylistOutputNodeErrors ContainsInvalidNode = new() { ErrorCode = 0};
    }
}
