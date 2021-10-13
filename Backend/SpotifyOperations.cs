﻿using Backend.Entities;
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
        private static ILogger Logger { get; } = Log.ForContext("SourceContext", "SP");
        private static SpotifyClient Spotify => ConnectionManager.Instance.Spotify;

        private static Func<object, bool> TrackIsValid { get; } = t => t is FullTrack ft && !ft.IsLocal && ft.IsPlayable;
        private static Track ToTrack(FullTrack track)
        {
            return new Track
            {
                Id = track.LinkedFrom == null ? track.Id : track.LinkedFrom.Id,
                Name = track.Name,
                DurationMs = track.DurationMs,
                Album = new Album
                {
                    Id = track.Album.Id,
                    Name = track.Album.Name,
                    ReleaseDate = track.Album.ReleaseDate,
                    ReleaseDatePrecision = track.Album.ReleaseDatePrecision,
                },
                Artists = track.Artists.Select(a => new Artist { Id = a.Id, Name = a.Name }).ToList(),
            };
        }
        
        private static async Task<List<T>> GetAll<T>(Paging<T> page)
        {
            var all = new List<T>();
            await foreach (var item in Spotify.Paginate(page))
                all.Add(item);
            return all;
        }

        private static async Task<List<Track>> LikedTracks()
        {
            Logger.Information("Start fetching liked tracks");
            var page = await Spotify.Library.GetTracks(new LibraryTracksRequest { Limit = 50, Market = DataContainer.Instance.User.Country });
            var allTracks = await GetAll(page);
            Logger.Information("Finished fetching liked tracks");
            var testTrack = allTracks.FirstOrDefault(t => t.Track.Name.Contains("OMG What's"));
            return allTracks.Where(t => TrackIsValid(t.Track)).Select(t => ToTrack(t.Track)).ToList();
        }

        private static async Task<List<Playlist>> PlaylistCurrentUsers()
        {
            Logger.Information("Start fetching users playlists");
            var page = await Spotify.Playlists.CurrentUsers(new PlaylistCurrentUsersRequest { Limit = 50 });
            var allPlaylists = await GetAll(page);
            Logger.Information("Finished fetching users playlists");
            return allPlaylists.Select(p => new Playlist
            {
                Id = p.Id,
                Name = p.Name,
            }).ToList();
        }

        public static async Task<List<Track>> PlaylistItems(string playlistId)
        {
            if (string.IsNullOrEmpty(playlistId)) return new();

            var request = new PlaylistGetItemsRequest { Limit = 100, Offset = 0, Market = DataContainer.Instance.User.Country };
            request.Fields.Add("items(" +
                    "type," +
                    "track(" +
                        "type," +
                        "id," +
                        "name," +
                        "duration_ms," +
                        "is_local," +
                        "is_playable," +
                        "linked_from," +
                        "album(id,name,release_date,release_date_precision)," +
                        "artists(id,name)))," +
                "next");
            Logger.Information($"Start fetching playlist items playlistId={playlistId}");
            try
            {
                var page = await Spotify.Playlists.GetItems(playlistId, request);
                var allTracks = await GetAll(page);
                Logger.Information($"Finished fetching playlist items playlistId={playlistId}");
                return allTracks.Where(t => TrackIsValid(t.Track)).Select(playlistTrack => ToTrack(playlistTrack.Track as FullTrack)).ToList();
            }
            catch (Exception e)
            {
                Logger.Error($"Error in PlaylistItems: {e.Message}");
                return new();
            }
        }

        public static async Task SyncPlaylistOutputNode(PlaylistOutputNode playlistOutputNode)
        {
            if (playlistOutputNode.AnyBackward(gn => !gn.IsValid))
            {
                Logger.Information($"cannot run PlaylistOutputNode Id={playlistOutputNode.Id} (encountered invalid graphNode)");
                return;
            }
            Logger.Information($"synchronizing PlaylistOutputNode {playlistOutputNode.PlaylistName} to spotify");

            if (playlistOutputNode.GeneratedPlaylistId == null)
            {
                // create playlist
                var request = new PlaylistCreateRequest(playlistOutputNode.PlaylistName);
                var createdPlaylist = await Spotify.Playlists.Create(DataContainer.Instance.User.Id, request);

                if(DatabaseOperations.EditPlaylistOutputNodeGeneratedPlaylistId(playlistOutputNode, createdPlaylist.Id))
                    playlistOutputNode.GeneratedPlaylistId = createdPlaylist.Id;
            }
            else
            {
                var playlistDetails = await Spotify.Playlists.Get(playlistOutputNode.GeneratedPlaylistId);
                // like playlist if it was unliked
                var followCheckReq = new FollowCheckPlaylistRequest(new List<string> { DataContainer.Instance.User.Id });
                if (!(await Spotify.Follow.CheckPlaylist(playlistOutputNode.GeneratedPlaylistId, followCheckReq)).First())
                    await Spotify.Follow.FollowPlaylist(playlistOutputNode.GeneratedPlaylistId);

                // rename spotify playlist if name changed
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

            // sync with spotify
            const int BATCH_SIZE = 100;
            playlistOutputNode.CalculateOutputResult();
            var tracks = playlistOutputNode.OutputResult;
            for (var i = 0; i < tracks.Count; i += BATCH_SIZE)
            {
                var request = new PlaylistAddItemsRequest(
                    Enumerable.Range(0, Math.Min(tracks.Count - i, BATCH_SIZE))
                        .Select(j => $"spotify:track:{tracks[i+j].Id}")
                        .ToList());
                await Spotify.Playlists.AddItems(playlistOutputNode.GeneratedPlaylistId, request);
            }
            Logger.Information($"synchronized PlaylistOutputNode {playlistOutputNode.PlaylistName} to spotify");
        }

        public static async Task<(List<Playlist>, Dictionary<string, Track>)> GetFullLibrary(List<string> generatedPlaylistIds)
        {
            try
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
                Logger.Information("Start fetching full spotify library");
                for (var i = 0; i < playlists.Count; i++)
                {
                    Logger.Information($"Fetching tracks from playlist \"{playlists[i].Name}\" {i + 1}/{playlists.Count} " +
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
                Logger.Information("Finished fetching full spotify library");
                return (playlists, tracks);
            }
            catch (Exception e)
            {
                Logger.Error($"Error in GetFullLibrary: {e.Message}");
                return (null, null);
            }
        }

        public static async Task<List<Track>> GetTracks(List<string> trackIds)
        {
            try
            {
                var allTracks = await Spotify.Tracks.GetSeveral(new TracksRequest(trackIds.ToList()) { Market = DataContainer.Instance.User.Country });
                return allTracks.Tracks.Where(t => TrackIsValid(t)).Select(track => ToTrack(track)).ToList();
            }
            catch (Exception e)
            {
                Logger.Error($"Error in GetTracks {e.Message} {e.InnerException?.Message}");
                return new();
            }
        }
    }
}
