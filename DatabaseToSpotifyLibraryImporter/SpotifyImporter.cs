using Backend;
using Serilog;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DatabaseToSpotifyLibraryImporter
{
    public static class SpotifyImporter
    {
        private static ILogger Logger { get; } = Log.ForContext("SourceContext", "SI");
        private static ISpotifyClient Spotify => ConnectionManager.Instance.Spotify;

        public static async Task Import()
        {
            await ImportLikedSongs();
            await ImportLikedPlaylists();
            Logger.Information("imported db into spotify");
        }

        public static async Task ImportLikedSongs()
        {
            const int limit = 50;
            using var db = ConnectionManager.NewContext();
            var allSongs = db.Tracks.Select(t => t.Id).ToList();
            Logger.Information($"importing {allSongs.Count} songs");
            for (var i = 0; i < allSongs.Count; i += limit)
            {
                var ids = allSongs.Skip(i).Take(limit).ToList();
                if (!await Spotify.Library.SaveTracks(new LibrarySaveTracksRequest(ids)))
                    Logger.Error($"failed importing songs {i} - {i * limit + limit}");
                else
                    Logger.Information($"imported songs {i} - {i * limit + limit}");
            }
            Logger.Information("imported songs");
        }
        public static async Task ImportLikedPlaylists()
        {
            const int limit = 50;
            using var db = ConnectionManager.NewContext();
            var allPlaylists = db.Playlists.Include(p => p.Tracks).ToList();


            Logger.Information($"importing {allPlaylists.Count} playlists");
            for (var i = 0; i < allPlaylists.Count; i++)
            {
                var playlist = allPlaylists[i];
                Logger.Information($"importing playlist {i+1}/{allPlaylists.Count} name={playlist.Name}");
                var spotifyPlaylist = await Spotify.Playlists.Create(DataContainer.Instance.User.Id, new PlaylistCreateRequest(playlist.Name));
                if (spotifyPlaylist != null)
                {
                    for (var j = 0; j < playlist.Tracks.Count; j += limit)
                    {
                        var uris = playlist.Tracks.Skip(j).Take(limit).Select(t => $"spotify:track:{t.Id}").ToList();
                        await Spotify.Playlists.AddItems(spotifyPlaylist!.Id, new PlaylistAddItemsRequest(uris));
                    }
                }
                else
                    Logger.Error($"failed to create playlist {playlist.Name}");
                
                
            }
            Logger.Information("imported playlists");
        }
    }
}
