using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes
{
    public class LikedPlaylistInputNode : PlaylistInputNode
    {
        protected override Task<List<Track>> GetTracks()
        {
            return DatabaseOperations.PlaylistTracks(Playlist.Id, includeAlbums: IncludedAlbums,
                includeArtists: IncludedArtists, includeTags: IncludedTags);
        }
    }
}
