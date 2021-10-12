using Backend;
using Backend.Entities;
using MaterialDesignThemes.Wpf;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace SpotifySongTagger.ViewModels
{
    public class TagEditorViewModel : BaseViewModel
    {
        private ISnackbarMessageQueue MessageQueue{ get; set; }
        public TagEditorViewModel(ISnackbarMessageQueue messageQueue)
        {
            MessageQueue = messageQueue;
        }

        private Task LoadTagsTask { get; set; }
        public void OnLoaded()
        {
            // register PlayerManager error handling
            BaseViewModel.PlayerManager.OnPlayerError += OnPlayerError;

            // register player updates
            BaseViewModel.PlayerManager.OnTrackChanged += UpdatePlayingTrack;
            BaseViewModel.PlayerManager.OnProgressChanged += SetProgressSpotify;

            // start updates for player
            BaseViewModel.PlayerManager.StartUpdateTrackInfoTimer();
            BaseViewModel.PlayerManager.StartUpdatePlaybackInfoTimer();
            // update playback info to display it at once (otherwise it would wait for first UpdatePlaybackInfoTimer tick)
            var updatePlaybackInfoTask = BaseViewModel.PlayerManager.UpdatePlaybackInfo();

            // update treeview on playlists refresh (e.g. when sync library updates sourceplaylists)
            BaseViewModel.DataContainer.OnPlaylistsUpdated += () => NotifyPropertyChanged(nameof(PlaylistCategories));

            // load playlists
            var sourcePlaylistsTask = BaseViewModel.DataContainer.LoadSourcePlaylists();
            var generatedPlaylistsTask = BaseViewModel.DataContainer.LoadGeneratedPlaylists();
            var _ = Task.WhenAll(sourcePlaylistsTask, generatedPlaylistsTask).ContinueWith(result =>
            {
                LoadedPlaylists = true;
            });

            // load tags
            LoadTagsTask = BaseViewModel.DataContainer.LoadTags();
        }

        public void OnUnloaded()
        {
            BaseViewModel.PlayerManager.OnPlayerError -= OnPlayerError;
            BaseViewModel.PlayerManager.OnTrackChanged -= UpdatePlayingTrack;
            BaseViewModel.PlayerManager.OnProgressChanged -= SetProgressSpotify;
            BaseViewModel.PlayerManager.StopUpdateTrackInfoTimer();
            BaseViewModel.PlayerManager.StopUpdatePlaybackInfoTimer();
        }


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
        private List<TrackViewModel> trackVMs;
        public List<TrackViewModel> TrackVMs
        {
            get => trackVMs;
            set => SetProperty(ref trackVMs, value, nameof(TrackVMs));
        }
        public async Task LoadTracks(Playlist playlist)
        {
            IsLoadingTracks = true;
            TrackVMs = null;
            List<Track> tracks;
            if (DataContainer.GeneratedPlaylists.Contains(playlist))
                tracks = await Task.Run(async () => await DatabaseOperations.GeneratedPlaylistTracks(playlist.Id));
            else if (Backend.Constants.META_PLAYLIST_IDS.Contains(playlist.Id))
                tracks = await Task.Run(() => DatabaseOperations.MetaPlaylistTracks(playlist.Id));
            else
                tracks = await Task.Run(() => DatabaseOperations.PlaylistTracks(playlist.Id));

            var newTrackVMs = tracks.Select(t => new TrackViewModel(t)).ToList();
            // check if the playlist is still selected
            if (SelectedPlaylist.Id == playlist.Id)
            {
                await LoadTagsTask;
                // tags of tracks dont have the same reference to tag as they come from a different db context
                foreach(var trackVM in newTrackVMs)
                {
                    var track = trackVM.Track;
                    for (var i = 0; i < track.Tags.Count; i++)
                    {
                        var vmTag = track.Tags[i];
                        var globalTag = DataContainer.Tags.FirstOrDefault(t => t.Id == vmTag.Id);
                        if(globalTag == null)
                        {
                            Log.Error($"tag {vmTag.Name} of track {track.Name} is not in database");
                            continue;
                        }

                        track.Tags[i] = globalTag;
                    }
                }

                TrackVMs = newTrackVMs;
                IsLoadingTracks = false;
                Log.Information($"Selected playlist {playlist.Name} with {TrackVMs.Count} songs");
            }
        }
        private void UpdatePlayingTrack(string newId)
        {
            if (TrackVMs == null) return;
            foreach (var trackVM in TrackVMs)
                trackVM.IsPlaying = trackVM.Track.Id == newId;
        }
        #endregion

        private string newTagName;
        public string NewTagName
        {
            get => newTagName;
            set
            {
                SetProperty(ref newTagName, value, nameof(NewTagName));
                NotifyPropertyChanged(nameof(CanAddTag));
                NotifyPropertyChanged(nameof(CanEditTag));
            }
        }
        public Tag ClickedTag { get; set; }

        // for some reason this does not work when it is static
#pragma warning disable CA1822 // Mark members as static
        public List<PlaylistCategory> PlaylistCategories => new()
#pragma warning restore CA1822 // Mark members as static
        {
            new PlaylistCategory("Meta Playlists", true, DataContainer.MetaPlaylists),
            new PlaylistCategory("Liked Playlists", false, DataContainer.LikedPlaylists),
            new PlaylistCategory("Generated Playlists", false, DataContainer.GeneratedPlaylists),
        };
        private Playlist selectedPlaylist;
        public Playlist SelectedPlaylist 
        {
            get => selectedPlaylist;
            set => SetProperty(ref selectedPlaylist, value, nameof(SelectedPlaylist));
        }
        private bool loadedPlaylists;
        public bool LoadedPlaylists
        {
            get => loadedPlaylists;
            set => SetProperty(ref loadedPlaylists, value, nameof(LoadedPlaylists));
        }

        public static void AssignTag(Track track, string tagName)
        {
            var tag = DataContainer.Tags.FirstOrDefault(t => t.Name == tagName);
            if(tag == null)
            {
                Log.Error($"Could not find tagName {tagName} in db");
                return;
            }
            if(DatabaseOperations.AssignTag(track, tag))
                track.Tags.Add(tag);
        }
        public void RemoveAssignment(Tag tag)
        {
            if(DatabaseOperations.DeleteAssignment(SelectedTrackVM.Track, tag))
                SelectedTrackVM.Track.Tags.Remove(tag);
        }
        public bool CanAddTag => DatabaseOperations.CanAddTag(NewTagName);
        public void AddTag()
        {
            var tag = NewTagName;
            if (string.IsNullOrEmpty(tag)) return;
            DatabaseOperations.AddTag(tag);
        }
        public bool CanEditTag => DatabaseOperations.CanEditTag(ClickedTag, NewTagName);
        public void EditTag()
        {
            if (ClickedTag == null) return;
            DatabaseOperations.EditTag(ClickedTag, NewTagName);
        }
        public void DeleteTag()
        {
            if (ClickedTag == null) return;
            if (DatabaseOperations.DeleteTag(ClickedTag))
            {
                foreach(var trackVM in TrackVMs)
                {
                    if (trackVM.Track.Tags.Contains(ClickedTag))
                        trackVM.Track.Tags.Remove(ClickedTag);
                }
            }
        }

        #region edit/delete icons for tags
        private bool isTagEditMode;
        public bool IsTagEditMode
        {
            get => isTagEditMode;
            set
            {
                SetProperty(ref isTagEditMode, value, nameof(IsTagEditMode));
                NotifyPropertyChanged(nameof(TagEditIcon));
                NotifyPropertyChanged(nameof(TagDeleteOrEditIcon));
                NotifyPropertyChanged(nameof(IsTagEditOrDeleteMode));
            }
        }
        public PackIconKind TagEditIcon => IsTagEditMode ? PackIconKind.Close : PackIconKind.Edit;
        private bool isTagDeleteMode;
        public bool IsTagDeleteMode
        {
            get => isTagDeleteMode;
            set
            {
                SetProperty(ref isTagDeleteMode, value, nameof(IsTagDeleteMode));
                NotifyPropertyChanged(nameof(TagDeleteIcon));
                NotifyPropertyChanged(nameof(TagDeleteOrEditIcon));
                NotifyPropertyChanged(nameof(IsTagEditOrDeleteMode));
            }
        }
        public PackIconKind TagDeleteIcon => IsTagDeleteMode ? PackIconKind.Close : PackIconKind.Delete;
        public PackIconKind TagDeleteOrEditIcon => IsTagEditMode ? PackIconKind.Edit : PackIconKind.Delete;
        public bool IsTagEditOrDeleteMode => IsTagDeleteMode || IsTagEditMode;
        #endregion

        #region Player
        public bool DisableVolumeUpdates { get; set; }
        public bool DisableSpotifyProgressUpdates { get; set; }
        public enum ProgressUpdateSource
        {
            Spotify,
            User,
        }
        private void SetProgressSpotify(int newProgress) => SetProgress(newProgress, ProgressUpdateSource.Spotify);

        public void SetProgress(int newProgress, ProgressUpdateSource source)
        {
            if (DisableSpotifyProgressUpdates && source == ProgressUpdateSource.Spotify)
                return;

            progress = newProgress;
            ProgressSource = source;
            NotifyPropertyChanged(nameof(Progress));
        }
        public ProgressUpdateSource ProgressSource { get; private set; }
        private int progress;
        public int Progress
        {
            get => progress;
            set => SetProgress(value, ProgressUpdateSource.User);
        }

        private void OnPlayerError(PlayerManager.PlayerError error)
        {
            object msg;
            switch (error)
            {
                case PlayerManager.PlayerError.RequiresSpotifyPremium:
                    var textBlock = new TextBlock { Text = "Requires " };
                    var link = new Hyperlink() { NavigateUri = new Uri("https://www.spotify.com/us/premium/") };
                    link.RequestNavigate += (sender, e) => Process.Start(new ProcessStartInfo
                    {
                        FileName = e.Uri.ToString(),
                        UseShellExecute = true
                    });
                    link.Inlines.Add(new Run("Spotify Premium"));
                    textBlock.Inlines.Add(link);
                    msg = textBlock;
                    break;
                default:
                    msg = "Unknown Error from Spotify Player";
                    break;
            }
            MessageQueue.Enqueue(msg);
        }
        #endregion
    }
    public record PlaylistCategory(string Name, bool IsExpanded, IEnumerable<Playlist> Playlists);
}
