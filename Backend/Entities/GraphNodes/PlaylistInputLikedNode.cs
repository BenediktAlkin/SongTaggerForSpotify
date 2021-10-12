using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes
{
    public class LikedPlaylistInputNode : PlaylistInputNode
    {
        protected override List<Track> GetTracks()
        {
            return DatabaseOperations.PlaylistTracks(Playlist.Id, includeAlbums: IncludedAlbums,
                includeArtists: IncludedArtists, includeTags: IncludedTags);
        }
    }
}
