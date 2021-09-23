using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
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
                GraphGeneratorPage?.NotifyIsValidChanged();
            }
        }
        private Playlist playlist;
        public Playlist Playlist
        {
            get => playlist;
            set
            {
                SetProperty(ref playlist, value, nameof(Playlist));
                GraphGeneratorPage?.NotifyIsValidChanged();
            }
        }



        protected override bool CanAddInput(GraphNode input) => false;

        public override async Task<List<Track>> GetResult()
        {
            IQueryable<Track> tracks = Db.Tracks;
            if (AnyForward(gn => gn.RequiresArtists))
                tracks = tracks.Include(t => t.Artists);
            if (AnyForward(gn => gn.RequiresTags))
                tracks = tracks.Include(t => t.Tags);

            return await tracks.Where(t => t.Playlists.Contains(Playlist)).ToListAsync();
        }

        public override string ToString() => $"{base.ToString()} {PlaylistId}";
        public override bool IsValid => !string.IsNullOrEmpty(PlaylistId) || Playlist != null;
    }
}
