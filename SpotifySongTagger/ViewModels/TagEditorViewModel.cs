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
    public class TagEditorViewModel : SpotifyPlayerViewModel
    {
        public TagEditorViewModel(ISnackbarMessageQueue messageQueue) 
            : base(messageQueue) { }

        #region load/unload
        private Task LoadTagsTask { get; set; }
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

            // load tags
            LoadTagsTask = BaseViewModel.DataContainer.LoadTagGroups();
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
                    tracks = await Task.Run(async () => await DatabaseOperations.GeneratedPlaylistTracks(playlist.Id));
                else if (Backend.Constants.META_PLAYLIST_IDS.Contains(playlist.Id))
                    tracks = await Task.Run(() => DatabaseOperations.MetaPlaylistTracks(playlist.Id));
                else
                    tracks = await Task.Run(() => DatabaseOperations.PlaylistTracks(playlist.Id));
            }
            else
                tracks = await Task.Run(() => DatabaseOperations.TagPlaylistTracks(playlistOrTag.Tag.Id));
            

            var newTrackVMs = tracks.Select(t => new TrackViewModel(t)).ToList();
            // check if the playlist is still selected
            if (SelectedPlaylistOrTag == playlistOrTag)
            {
                await LoadTagsTask;
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


        #region assign/unassign tag
        public static void AssignTag(Track track, string tagName)
        {
            var tag = DataContainer.Tags.FirstOrDefault(t => t.Name == tagName);
            if (tag == null)
            {
                Log.Error($"Could not find tagName {tagName} in db");
                return;
            }
            if (DatabaseOperations.AssignTag(track, tag))
                track.Tags.Add(tag);
        }
        public void AssignTagToCurrentlyPlayingTrack(string tagName)
        {
            if (PlayerManager.Track == null) return;
            var trackId = PlayerManager.TrackId;
            // search for trackVM if the currently playing track is in the currently selected playlist
            var trackVM = TrackVMs == null ? null : TrackVMs.Where(tvm => tvm.Track.Id == trackId).FirstOrDefault();
            Track track;
            if (trackVM != null)
                track = trackVM.Track;
            else
                // track is not in currently selected playlist --> tag does not need to be added in UI
                track = SpotifyOperations.ToTrack(PlayerManager.Track);
            // add track to db if it is not already in db
            DatabaseOperations.AddTrack(track);
            AssignTag(track, tagName);
        }
        public void RemoveAssignment(Tag tag)
        {
            if (DatabaseOperations.DeleteAssignment(SelectedTrackVM.Track, tag))
                SelectedTrackVM.Track.Tags.Remove(tag);
        }
        #endregion

        #region add/edit/delete tag

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

        public bool CanAddTag => DatabaseOperations.IsValidTag(NewTagName);
        public void AddTag()
        {
            if (string.IsNullOrEmpty(NewTagName)) return;
            var tag = new Tag { Name = NewTagName };
            if (DatabaseOperations.AddTag(tag))
            {
                DataContainer.Instance.AddTag(tag);
            }
        }
        public bool CanEditTag => DatabaseOperations.IsValidTag(NewTagName);
        public void EditTag()
        {
            if (ClickedTag == null) return;

            if (DatabaseOperations.EditTag(ClickedTag, NewTagName))
                DataContainer.Instance.EditTag(ClickedTag, NewTagName);
        }
        public void DeleteTag()
        {
            if (ClickedTag == null) return;
            if (DatabaseOperations.DeleteTag(ClickedTag))
            {
                if(TrackVMs != null)
                {
                    foreach (var trackVM in TrackVMs)
                    {
                        if (trackVM.Track.Tags.Contains(ClickedTag))
                            trackVM.Track.Tags.Remove(ClickedTag);
                    }
                }
                DataContainer.Instance.DeleteTag(ClickedTag);
            }
        }
        #endregion

        #region edit/delete icons for tags/tagGroups
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

        #region TagGroups

        private string newTagGroupName;
        public string NewTagGroupName
        {
            get => newTagGroupName;
            set
            {
                SetProperty(ref newTagGroupName, value, nameof(NewTagGroupName));
                NotifyPropertyChanged(nameof(CanAddTagGroup));
                NotifyPropertyChanged(nameof(CanEditTagGroup));
            }
        }
        public TagGroup ClickedTagGroup { get; set; }
        public bool CanAddTagGroup => DatabaseOperations.IsValidTagGroupName(NewTagGroupName);
        public bool CanEditTagGroup => DatabaseOperations.IsValidTagGroupName(NewTagGroupName);
        public void AddTagGroup()
        {
            var tagGroup = new TagGroup { Name = NewTagGroupName };
            if (DatabaseOperations.AddTagGroup(tagGroup))
                DataContainer.Instance.TagGroups.Add(tagGroup);
        }
        public void EditTagGroup()
        {
            if (ClickedTagGroup == null) return;

            if (DatabaseOperations.EditTagGroup(ClickedTagGroup, NewTagGroupName))
                ClickedTagGroup.Name = NewTagGroupName;
        }
        public void DeleteTagGroup()
        {
            if (ClickedTagGroup == null) return;
            if(ClickedTagGroup.Id == Backend.Constants.DEFAULT_TAGGROUP_ID)
                MessageQueue.Enqueue("Can't delete default Tag Group");

            var tags = ClickedTagGroup.Tags;
            if (DatabaseOperations.DeleteTagGroup(ClickedTagGroup))
            {
                if (TrackVMs != null)
                {
                    foreach(var tag in tags)
                    {
                        foreach (var trackVM in TrackVMs)
                        {
                            if (trackVM.Track.Tags.Contains(tag))
                                trackVM.Track.Tags.Remove(tag);
                        }
                    }
                }
                DataContainer.Instance.DeleteTagGroup(ClickedTagGroup);
            }
        }
        public void ChangeTagGroup(string tagName, TagGroup tagGroup)
        {
            var tag = DataContainer.Tags.FirstOrDefault(t => t.Name == tagName);
            if (tag == null)
            {
                Log.Error($"Could not find tagName {tagName} in db");
                return;
            }
            if (DatabaseOperations.ChangeTagGroup(tag, tagGroup))
                DataContainer.Instance.ChangeTagGroup(tag, tagGroup);
        }
        public void MoveUp(TagGroup tagGroup)
        {
            var idx = DataContainer.Instance.TagGroups.IndexOf(tagGroup);
            if (idx != -1 && idx != 0)
            {
                var prevTagGroup = DataContainer.Instance.TagGroups[idx - 1];
                if (DatabaseOperations.SwapTagGroupOrder(tagGroup, prevTagGroup))
                    DataContainer.Instance.SwapTagGroupOrder(tagGroup, prevTagGroup);
            }
        }
        public void MoveDown(TagGroup tagGroup)
        {
            var idx = DataContainer.Instance.TagGroups.IndexOf(tagGroup);
            if (idx != -1 && idx != DataContainer.Instance.TagGroups.Count - 1)
            {
                var nextTagGroup = DataContainer.Instance.TagGroups[idx + 1];
                if (DatabaseOperations.SwapTagGroupOrder(tagGroup, nextTagGroup))
                    DataContainer.Instance.SwapTagGroupOrder(tagGroup, nextTagGroup);
            }
        }
        #endregion

        

    }
}
