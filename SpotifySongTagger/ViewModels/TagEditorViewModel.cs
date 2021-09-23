using Backend;
using Backend.Entities;
using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifySongTagger.ViewModels
{
    public class TagEditorViewModel : BaseViewModel
    {
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
        public ObservableCollection<TrackViewModel> TrackVMs { get; } = new ObservableCollection<TrackViewModel>();
        public async Task LoadTracks(string playlistId)
        {
            IsLoadingTracks = true;
            TrackVMs.Clear();
            var tracks = await ConnectionManager.Instance.Database.Tracks
                .Include(t => t.Album)
                .Include(t => t.Artists)
                .Include(t => t.Tags)
                .Include(t => t.Playlists)
                .Where(t => t.Playlists.Select(p => p.Id).Contains(playlistId))
                .ToListAsync();

            // check if the playlist is still selected
            if (SelectedPlaylist.Id == playlistId)
            {
                foreach (var track in tracks)
                    TrackVMs.Add(new TrackViewModel(track));
                IsLoadingTracks = false;
            }
        }
        public void UpdatePlayingTrack(string newId)
        {
            foreach (var trackVM in TrackVMs)
                trackVM.IsPlaying = trackVM.Track.Id == newId;
        }
        #endregion

        private Playlist selectedPlaylist;
        public Playlist SelectedPlaylist
        {
            get => selectedPlaylist;
            set => SetProperty(ref selectedPlaylist, value, nameof(SelectedPlaylist));
        }
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

        public bool DisableVolumeUpdates { get; set; }
        public bool DisableSpotifyProgressUpdates { get; set; }
        public enum ProgressUpdateSource
        {
            Spotify,
            User,
        }
        public void SetProgressSpotify(int newProgress) => SetProgress(newProgress, ProgressUpdateSource.Spotify);
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

    }
}
