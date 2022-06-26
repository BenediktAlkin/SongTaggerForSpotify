using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Errors
{
    public class SyncPlaylistOutputNodeErrors : Error
    {
        public static readonly SyncPlaylistOutputNodeErrors ContainsInvalidNode = new() { ErrorCode = 0};
        public static readonly SyncPlaylistOutputNodeErrors FailedCreatePlaylist = new() { ErrorCode = 1 };
        public static readonly SyncPlaylistOutputNodeErrors FailedLike = new() { ErrorCode = 2 };
        public static readonly SyncPlaylistOutputNodeErrors FailedRename = new() { ErrorCode = 3 };
        public static readonly SyncPlaylistOutputNodeErrors FailedRemoveOldTracks = new() { ErrorCode = 4 };
        public static readonly SyncPlaylistOutputNodeErrors FailedAddNewTrack = new() { ErrorCode = 5 };
        public static readonly SyncPlaylistOutputNodeErrors ReachedSizeLimit = new() { ErrorCode = 6 };
        public static readonly SyncPlaylistOutputNodeErrors SpotifyAPIRemoveUnstable = new() { ErrorCode = 7 };
    }
}
