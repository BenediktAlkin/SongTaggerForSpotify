using Backend.Entities;
using Backend.Entities.GraphNodes;
using NUnit.Framework;
using SpotifyAPI.Web;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Tests
{
    public class SpotifyOperationsTests : BaseTests
    {
        [Test]
        public async Task PlaylistItems()
        {
            Assert.AreEqual(0, (await SpotifyOperations.PlaylistItems(null)).Count);
            Assert.AreEqual(0, (await SpotifyOperations.PlaylistItems("")).Count);

            var tracks = Enumerable.Range(1, 10).Select(i => NewTrack(i)).ToList();
            var playlists = Enumerable.Range(1, 1).Select(i => NewPlaylist(i)).ToList();
            var playlistTracks = Enumerable.Range(0, playlists.Count).ToDictionary(i => playlists[i].Id, i => tracks.ToList());
            InitSpotify(tracks, new(), playlists, new(), playlistTracks);

            Assert.AreEqual(10, (await SpotifyOperations.PlaylistItems(playlists[0].Id)).Count);
        }

        [Test]
        public async Task SyncPlaylistOutput()
        {
            // TODO test this with sync library
            await Task.Delay(1);
        }

        [Test]
        public async Task GetFullLibrary_NoItems()
        {
            InitSpotify(new(), new(), new(), new(), new());
            var (playlists, tracks) = await SpotifyOperations.GetFullLibrary(new());
            Assert.AreEqual(0, playlists.Count);
            Assert.AreEqual(0, tracks.Count);
        }
        [Test]
        public async Task GetFullLibrary_OnlyLikedTracks()
        {
            var tracks = Enumerable.Range(1, 100).Select(i => NewTrack(i)).ToList();
            var likedTracks = tracks.Take(50).ToList();
            InitSpotify(tracks, likedTracks, new(), new(), new());

            var (spotifyPlaylists, spotifyTracks) = await SpotifyOperations.GetFullLibrary(new());
            Assert.AreEqual(0, spotifyPlaylists.Count);
            Assert.AreEqual(likedTracks.Count, spotifyTracks.Count);
        }
        [Test]
        public async Task GetFullLibrary_OnlyLikedPlaylists()
        {
            var tracks = Enumerable.Range(1, 100).Select(i => NewTrack(i)).ToList();
            var playlists = Enumerable.Range(1, 3).Select(i => NewPlaylist(i)).ToList();
            var likedPlaylists = playlists.ToList();
            var playlistTrackIdxs = new int[][]
            {
                new []{ 1, 2, 3 },
                new []{ 1, 2, 3, 5 , 8 },
                new []{ 10, 25, 62, 53, 23, 15, 15},
            };
            var uniqueTracks = playlistTrackIdxs.SelectMany(idxs => idxs).Distinct().Count();
            var playlistTracks = Enumerable.Range(0, playlists.Count)
                .ToDictionary(i => playlists[i].Id, i => playlistTrackIdxs[i].Select(j => tracks[j]).ToList());
            InitSpotify(tracks, new(), playlists, likedPlaylists, playlistTracks);

            var (spotifyPlaylists, spotifyTracks) = await SpotifyOperations.GetFullLibrary(new());
            Assert.AreEqual(likedPlaylists.Count, spotifyPlaylists.Count);
            Assert.AreEqual(uniqueTracks, spotifyTracks.Count);
        }
        [Test]
        public async Task GetFullLibrary()
        {
            var tracks = Enumerable.Range(1, 100).Select(i => NewTrack(i)).ToList();
            var likedTrackIdxs = new[] { 5, 9, 12, 31, 23, 54, 67, 11, 8 };
            var likedTracks = likedTrackIdxs.Select(i => tracks[i]).ToList();
            var playlists = Enumerable.Range(1, 3).Select(i => NewPlaylist(i)).ToList();
            var likedPlaylists = playlists.ToList();
            var playlistTrackIdxs = new int[][]
            {
                new []{ 1, 2, 3 },
                new []{ 1, 2, 3, 5 , 8 },
                new []{ 10, 25, 62, 53, 23, 15, 15},
            };
            var uniqueTracks = playlistTrackIdxs.SelectMany(idxs => idxs).Concat(likedTrackIdxs).Distinct().Count();
            var playlistTracks = Enumerable.Range(0, playlists.Count)
                .ToDictionary(i => playlists[i].Id, i => playlistTrackIdxs[i].Select(j => tracks[j]).ToList());
            InitSpotify(tracks, likedTracks, playlists, likedPlaylists, playlistTracks);

            var (spotifyPlaylists, spotifyTracks) = await SpotifyOperations.GetFullLibrary(new());
            Assert.AreEqual(likedPlaylists.Count, spotifyPlaylists.Count);
            Assert.AreEqual(uniqueTracks, spotifyTracks.Count);
        }
    }
}