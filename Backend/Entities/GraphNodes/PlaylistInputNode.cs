using Microsoft.EntityFrameworkCore;
using Serilog;
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
                PropagateForward(gn => gn.ClearResult());
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
                PropagateForward(gn => gn.ClearResult());
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
        public override async Task CalculateInputResult(bool includeAll=false)
        {
            if (InputResult != null || Playlist == null) return;

            IncludedArtists = includeAll || AnyForward(gn => gn.RequiresArtists);
            IncludedTags = includeAll || AnyForward(gn => gn.RequiresTags);

            InputResult = await DatabaseOperations.PlaylistTracks(Playlist.Id, includeAlbums: includeAll, includeArtists: IncludedArtists, includeTags: IncludedTags);
            Log.Information($"Calculated InputResult for {this} (count={InputResult?.Count} id={PlaylistId} IncludedArtist={IncludedArtists} IncludedTags={IncludedTags} IncludeAlbums={includeAll})");
        }


        public override bool IsValid => !string.IsNullOrEmpty(PlaylistId) || Playlist != null;
    }
}
