using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes
{
    public class PlaylistInputNode : GraphNode
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



        protected override void OnConnectionAdded(GraphNode from, GraphNode to)
        {
            if ((to.RequiresArtists && !IncludedArtists) ||
                (to.RequiresTags && !IncludedTags))
                ClearResult();
        }
        protected override bool CanAddInput(GraphNode input) => false;
        private bool IncludedArtists { get; set; }
        private bool IncludedTags { get; set; }
        public override async Task CalculateInputResult()
        {
            if (InputResult != null) return;

            IncludedArtists = false;
            IncludedTags = false;

            IQueryable<Track> tracks = Db.Tracks;
            if (AnyForward(gn => gn.RequiresArtists))
            {
                tracks = tracks.Include(t => t.Artists);
                IncludedArtists = true;
            }
            if (AnyForward(gn => gn.RequiresTags))
            {
                tracks = tracks.Include(t => t.Tags);
                IncludedTags = true;
            }

            InputResult = await tracks.Where(t => t.Playlists.Contains(Playlist)).ToListAsync();
        }


        public override bool IsValid => !string.IsNullOrEmpty(PlaylistId) || Playlist != null;
    }
}
