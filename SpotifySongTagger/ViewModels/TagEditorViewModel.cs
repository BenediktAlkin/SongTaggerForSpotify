using Backend;
using Backend.Entities;
using MaterialDesignThemes.Wpf;
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

        public async Task OnLoaded()
        {
            // register PlayerManager error handling
            BaseViewModel.PlayerManager.OnPlayerError += OnPlayerError;

            BaseViewModel.PlayerManager.OnTrackChanged += UpdatePlayingTrack;
            BaseViewModel.PlayerManager.OnProgressChanged += SetProgressSpotify;

            // start updates for player
            BaseViewModel.PlayerManager.StartUpdateTrackInfoTimer();
            BaseViewModel.PlayerManager.StartUpdatePlaybackInfoTimer();
            var updatePlaybackInfoTask = BaseViewModel.PlayerManager.UpdatePlaybackInfo();

            IsLoadingPlaylists = true;
            await BaseViewModel.DataContainer.LoadSourcePlaylists();
            BaseViewModel.DataContainer.LoadGeneratedPlaylists();
            IsLoadingPlaylists = false;
            await BaseViewModel.DataContainer.LoadTags();

            if (updatePlaybackInfoTask != null)
                await updatePlaybackInfoTask;

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
        public ObservableCollection<TrackViewModel> TrackVMs { get; } = new();
        public async Task LoadTracks(Playlist playlist)
        {
            IsLoadingTracks = true;
            TrackVMs.Clear();
            List<Track> tracks;
            if(DataContainer.GeneratedPlaylists.Contains(playlist))
                tracks = await DatabaseOperations.GeneratedPlaylistTracks(playlist.Id);
            else if(Backend.Constants.META_PLAYLIST_IDS.Contains(playlist.Id))
                tracks = await DatabaseOperations.MetaPlaylistTracks(playlist.Id);
            else
                tracks = await DatabaseOperations.PlaylistTracks(playlist.Id);

            // check if the playlist is still selected
            if (SelectedPlaylist.Id == playlist.Id)
            {
                foreach (var track in tracks)
                    TrackVMs.Add(new TrackViewModel(track));
                IsLoadingTracks = false;
            }
        }
        private void UpdatePlayingTrack(string newId)
        {
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

        public List<PlaylistCategory> PlaylistCategories { get; } = new()
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
        private bool isLoadingPlaylists;
        public bool IsLoadingPlaylists
        {
            get => isLoadingPlaylists;
            set => SetProperty(ref isLoadingPlaylists, value, nameof(IsLoadingPlaylists));
        }

        public static void AssignTag(Track track, string tag) => DatabaseOperations.AssignTag(track, tag);
        public void RemoveAssignment(Tag tag) => DatabaseOperations.RemoveAssignment(SelectedTrackVM.Track, tag);
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
            DatabaseOperations.DeleteTag(ClickedTag);
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
