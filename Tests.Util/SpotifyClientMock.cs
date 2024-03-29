﻿using Moq;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.Util
{
    public class SpotifyClientMock
    {
        private ISpotifyClient SpotifyClient { get; set; }


        private List<FullTrack> Tracks { get; } = new();
        private List<FullTrack> LikedTracks { get; } = new();
        private List<SimplePlaylist> LikedPlaylists { get; } = new();
        private List<SimplePlaylist> Playlists { get; } = new();
        private Dictionary<string, List<FullTrack>> PlaylistTracks { get; } = new();
        private Dictionary<string, (string, List<SimpleTrack>)> Albums { get; } = new();
        private PrivateUser User { get; set; }


        private static string ToPageUri(string actionType, string action, int offset, int limit, string param = null)
        {
            var ret = $"{actionType}.{action}.{offset}.{limit}";
            if (param != null)
                ret = $"{ret}.{param}";
            return ret;
        }

        private static (string, string, int, int, string) FromPageUri(string uri)
        {
            var split = uri.Split('.');
            var param = split.Length == 5 ? split[4] : null;
            return (split[0], split[1], int.Parse(split[2]), int.Parse(split[3]), param);
        }

        private void MockPaginate<T>(Mock<ISpotifyClient> mock)
        {
            mock.Setup(client => client.Paginate<T>(
                It.IsAny<IPaginatable<T>>(),
                It.IsAny<IPaginator>(),
                It.IsAny<CancellationToken>()))
                .Returns((IPaginatable<T> page, IPaginator _1, CancellationToken _2) =>
                MockPaginateReturns<T>(page, _1, _2));
        }
        private IAsyncEnumerable<T> MockPaginateReturns<T>(IPaginatable<T> page, IPaginator _1, CancellationToken _2)
        {
            var pages = new List<object>();
            object curPage = page;
            while (curPage != null)
            {
                pages.Add(curPage);
                var next = (string)curPage.GetType().GetProperty("Next").GetValue(curPage);
                if (next == null)
                    curPage = null;
                else
                {
                    var (actionType, action, offset, limit, param) = FromPageUri(next);
                    switch (actionType)
                    {
                        case nameof(ISpotifyClient.Library):
                            switch (action)
                            {
                                case nameof(ILibraryClient.GetTracks):
                                    var req = new LibraryTracksRequest
                                    {
                                        Offset = offset,
                                        Limit = limit,
                                    };
                                    curPage = SpotifyClient.Library.GetTracks(req).Result;
                                    break;
                                default: throw new ArgumentException($"Invalid Action {action}");
                            }
                            break;
                        case nameof(ISpotifyClient.Playlists):
                            switch (action)
                            {
                                case nameof(IPlaylistsClient.CurrentUsers):
                                    var req1 = new PlaylistCurrentUsersRequest
                                    {
                                        Offset = offset,
                                        Limit = limit,
                                    };
                                    curPage = SpotifyClient.Playlists.CurrentUsers(req1).Result;
                                    break;
                                case nameof(IPlaylistsClient.GetItems):
                                    var req2 = new PlaylistGetItemsRequest
                                    {
                                        Offset = offset,
                                        Limit = limit,
                                    };
                                    curPage = SpotifyClient.Playlists.GetItems(param, req2).Result;
                                    break;
                                default: throw new ArgumentException($"Invalid Action {action}");
                            }
                            break;
                        case nameof(ISpotifyClient.Albums):
                            switch (action)
                            {
                                case nameof(IAlbumsClient.Get):
                                    var req3 = new AlbumTracksRequest
                                    {
                                        Offset = offset,
                                        Limit = limit,
                                    };
                                    curPage = SpotifyClient.Albums.GetTracks(param, req3).Result;
                                    break;
                                default: throw new ArgumentException($"Invalid Action {action}");
                            }
                            break;
                        default: throw new ArgumentException($"Invalid ActionType {actionType}");
                    }
                }
            }

            return pages.Select(p => (IPaginatable<T>)p).SelectMany(p => p.Items).ToAsyncEnumerable();
        }

        public ISpotifyClient SetUp(
            List<FullTrack> tracks,
            List<FullTrack> likedTracks,
            List<SimplePlaylist> playlists,
            List<SimplePlaylist> likedPlaylists,
            Dictionary<string, List<FullTrack>> playlistTracks,
            Dictionary<string, (string, List<SimpleTrack>)> albums,
            PrivateUser user)
        {
            if(tracks != null)
                Tracks.AddRange(tracks);
            if(likedTracks != null)
                LikedTracks.AddRange(likedTracks);
            if(playlists != null)
                Playlists.AddRange(playlists);
            if(likedPlaylists != null)
                LikedPlaylists.AddRange(likedPlaylists);
            if(playlistTracks != null)
                foreach (var item in playlistTracks)
                    PlaylistTracks[item.Key] = item.Value;
            if(albums != null)
                foreach (var item in albums)
                    Albums[item.Key] = item.Value;
            User = user;

            var mock = new Mock<ISpotifyClient>();
            // paginate
            MockPaginate<SavedTrack>(mock);
            MockPaginate<SimplePlaylist>(mock);
            MockPaginate<SimpleTrack>(mock);
            MockPaginate<PlaylistTrack<IPlayableItem>>(mock);


            var libraryMock = new Mock<ILibraryClient>();
            libraryMock.Setup(library => library.GetTracks(It.IsAny<LibraryTracksRequest>()))
                .Returns((LibraryTracksRequest req) =>
                {
                    var page = new Paging<SavedTrack>();
                    var offset = req.Offset == null ? 0 : req.Offset.Value;
                    var limit = req.Limit == null ? 20 : req.Limit.Value;
                    page.Items = LikedTracks
                        .Skip(offset)
                        .Take(limit)
                        .Select(t => new SavedTrack { Track = t })
                        .ToList();
                    if (offset + limit < LikedTracks.Count)
                        page.Next = ToPageUri(
                            nameof(ISpotifyClient.Library),
                            nameof(ILibraryClient.GetTracks),
                            offset + limit,
                            limit);
                    return Task.FromResult(page);
                });


            var playlistsMock = new Mock<IPlaylistsClient>();
            playlistsMock.Setup(playlists => playlists.CurrentUsers(It.IsAny<PlaylistCurrentUsersRequest>()))
                .Returns((PlaylistCurrentUsersRequest req) =>
                {
                    var page = new Paging<SimplePlaylist>();
                    var offset = req.Offset == null ? 0 : req.Offset.Value;
                    var limit = req.Limit == null ? 20 : req.Limit.Value;
                    page.Items = LikedPlaylists
                        .Skip(offset)
                        .Take(limit)
                        .Select(p => new SimplePlaylist { Id = p.Id, Name = p.Name })
                        .ToList();
                    if (offset + limit < LikedPlaylists.Count)
                        page.Next = ToPageUri(
                            nameof(ISpotifyClient.Playlists),
                            nameof(IPlaylistsClient.CurrentUsers),
                            offset + limit,
                            limit);
                    return Task.FromResult(page);
                });
            playlistsMock.Setup(playlists => playlists.GetItems(It.IsAny<string>(), It.IsAny<PlaylistGetItemsRequest>()))
                .Returns((string playlistId, PlaylistGetItemsRequest req) =>
                {
                    var page = new Paging<PlaylistTrack<IPlayableItem>>();
                    var offset = req.Offset == null ? 0 : req.Offset.Value;
                    var limit = req.Limit == null ? 20 : req.Limit.Value;
                    page.Items = PlaylistTracks[playlistId]
                        .Skip(offset)
                        .Take(limit)
                        .Select(t => new PlaylistTrack<IPlayableItem> { IsLocal = t.IsLocal, Track = t })
                        .ToList();
                    if (offset + limit < PlaylistTracks[playlistId].Count)
                        page.Next = ToPageUri(
                            nameof(ISpotifyClient.Playlists),
                            nameof(IPlaylistsClient.GetItems),
                            offset + limit,
                            limit,
                            playlistId);
                    return Task.FromResult(page);
                });
            playlistsMock.Setup(playlists => playlists.Create(It.IsAny<string>(), It.IsAny<PlaylistCreateRequest>()))
                .Returns((string userId, PlaylistCreateRequest req) =>
                {
                    var newId = Guid.NewGuid().ToString("N");
                    var playlist = new SimplePlaylist { Id = newId, Name = req.Name };
                    Playlists.Add(playlist);
                    LikedPlaylists.Add(playlist);
                    PlaylistTracks[playlist.Id] = new List<FullTrack>();
                    return Task.FromResult(new FullPlaylist { Id = newId, Name = req.Name });
                });
            playlistsMock.Setup(playlists => playlists.Get(It.IsAny<string>()))
                .Returns((string playlistId) =>
                {
                    var playlist = Playlists.FirstOrDefault(p => p.Id == playlistId);
                    if (playlist == null) return Task.FromResult<FullPlaylist>(null);

                    var tracks = PlaylistTracks[playlistId];
                    var fullPlaylist = new FullPlaylist
                    {
                        Id = playlist.Id,
                        Name = playlist.Name,
                        Tracks = new Paging<PlaylistTrack<IPlayableItem>>
                        {
                            Total = tracks.Count,
                        },
                    };
                    return Task.FromResult(fullPlaylist);
                });
            playlistsMock.Setup(playlists => playlists.ChangeDetails(It.IsAny<string>(), It.IsAny<PlaylistChangeDetailsRequest>()))
                 .Returns((string playlistId, PlaylistChangeDetailsRequest req) =>
                 {
                     var playlist = Playlists.First(p => p.Id == playlistId);
                     playlist.Name = req.Name;
                     return Task.FromResult(true);
                 });
            playlistsMock.Setup(playlists => playlists.RemoveItems(It.IsAny<string>(), It.IsAny<PlaylistRemoveItemsRequest>()))
                 .Returns((string playlistId, PlaylistRemoveItemsRequest req) =>
                 {
                     var tracks = PlaylistTracks[playlistId];
                     var toRemove = new List<FullTrack>();
                     foreach (var pos in req.Positions)
                         toRemove.Add(tracks[pos]);
                     foreach (var track in toRemove)
                         tracks.Remove(track);
                     return Task.FromResult(new SnapshotResponse { });
                 });
            playlistsMock.Setup(playlists => playlists.AddItems(It.IsAny<string>(), It.IsAny<PlaylistAddItemsRequest>()))
                 .Returns((string playlistId, PlaylistAddItemsRequest req) =>
                 {
                     var tracks = PlaylistTracks[playlistId];
                     foreach (var trackUri in req.Uris)
                     {
                         var trackId = trackUri.Split(':')[2];
                         var track = Tracks.First(t => t.Id == trackId);
                         tracks.Add(track);
                     }
                     return Task.FromResult(new SnapshotResponse { });
                 });


            var followMock = new Mock<IFollowClient>();
            followMock.Setup(follow => follow.CheckPlaylist(It.IsAny<string>(), It.IsAny<FollowCheckPlaylistRequest>()))
                .Returns((string playlistId, FollowCheckPlaylistRequest req) =>
                Task.FromResult(new List<bool> { LikedPlaylists.FirstOrDefault(p => p.Id == playlistId) != null }));
            followMock.Setup(follow => follow.FollowPlaylist(It.IsAny<string>()))
                .Returns((string playlistId) =>
                {
                    var playlist = Playlists.First(p => p.Id == playlistId);
                    LikedPlaylists.Add(playlist);
                    return Task.FromResult(true);
                });
            followMock.Setup(follow => follow.UnfollowPlaylist(It.IsAny<string>()))
                            .Returns((string playlistId) =>
                            {
                                var playlist = Playlists.First(p => p.Id == playlistId);
                                LikedPlaylists.Remove(playlist);
                                return Task.FromResult(true);
                            });


            var albumMock = new Mock<IAlbumsClient>();
            albumMock.Setup(album => album.Get(It.IsAny<string>(), It.IsAny<AlbumRequest>()))
                .Returns((string albumId, AlbumRequest req) =>
                {
                    var album = Albums[albumId];
                    var fullAlbum = new FullAlbum 
                    { 
                        Id = albumId, 
                        Name = album.Item1, 
                        TotalTracks = album.Item2.Count 
                    };
                    
                    var page = new Paging<SimpleTrack>();
                    var offset = 0;
                    var limit = 50;
                    page.Items = album.Item2
                        .Skip(offset)
                        .Take(limit)
                        .Select(t => new SimpleTrack { Id = t.Id, Name = t.Name, Artists = t.Artists.ToList() })
                        .ToList();
                    if (offset + limit < LikedPlaylists.Count)
                        page.Next = ToPageUri(
                            nameof(ISpotifyClient.Albums),
                            nameof(IPlaylistsClient.Get),
                            offset + limit,
                            limit,
                            albumId);
                    page.Total = album.Item2.Count;
                    fullAlbum.Tracks = page;
                    return Task.FromResult(fullAlbum);
                });


            var trackMock = new Mock<ITracksClient>();
            trackMock.Setup(track => track.Get(It.IsAny<string>(), It.IsAny<TrackRequest>()))
                .Returns((string trackId, TrackRequest req) => 
                Task.FromResult(Tracks.FirstOrDefault(t => t.Id == trackId)));
            trackMock.Setup(track => track.GetSeveralAudioFeatures(It.IsAny<TracksAudioFeaturesRequest>()))
                .Returns((TracksAudioFeaturesRequest req) =>
                {
                    var response = new TracksAudioFeaturesResponse
                    {
                        // TODO this is not tested yet and is only here to avoid SyncLibrary failing
                        AudioFeatures = req.Ids.Select(id => new TrackAudioFeatures
                        {
                            Id = id,
                            Acousticness = 0.5f,
                            AnalysisUrl = "http://some.url/",
                            Danceability = 0.5f,
                            DurationMs = 123,
                            Energy = 0.5f,
                            Instrumentalness = 0.5f,
                            Key = 3,
                            Liveness = 0.5f,
                            Loudness = 0.5f,
                            Mode = 0,
                            Speechiness = 0.5f,
                            Tempo = 123,
                            TimeSignature = 4,
                            TrackHref = "http://some.href/",
                            Type = "SomeType",
                            Uri = "http://some.uri",
                            Valence = 0.5f,
                        }).ToList(),
                    };
                    return Task.FromResult(response);
                });

            var userMock = new Mock<IUserProfileClient>();
            userMock.Setup(user => user.Current())
                .Returns(() => Task.FromResult(User));

            // TODO this was not tested yet
            var artistMock = new Mock<IArtistsClient>();
            artistMock.Setup(artist => artist.GetSeveral(It.IsAny<ArtistsRequest>()))
                .Returns((ArtistsRequest req) =>
                {
                    var response = new ArtistsResponse
                    {
                        Artists = req.Ids.Select(id => new FullArtist
                        {
                            Id = id, // TODO this is technically not needed (only genres are needed)
                            Genres = new List<string> { "genre1", "genre2" },
                            Name = $"{id}.Name", // TODO this is technically not needed (only genres are needed)
                        }).ToList(),
                    };
                    return Task.FromResult(response);
                });

            mock.SetupGet(client => client.Library).Returns(libraryMock.Object);
            mock.SetupGet(client => client.Playlists).Returns(playlistsMock.Object);
            mock.SetupGet(client => client.Follow).Returns(followMock.Object);
            mock.SetupGet(client => client.Albums).Returns(albumMock.Object);
            mock.SetupGet(client => client.Tracks).Returns(trackMock.Object);
            mock.SetupGet(client => client.UserProfile).Returns(userMock.Object);
            mock.SetupGet(client => client.Artists).Returns(artistMock.Object);


            SpotifyClient = mock.Object;
            return SpotifyClient;
        }
    }
}
