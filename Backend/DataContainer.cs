﻿using Backend.Entities;
using SpotifyAPI.Web;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Util;

namespace Backend
{
    public class DataContainer : NotifyPropertyChangedBase
    {
        public static DataContainer Instance { get; } = new();
        private DataContainer() { }

        public void Clear()
        {
            User = null;
            ClearData();
        }
        public void ClearData()
        {
            TagGroups = null;
            MetaPlaylists = null;
            LikedPlaylists = null;
            GeneratedPlaylists = null;
            GraphGeneratorPages = null;
        }


        // spotify data
        private PrivateUser user;
        public PrivateUser User
        {
            get => user;
            set
            {
                SetProperty(ref user, value, nameof(User));
                NotifyPropertyChanged(nameof(DbFileName));
            }
        }
        public string DbFileName => User == null ? string.Empty : $"{User.Id}.sqlite";

        #region playlists
        public delegate void PlaylistsUpdatedEvenHandler();
        public event PlaylistsUpdatedEvenHandler OnPlaylistsUpdated;
        private List<Playlist> metaPlaylists;
        public List<Playlist> MetaPlaylists
        {
            get => metaPlaylists;
            set
            {
                SetProperty(ref metaPlaylists, value, nameof(MetaPlaylists));
                OnPlaylistsUpdated?.Invoke();
            }
        }
        private List<Playlist> likedPlaylists;
        public List<Playlist> LikedPlaylists
        {
            get => likedPlaylists;
            set
            {
                SetProperty(ref likedPlaylists, value, nameof(LikedPlaylists));
                OnPlaylistsUpdated?.Invoke();
            }
        }
        private List<Playlist> generatedPlaylists;
        public List<Playlist> GeneratedPlaylists
        {
            get => generatedPlaylists;
            set
            {
                SetProperty(ref generatedPlaylists, value, nameof(GeneratedPlaylists));
                OnPlaylistsUpdated?.Invoke();
            }
        }

        public Task LoadSourcePlaylists(bool forceReload = false)
        {
            // if liked changes --> force reload
            var newLikedTask = MetaPlaylists == null || forceReload
                ? Task.Run(() => LikedPlaylists = DatabaseOperations.PlaylistsLiked())
                : Task.CompletedTask;
            // meta does not change --> only require loading once
            var newMetaTask = MetaPlaylists == null
                ? Task.Run(() => MetaPlaylists = DatabaseOperations.PlaylistsMeta())
                : Task.CompletedTask;
            return Task.WhenAll(newLikedTask, newMetaTask);
        }
        public Task LoadGeneratedPlaylists()
        {
            return Task.Run(() => GeneratedPlaylists = DatabaseOperations.PlaylistsGenerated());
        }
        #endregion

        #region TagGroups
        public List<Tag> Tags => TagGroups == null ? null : TagGroups.SelectMany(tg => tg.Tags).ToList();
        private ObservableCollection<TagGroup> tagGroups;
        public ObservableCollection<TagGroup> TagGroups
        {
            get => tagGroups;
            set
            {
                SetProperty(ref tagGroups, value, nameof(TagGroups));
                NotifyPropertyChanged(nameof(Tags));
                // TagGroups are used to populate a TreeView for TagPlaylists
                OnPlaylistsUpdated?.Invoke();
            }
        }

        private Task LoadTagGroupsTask { get; set; }
        public async Task LoadTagGroups()
        {
            if (TagGroups != null) return;
            if (LoadTagGroupsTask != null)
            {
                await LoadTagGroupsTask;
                return;
            }

            TagGroups = null;
            LoadTagGroupsTask = Task.Run(() =>
            {
                var dbTagGroups = DatabaseOperations.GetTagGroups();
                TagGroups = new ObservableCollection<TagGroup>(dbTagGroups);
            });
            await LoadTagGroupsTask;
            LoadTagGroupsTask = null;
        }
        private void ChangeTagGroupSorted(Tag tag, TagGroup tagGroup)
        {
            // insert sorted
            bool wasInserted = false;
            for (var i = 0; i < tagGroup.Tags.Count; i++)
            {
                if (string.Compare(tagGroup.Tags[i].Name, tag.Name) > 0)
                {
                    tagGroup.Tags.Insert(i, tag);
                    wasInserted = true;
                    break;
                }
            }
            // insert at last position
            if (!wasInserted)
                tagGroup.Tags.Add(tag);
        }
        public void AddTag(Tag tag)
        {
            var defaultTagGroup = TagGroups.FirstOrDefault(tg => tg.Id == Constants.DEFAULT_TAGGROUP_ID);
            if (defaultTagGroup != null)
            {
                ChangeTagGroupSorted(tag, defaultTagGroup);
                tag.TagGroup = defaultTagGroup;
                NotifyPropertyChanged(nameof(Tags));
            }
        }
        public void ChangeTagGroup(Tag tag, TagGroup tagGroup)
        {
            tag.TagGroup.Tags.Remove(tag);
            ChangeTagGroupSorted(tag, tagGroup);
            tag.TagGroup = tagGroup;
        }
        public void DeleteTag(Tag tag)
        {
            tag.TagGroup.Tags.Remove(tag);
            NotifyPropertyChanged(nameof(Tags));
        }
        public void EditTag(Tag tag, string newName)
        {
            tag.Name = newName;
            tag.TagGroup.Tags.Remove(tag);
            ChangeTagGroupSorted(tag, tag.TagGroup);
        }

        public void DeleteTagGroup(TagGroup tagGroup)
        {
            TagGroups.Remove(tagGroup);
            NotifyPropertyChanged(nameof(Tags));
        }

        public void SwapTagGroupOrder(TagGroup a, TagGroup b)
        {
            var temp = a.Order;
            a.Order = b.Order;
            b.Order = temp;
            var aIdx = TagGroups.IndexOf(a);
            var bIdx = TagGroups.IndexOf(b);
            if(aIdx != -1 && bIdx != -1)
            {
                TagGroups[aIdx] = b;
                TagGroups[bIdx] = a;
                NotifyPropertyChanged(nameof(Tags));
            }
        }
        #endregion

        #region GraphGeneratorPages
        private ObservableCollection<GraphGeneratorPage> graphGeneratorPages;
        public ObservableCollection<GraphGeneratorPage> GraphGeneratorPages
        {
            get => graphGeneratorPages;
            set => SetProperty(ref graphGeneratorPages, value, nameof(GraphGeneratorPages));
        }
        public async Task LoadGraphGeneratorPages()
        {
            if (GraphGeneratorPages != null) return;

            await Task.Run(() =>
            {
                var dbPages = DatabaseOperations.GetGraphGeneratorPages();
                GraphGeneratorPages = new ObservableCollection<GraphGeneratorPage>(dbPages);
            });
        }
        #endregion
    }
}
