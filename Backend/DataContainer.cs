using Backend.Entities;
using Backend.Entities.GraphNodes;
using Microsoft.EntityFrameworkCore;
using Serilog;
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
        public static DataContainer Instance { get; } = new DataContainer();
        private DataContainer() { }

        public void Clear()
        {
            User = null;
            SourcePlaylists = null;
            Tags = null;
        }


        // spotify data
        private PrivateUser user;
        public PrivateUser User
        {
            get => user;
            set => SetProperty(ref user, value, nameof(User));
        }


        // source playlists
        private bool isLoadingSourcePlaylists;
        public bool IsLoadingSourcePlaylists
        {
            get => isLoadingSourcePlaylists;
            set => SetProperty(ref isLoadingSourcePlaylists, value, nameof(IsLoadingSourcePlaylists));
        }
        private List<Playlist> specialPlaylists;
        public List<Playlist> SpecialPlaylists
        {
            get => specialPlaylists;
            set => SetProperty(ref specialPlaylists, value, nameof(SpecialPlaylists));
        }
        private List<Playlist> followedPlaylists;
        public List<Playlist> FollowedPlaylists
        {
            get => followedPlaylists;
            set => SetProperty(ref followedPlaylists, value, nameof(FollowedPlaylists));
        }
        private List<Playlist> generatedPlaylists;
        public List<Playlist> GeneratedPlaylists
        {
            get => generatedPlaylists;
            set => SetProperty(ref generatedPlaylists, value, nameof(GeneratedPlaylists));
        }
        // special playlists + followed playlists
        private List<Playlist> sourcePlaylists;
        public List<Playlist> SourcePlaylists
        {
            get => sourcePlaylists;
            set => SetProperty(ref sourcePlaylists, value, nameof(SourcePlaylists));
        }
        public async Task LoadSourcePlaylists(bool forceReload = false)
        {
            if (!forceReload && sourcePlaylists != null) return;

            Log.Information("Loading source playlists");
            IsLoadingSourcePlaylists = true;
            FollowedPlaylists = await DatabaseOperations.PlaylistsLiked();
            SpecialPlaylists = DatabaseOperations.PlaylistsSpecial();
            SourcePlaylists = SpecialPlaylists.Concat(FollowedPlaylists).ToList();
            IsLoadingSourcePlaylists = false;
        }
        public void LoadGeneratedPlaylists()
        {
            Log.Information("Loading generated playlists");
            GeneratedPlaylists = DatabaseOperations.PlaylistsGenerated();
        }


        // all tags in the database
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
    }
}
