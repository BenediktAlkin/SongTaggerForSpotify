using Backend;
using Serilog;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseToSpotifyLibraryImporter
{
    public static class SpotifyCleaner
    {
        private static ILogger Logger { get; } = Log.ForContext("SourceContext", "SC");
        private static ISpotifyClient Spotify => ConnectionManager.Instance.Spotify;

        private static async Task<List<T>> GetAll<T>(Paging<T> page)
        {
            var all = new List<T>();
            await foreach (var item in Spotify.Paginate(page))
                all.Add(item);
            return all;
        }

        public static async Task ClearLibrary()
        {
            await ClearLikedSongs();
            await ClearLikedPlaylists();
            Logger.Information("cleared library");
        }
        public static async Task ClearLikedSongs()
        {
            const int limit = 50;
            Logger.Information("fetching liked songs");
            var page = await Spotify.Library.GetTracks(new LibraryTracksRequest { Limit = limit, Market = DataContainer.Instance.User.Country });
            var allTracks = await GetAll(page);

            Logger.Information($"deleting {allTracks.Count} liked songs");
            for (var i = 0; i < allTracks.Count; i+=limit)
            {
                var ids = allTracks.Skip(i).Take(limit).Select(t => t.Track.LinkedFrom == null ? t.Track.Id : t.Track.LinkedFrom.Id).ToList();
                if (!await Spotify.Library.RemoveTracks(new LibraryRemoveTracksRequest(ids)))
                    Logger.Information($"failed deleting songs {i} - {i + limit}");
            }
        }
        public static async Task ClearLikedPlaylists()
        {
            const int limit = 50;
            Logger.Information("fetching liked playlists");
            var page = await Spotify.Playlists.CurrentUsers(new PlaylistCurrentUsersRequest { Limit = limit });
            var allPlaylists = await GetAll(page);

            Logger.Information($"deleting {allPlaylists.Count} liked playlists");
            for (var i = 0; i < allPlaylists.Count; i++)
                await Spotify.Follow.UnfollowPlaylist(allPlaylists[i].Id);
        }
    }
}
