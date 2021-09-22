using Backend.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes
{
    public class InputNode : GraphNode
    {
        private string playlistId;
        public string PlaylistId 
        {
            get => playlistId;
            set
            {
                SetProperty(ref playlistId, value, nameof(PlaylistId));
                GraphGeneratorPage.NotifyIsValidChanged();
            }
        }
        private Playlist playlist;
        public Playlist Playlist 
        {
            get => playlist;
            set
            {
                SetProperty(ref playlist, value, nameof(Playlist));
                GraphGeneratorPage.NotifyIsValidChanged();
            } 
        }



        protected override bool CanAddInput(GraphNode input) => false;

        public override async Task<List<Track>> GetResult()
        {
            if (PlaylistId == Constants.LIKED_SONGS_PLAYLIST_ID)
                return await SpotifyOperations.LikedTracks();

            return await SpotifyOperations.PlaylistItems(PlaylistId);
        }
        public override string ToString() => $"{base.ToString()} {PlaylistId}";
        public override bool IsValid => !string.IsNullOrEmpty(PlaylistId) || Playlist != null;
    }
}
