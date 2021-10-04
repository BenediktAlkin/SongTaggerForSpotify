using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes
{
    public class MetaPlaylistInputNode : PlaylistInputNode
    {
        protected override Task<List<Track>> GetTracks()
        {
            return DatabaseOperations.MetaPlaylistTracks(Playlist.Id, includeAlbums: IncludedAlbums,
                includeArtists: IncludedArtists, includeTags: IncludedTags);
        }
    }
}
