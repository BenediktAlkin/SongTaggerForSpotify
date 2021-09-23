using Backend.Entities;
using Backend.Entities.GraphNodes;
using Serilog;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend
{
    public static class SpotifyOperations
    {
        private static SpotifyClient Spotify => ConnectionManager.Instance.Spotify;

        private static async Task<List<T>> GetAll<T>(Paging<T> page)
        {
            var all = new List<T>();
            await foreach (var item in Spotify.Paginate(page))
                all.Add(item);
            return all;
        }

        public static async Task<List<Track>> LikedTracks()
        {
            Log.Information("Start fetching liked tracks");
            var page = await Spotify.Library.GetTracks(new LibraryTracksRequest { Limit = 50 });
            var allTracks = await GetAll(page);
            Log.Information("Finished fetching liked tracks");
            return allTracks.Where(t => !t.Track.IsLocal).Select(t => new Track
            {
                Id = t.Track.Id,
                Name = t.Track.Name,
                DurationMs = t.Track.DurationMs,
                AddedAt = t.AddedAt,
                Album = new Album
                {
                    Id = t.Track.Album.Id,
                    Name = t.Track.Album.Name,
                    ReleaseDate = t.Track.Album.ReleaseDate,
                    ReleaseDatePrecision = t.Track.Album.ReleaseDatePrecision,
                },
                Artists = t.Track.Artists.Select(a => new Artist { Id = a.Id, Name = a.Name }).ToList(),
            }).ToList();
        }

        public static async Task<List<Playlist>> PlaylistCurrentUsers()
        {
            Log.Information("Start fetching users playlists");
            var page = await Spotify.Playlists.CurrentUsers(new PlaylistCurrentUsersRequest { Limit = 50 });
            var allPlaylists = await GetAll(page);
            Log.Information("Finished fetching users playlists");
            return allPlaylists.Select(p => new Playlist
            {
                Id = p.Id,
                Name = p.Name,
            }).ToList();
        }

        public static async Task<List<Track>> PlaylistItems(string playlistId)
        {
            var request = new PlaylistGetItemsRequest { Limit = 100, Offset = 0 };
            request.Fields.Add("items(" +
                    "type," +
                    "added_at," +
                    "is_local," +
                    "track(" +
                        "type," +
                        "id," +
                        "name," +
                        "duration_ms," +
                        "album(id,name,release_date,release_date_precision)," +
                        "artists(id,name)))," +
                "next");
            Log.Information($"Start fetching playlist items playlistId={playlistId}");
            var page = await Spotify.Playlists.GetItems(playlistId, request);
            var allTracks = await GetAll(page);
            Log.Information($"Finished fetching playlist items playlistId={playlistId}");
            return allTracks.Where(t => !t.IsLocal).Select(playlistTrack =>
            {
                var track = playlistTrack.Track as FullTrack;
                return new Track
                {
                    Id = track.Id,
                    Name = track.Name,
                    DurationMs = track.DurationMs,
                    AddedAt = playlistTrack.AddedAt.Value,
                    Album = new Album
                    {
                        Id = track.Album.Id,
                        Name = track.Album.Name,
                        ReleaseDate = track.Album.ReleaseDate,
                        ReleaseDatePrecision = track.Album.ReleaseDatePrecision,
                    },
                    Artists = track.Artists.Select(a => new Artist { Id = a.Id, Name = a.Name }).ToList(),
                };
            }).ToList();
        }

        public static async Task SyncPlaylistOutputNode(PlaylistOutputNode playlistOutputNode)
        {
            if (playlistOutputNode.AnyBackward(gn => !gn.IsValid)) return;

            if (playlistOutputNode.GeneratedPlaylistId == null)
            {
                // create playlist
                var request = new PlaylistCreateRequest(playlistOutputNode.PlaylistName);
                var createdPlaylist = await Spotify.Playlists.Create(DataContainer.Instance.User.Id, request);
                playlistOutputNode.GeneratedPlaylistId = createdPlaylist.Id;
            }
            else
            {
                var playlistDetails = await Spotify.Playlists.Get(playlistOutputNode.GeneratedPlaylistId);
                // follow playlist if it was unfollowed
                var followCheckReq = new FollowCheckPlaylistRequest(new List<string> { DataContainer.Instance.User.Id });
                if (!(await Spotify.Follow.CheckPlaylist(playlistOutputNode.GeneratedPlaylistId, followCheckReq)).First())
                    await Spotify.Follow.FollowPlaylist(playlistOutputNode.GeneratedPlaylistId);

                // rename playlist if name changed
                if (playlistDetails.Name != playlistOutputNode.PlaylistName)
                {
                    var changeNameReq = new PlaylistChangeDetailsRequest { Name = playlistOutputNode.PlaylistName };
                    await Spotify.Playlists.ChangeDetails(playlistOutputNode.GeneratedPlaylistId, changeNameReq);
                }


                // remove everything
                if (playlistDetails.Tracks.Total.Value > 0)
                {
                    var request = new PlaylistRemoveItemsRequest
                    {
                        Positions = Enumerable.Range(0, playlistDetails.Tracks.Total.Value).ToList(),
                        SnapshotId = playlistDetails.SnapshotId,
                    };
                    await Spotify.Playlists.RemoveItems(playlistOutputNode.GeneratedPlaylistId, request);
                }
            }

            // add all tracks
            const int BATCH_SIZE = 100;
            await playlistOutputNode.CalculateOutputResult();
            var tracks = playlistOutputNode.OutputResult;
            for (var i = 0; i < tracks.Count; i += BATCH_SIZE)
            {
                var request = new PlaylistAddItemsRequest(
                    Enumerable.Range(0, Math.Min(tracks.Count - i, BATCH_SIZE))
                        .Select(i => $"spotify:track:{tracks[i].Id}")
                        .ToList());
                await Spotify.Playlists.AddItems(playlistOutputNode.GeneratedPlaylistId, request);
            }
        }

        public static async Task<(List<Playlist>, Dictionary<string, Track>)> GetFullLibrary(List<string> generatedPlaylistIds)
        {
            var likedTracksTask = SpotifyOperations.LikedTracks();
            var playlistsTask = SpotifyOperations.PlaylistCurrentUsers();
            var playlistsTracksTask = playlistsTask.ContinueWith(playlists =>
            {
                var playlistTasks = playlists.Result
                    .Where(pl => !generatedPlaylistIds.Contains(pl.Id))
                    .Select(p => SpotifyOperations.PlaylistItems(p.Id)).ToArray();
                Task.WaitAll(playlistTasks);
                return playlistTasks.Select(playlistTask => playlistTask.Result).ToList();
            });

            // add liked tracks to local mirror
            var likedTracks = await likedTracksTask;
            var likedTracksPlaylist = new Playlist { Id = Constants.LIKED_SONGS_PLAYLIST_ID, Name = Constants.LIKED_SONGS_PLAYLIST_ID };
            foreach (var likedTrack in likedTracks)
                likedTrack.Playlists = new List<Playlist> { likedTracksPlaylist };
            var tracks = likedTracks.ToDictionary(t => t.Id, t => t);

            // add tracks from liked playlists
            var playlists = (await playlistsTask).Where(pl => !generatedPlaylistIds.Contains(pl.Id)).ToList();
            var playlistsTracks = await playlistsTracksTask;
            Log.Information("Start fetching full spotify library");
            for (var i = 0; i < playlists.Count; i++)
            {
                Log.Information($"Fetching tracks from playlist \"{playlists[i].Name}\" {i + 1}/{playlists.Count} " +
                    $"({playlistsTracks[i].Count} tracks)");
                var playlist = playlists[i];
                foreach (var track in playlistsTracks[i])
                {
                    if (tracks.TryGetValue(track.Id, out var addedTrack))
                    {
                        // track is multiple times in library --> only add playlist name to track
                        addedTrack.Playlists.Add(playlist);
                    }
                    else
                    {
                        // track is first encountered in this playlist
                        track.Playlists = new List<Playlist> { playlist };
                        tracks[track.Id] = track;
                    }
                }
            }
            Log.Information("Finished fetching full spotify library");
            return (playlists, tracks);
        }
    }
}
