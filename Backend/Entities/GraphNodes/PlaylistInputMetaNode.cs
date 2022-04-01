using System.Collections.Generic;

namespace Backend.Entities.GraphNodes
{
    public class PlaylistInputMetaNode : PlaylistInputNode
    {
        protected override List<Track> GetTracks()
        {
            return DatabaseOperations.MetaPlaylistTracks(Playlist.Id, includeAlbums: IncludedAlbums,
                includeArtists: IncludedArtists, includeTags: IncludedTags, includeAudioFeatures: IncludedAudioFeatures,
                includeArtistGenres: IncludedGenres);
        }
    }
}
