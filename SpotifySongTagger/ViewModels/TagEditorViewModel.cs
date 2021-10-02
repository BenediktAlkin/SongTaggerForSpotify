using Backend;
using Backend.Entities;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;

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
        public ObservableCollection<TrackViewModel> TrackVMs { get; } = new();
        public async Task LoadTracks(Playlist playlist)
        {
            IsLoadingTracks = true;
            TrackVMs.Clear();
            List<Track> tracks;
            if(DataContainer.GeneratedPlaylists.Contains(playlist))
                tracks = await DatabaseOperations.GeneratedPlaylistTracks(playlist.Id);
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
        public void UpdatePlayingTrack(string newId)
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

        public void UpdatePlaylists()
        {
            PlaylistCategories = new()
            {
                new PlaylistCategory("Meta Playlists", true, DataContainer.MetaPlaylists),
                new PlaylistCategory("Liked Playlists", false, DataContainer.LikedPlaylists),
                new PlaylistCategory("Generated Playlists", false, DataContainer.GeneratedPlaylists),
            };
        }
        private List<PlaylistCategory> playlistCategories;
        public List<PlaylistCategory> PlaylistCategories 
        {
            get => playlistCategories;
            private set => SetProperty(ref playlistCategories, value, nameof(PlaylistCategories));
        }
        private Playlist selectedPlaylist;
        public Playlist SelectedPlaylist 
        {
            get => selectedPlaylist;
            set => SetProperty(ref selectedPlaylist, value, nameof(SelectedPlaylist));
        }
        private bool isLoadingPlaylists = true;
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
        #endregion
    }
    public record PlaylistCategory(string Name, bool IsExpanded, List<Playlist> Playlists);
}
