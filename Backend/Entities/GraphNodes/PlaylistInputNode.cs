using System.Collections.Generic;

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
                PropagateForward(gn => gn.ClearResult());
            }
        }



        protected override void OnConnectionAdded(GraphNode from, GraphNode to)
        {
            if ((to.RequiresArtists && !IncludedArtists) ||
                (to.RequiresTags && !IncludedTags) ||
                (to.RequiresAlbums && !IncludedAlbums) ||
                (to.RequiresAudioFeatures && !RequiresAudioFeatures))
                ClearResult();
        }
        protected override bool CanAddInput(GraphNode input) => false;
        protected bool IncludedArtists { get; set; }
        protected bool IncludedTags { get; set; }
        protected bool IncludedAlbums { get; set; }
        protected bool IncludedAudioFeatures { get; set; }

        protected override void MapInputToOutput() => OutputResult = InputResult[0];
        protected override void CalculateInputResultImpl()
        {
            if (!IsValid)
            {
                Logger.Information($"{this} is invalid --> InputResult = empty list");
                InputResult = new() { new() };
                return;
            }
            IncludedArtists = AnyForward(gn => gn.RequiresArtists);
            IncludedTags = AnyForward(gn => gn.RequiresTags);
            IncludedAlbums = AnyForward(gn => gn.RequiresAlbums);
            IncludedAudioFeatures = AnyForward(gn => gn.RequiresAudioFeatures);

            var tracks = GetTracks();
            InputResult = new List<List<Track>> { tracks };
            Logger.Information($"Calculated InputResult for {this} (count={InputResult?.Count} id={PlaylistId} " +
                $"IncludedArtist={IncludedArtists} IncludedTags={IncludedTags} IncludeAlbums={IncludedAlbums})");
        }
        protected abstract List<Track> GetTracks();


        public override bool IsValid => !string.IsNullOrEmpty(PlaylistId) || Playlist != null;
    }
}
