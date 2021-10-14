using System.Collections.Generic;

namespace Backend.Entities.GraphNodes
{
    public class PlaylistInputLikedNode : PlaylistInputNode
    {
        protected override List<Track> GetTracks()
        {
            return DatabaseOperations.PlaylistTracks(Playlist.Id, includeAlbums: IncludedAlbums,
                includeArtists: IncludedArtists, includeTags: IncludedTags);
        }
    }
}
