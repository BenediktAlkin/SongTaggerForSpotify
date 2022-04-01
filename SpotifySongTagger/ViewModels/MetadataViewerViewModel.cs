using Backend;
using Backend.Entities;
using MaterialDesignThemes.Wpf;
using Serilog;
using SpotifySongTagger.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace SpotifySongTagger.ViewModels
{
    public class MetadataViewerViewModel : SpotifyPlayerViewModel
    {
        public MetadataViewerViewModel(ISnackbarMessageQueue messageQueue) 
            : base(messageQueue) { }

        #region load/unload
        public override void OnLoaded()
        {
            base.OnLoaded(); 

            // start updates for player
            BaseViewModel.PlayerManager.StartUpdateTrackInfoTimer();
            BaseViewModel.PlayerManager.StartUpdatePlaybackInfoTimer();
            // update playback info to display it at once (otherwise it would wait for first UpdatePlaybackInfoTimer tick)
            var updatePlaybackInfoTask = BaseViewModel.PlayerManager.UpdatePlaybackInfo();

            // update treeview on playlists refresh (e.g. when sync library updates sourceplaylists)
            BaseViewModel.DataContainer.OnPlaylistsUpdated += NotifyPlaylistTreeNodesChanged;

            // load playlists
            var sourcePlaylistsTask = BaseViewModel.DataContainer.LoadSourcePlaylists();
            var generatedPlaylistsTask = BaseViewModel.DataContainer.LoadGeneratedPlaylists();
            Task.WhenAll(sourcePlaylistsTask, generatedPlaylistsTask).ContinueWith(result => LoadedPlaylists = true);
        }
        public override void OnUnloaded()
        {
            base.OnUnloaded();

            BaseViewModel.PlayerManager.StopUpdateTrackInfoTimer();
            BaseViewModel.PlayerManager.StopUpdatePlaybackInfoTimer();
            BaseViewModel.DataContainer.OnPlaylistsUpdated -= NotifyPlaylistTreeNodesChanged;
        }
        private void NotifyPlaylistTreeNodesChanged() => NotifyPropertyChanged(nameof(PlaylistTreeNodes));
        #endregion

        #region track
        private bool isLoadingTracks;
        public bool IsLoadingTracks
        {
            get => isLoadingTracks;
            set => SetProperty(ref isLoadingTracks, value, nameof(IsLoadingTracks));
        }

        private TrackViewModel selectedTrackVM;
        public TrackViewModel SelectedTrackVM
        {
            get => selectedTrackVM;
            set => SetProperty(ref selectedTrackVM, value, nameof(SelectedTrackVM));
        }
        public async Task LoadTracks(PlaylistOrTag playlistOrTag)
        {
            Log.Information($"loading tracks from {playlistOrTag}");
            IsLoadingTracks = true;
            TrackVMs = null;
            List<Track> tracks;
            if (playlistOrTag.Playlist != null)
            {
                var playlist = playlistOrTag.Playlist;
                if (DataContainer.GeneratedPlaylists.Contains(playlist))
                    tracks = await Task.Run(async () => await DatabaseOperations.GeneratedPlaylistTracks(playlist.Id, 
                        includeTags: false, includeAudioFeatures: true, includeArtistGenres: true));
                else if (Backend.Constants.META_PLAYLIST_IDS.Contains(playlist.Id))
                    tracks = await Task.Run(() => DatabaseOperations.MetaPlaylistTracks(playlist.Id, 
                        includeTags: false, includeAudioFeatures: true, includeArtistGenres: true));
                else
                    tracks = await Task.Run(() => DatabaseOperations.PlaylistTracks(playlist.Id,
                        includeTags: false, includeAudioFeatures: true, includeArtistGenres: true));
            }
            else
                tracks = await Task.Run(() => DatabaseOperations.TagPlaylistTracks(playlistOrTag.Tag.Id, 
                    includeTags: false, includeAudioFeatures: true, includeArtistGenres: true));
            

            var newTrackVMs = tracks.Select(t => new TrackViewModel(t)).ToList();
            // check if the playlist is still selected
            if (SelectedPlaylistOrTag == playlistOrTag)
            {
                // tags of tracks dont have the same reference to tag as they come from a different db context
                foreach (var trackVM in newTrackVMs)
                {
                    var track = trackVM.Track;
                    for (var i = 0; i < track.Tags.Count; i++)
                    {
                        var vmTag = track.Tags[i];
                        var globalTag = DataContainer.Tags.FirstOrDefault(t => t.Id == vmTag.Id);
                        if (globalTag == null)
                        {
                            Log.Error($"tag {vmTag.Name} of track {track.Name} is not in database");
                            continue;
                        }

                        track.Tags[i] = globalTag;
                    }
                }

                TrackVMs = newTrackVMs;
                IsLoadingTracks = false;
                Log.Information($"loaded {TrackVMs.Count} tracks from {playlistOrTag}");
            }
        }
        #endregion


        #region playlist selection
        // for some reason this does not work when it is static
        public List<PlaylistTreeNode> PlaylistTreeNodes => new()
        {
            new PlaylistTreeNode("Meta Playlists", true, DataContainer.MetaPlaylists),
            new PlaylistTreeNode("Liked Playlists", false, DataContainer.LikedPlaylists),
            new PlaylistTreeNode("Generated Playlists", false, DataContainer.GeneratedPlaylists),
            new PlaylistTreeNode("Tag Playlists", false, DataContainer.TagGroups),
        };
        private PlaylistOrTag selectedPlaylistOrTag;
        public PlaylistOrTag SelectedPlaylistOrTag
        {
            get => selectedPlaylistOrTag;
            set => SetProperty(ref selectedPlaylistOrTag, value, nameof(SelectedPlaylistOrTag));
        }
        private bool loadedPlaylists;
        public bool LoadedPlaylists
        {
            get => loadedPlaylists;
            set => SetProperty(ref loadedPlaylists, value, nameof(LoadedPlaylists));
        }
        #endregion
    }
}
