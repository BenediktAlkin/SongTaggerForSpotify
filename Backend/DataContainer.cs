using Backend.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SpotifyAPI.Web;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
            Tags = null;
            MetaPlaylists.Clear();
            LikedPlaylists.Clear();
            GeneratedPlaylists.Clear();
        }


        // spotify data
        private PrivateUser user;
        public PrivateUser User
        {
            get => user;
            set => SetProperty(ref user, value, nameof(User));
        }

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

        public async Task LoadSourcePlaylists(bool forceReload = false)
        {
            // if liked changes --> force reload
            var newLikedTask = MetaPlaylists == null || forceReload 
                ? Task.Run(() => LikedPlaylists = DatabaseOperations.PlaylistsLiked())
                : Task.CompletedTask;
            // meta does not change --> only require loading once
            var newMetaTask = MetaPlaylists == null
                ? Task.Run(() => MetaPlaylists = DatabaseOperations.PlaylistsMeta())
                : Task.CompletedTask;
            await Task.WhenAll(newLikedTask, newMetaTask);
        }
        public async Task LoadGeneratedPlaylists()
        {
            await Task.Run(() => GeneratedPlaylists = DatabaseOperations.PlaylistsGenerated());
        }
        #endregion


        #region tags
        private bool isLoadingTags;
        public bool IsLoadingTags
        {
            get => isLoadingTags;
            set => SetProperty(ref isLoadingTags, value, nameof(IsLoadingTags));
        }
        private ObservableCollection<Tag> tags;
        public ObservableCollection<Tag> Tags
        {
            get => tags;
            set => SetProperty(ref tags, value, nameof(Tags));
        }
        public async Task LoadTags()
        {
            if (Tags != null) return;

            Log.Information("Loading tags");
            IsLoadingTags = true;
            await Task.Run(() =>
            {
                var dbTags = DatabaseOperations.GetTags();
                Tags = new ObservableCollection<Tag>(dbTags);
            });
            IsLoadingTags = false;
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

            Log.Information("Loading pages");
            await Task.Run(() =>
            {
                // TODO
                //var dbPages = DatabaseOperations.GetGraphGeneratorPages();
                //GraphGeneratorPages = new ObservableCollection<GraphGeneratorPage>(dbPages);
            });
        }
        #endregion
    }
}
