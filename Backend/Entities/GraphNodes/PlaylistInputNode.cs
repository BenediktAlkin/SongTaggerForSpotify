using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes
{
    public abstract class PlaylistInputNode : GraphNode
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
                (to.RequiresTags && !IncludedTags) ||
                (to.RequiresAlbums && !IncludedAlbums))
                ClearResult();
        }
        protected override bool CanAddInput(GraphNode input) => false;
        protected bool IncludedArtists { get; set; }
        protected bool IncludedTags { get; set; }
        protected bool IncludedAlbums { get; set; }

        protected override Task MapInputToOutput()
        {
            OutputResult = InputResult[0];
            return Task.CompletedTask;
        }
        public override async Task CalculateInputResult()
        {
            if (InputResult != null || Playlist == null) return;

            IncludedArtists = AnyForward(gn => gn.RequiresArtists);
            IncludedTags = AnyForward(gn => gn.RequiresTags);
            IncludedAlbums = AnyForward(gn => gn.RequiresAlbums);

            var tracks = await GetTracks();
            InputResult = new List<List<Track>> { tracks };
            Log.Information($"Calculated InputResult for {this} (count={InputResult?.Count} id={PlaylistId} " +
                $"IncludedArtist={IncludedArtists} IncludedTags={IncludedTags} IncludeAlbums={IncludedAlbums})");
        }
        protected abstract Task<List<Track>> GetTracks();


        public override bool IsValid => !string.IsNullOrEmpty(PlaylistId) || Playlist != null;
    }
}
