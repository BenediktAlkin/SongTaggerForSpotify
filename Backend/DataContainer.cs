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
        private DataContainer()
        {
            MetaPlaylists.CollectionChanged += UpdateSourcePlaylists;
            LikedPlaylists.CollectionChanged += UpdateSourcePlaylists;
        }

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
        public ObservableCollection<Playlist> MetaPlaylists { get; } = new();
        public ObservableCollection<Playlist> LikedPlaylists { get; } = new();
        public ObservableCollection<Playlist> GeneratedPlaylists { get; } = new();
        // meta playlists + liked playlists
        public ObservableCollection<Playlist> SourcePlaylists { get; } = new();
        public async Task LoadSourcePlaylists()
        {
            Log.Information("Loading source playlists");
            LikedPlaylists.Clear();
            MetaPlaylists.Clear();
            ObservableCollectionAddRange(LikedPlaylists, await DatabaseOperations.PlaylistsLiked());
            ObservableCollectionAddRange(MetaPlaylists, DatabaseOperations.PlaylistsMeta());
        }
        public void LoadGeneratedPlaylists()
        {
            Log.Information("Loading generated playlists");
            GeneratedPlaylists.Clear();
            ObservableCollectionAddRange(GeneratedPlaylists, DatabaseOperations.PlaylistsGenerated());
        }
        private void UpdateSourcePlaylists(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var toAdd = e.NewItems[0] as Playlist;
                    if (sender == LikedPlaylists)
                        SourcePlaylists.Add(toAdd);
                    else
                        SourcePlaylists.Insert(MetaPlaylists.Count, toAdd);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    SourcePlaylists.Clear();
                    break;

                default:
                    Log.Warning($"Error syncing meta/generated playlists with source playlists {e.Action}");
                    break;
            }
        }
        private static void ObservableCollectionAddRange<T>(ObservableCollection<T> list, IEnumerable<T> toAdd)
        {
            foreach (var item in toAdd)
                list.Add(item);
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
            var dbTags = await ConnectionManager.Instance.Database.Tags.ToListAsync();
            Tags = new ObservableCollection<Tag>(dbTags);
            IsLoadingTags = false;
        }
        #endregion
    }
}
