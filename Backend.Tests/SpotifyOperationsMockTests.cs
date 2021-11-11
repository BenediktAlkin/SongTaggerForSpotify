using Backend.Entities;
using Backend.Entities.GraphNodes;
using NUnit.Framework;
using SpotifyAPI.Web;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tests.Util;

namespace Backend.Tests
{
    public class SpotifyOperationsMockTests : BaseTests
    {
        [Test]
        public async Task GetPlaylistItems()
        {
            Assert.AreEqual(0, (await SpotifyOperations.GetPlaylistTracks(null)).Count);
            Assert.AreEqual(0, (await SpotifyOperations.GetPlaylistTracks("")).Count);

            var tracks = Enumerable.Range(1, 10).Select(i => NewTrack(i)).ToList();
            var playlists = Enumerable.Range(1, 1).Select(i => NewPlaylist(i)).ToList();
            var playlistTracks = Enumerable.Range(0, playlists.Count).ToDictionary(i => playlists[i].Id, i => tracks.ToList());
            InitSpotify(tracks, new(), playlists, new(), playlistTracks);

            Assert.AreEqual(10, (await SpotifyOperations.GetPlaylistTracks(playlists[0].Id)).Count);
        }

        [Test]
        public async Task SyncPlaylistOutput()
        {
            // init spotify mock
            var tracks = Enumerable.Range(1, 100).Select(i => NewTrack(i)).ToList();
            var likedTrackIdxs = new[] { 5, 9, 12, 31, 23, 54, 67, 11, 8 };
            var likedTracks = likedTrackIdxs.Select(i => tracks[i]).ToList();
            InitSpotify(tracks, likedTracks, new(), new(), new());

            // init PlaylistOutputNode
            var ggp = new GraphGeneratorPage();
            DatabaseOperations.AddGraphGeneratorPage(ggp);
            var likedSongs = DatabaseOperations.PlaylistsMeta(Constants.LIKED_SONGS_PLAYLIST_ID);
            var inputNode = new PlaylistInputMetaNode();
            DatabaseOperations.AddGraphNode(inputNode, ggp);
            DatabaseOperations.EditPlaylistInputNode(inputNode, likedSongs);
            inputNode.Playlist = likedSongs;
            var outputNode = new PlaylistOutputNode();
            DatabaseOperations.AddGraphNode(outputNode, ggp);
            const string initialName = "GenPL";
            DatabaseOperations.EditPlaylistOutputNodeName(outputNode, initialName);
            outputNode.PlaylistName = initialName;
            DatabaseOperations.AddGraphNodeConnection(inputNode, outputNode);
            inputNode.AddOutput(outputNode);

            // sync library to get liked songs into db
            await DatabaseOperations.SyncLibrary();

            // create generated playlist and insert songs
            await SpotifyOperations.SyncPlaylistOutputNode(outputNode);

            var details = await SpotifyClient.Playlists.Get(outputNode.GeneratedPlaylistId);
            Assert.AreEqual(initialName, details.Name);
            var generatedPlaylist = await SpotifyOperations.GetPlaylistTracks(outputNode.GeneratedPlaylistId);
            Assert.AreEqual(likedTracks.Count, generatedPlaylist.Count);
            AssertIsFollowing(true);

            // unfollow generated playlist
            await SpotifyClient.Follow.UnfollowPlaylist(outputNode.GeneratedPlaylistId);
            AssertIsFollowing(false);
            await SpotifyOperations.SyncPlaylistOutputNode(outputNode);
            AssertIsFollowing(true);

            // change name
            const string newName = "newname";
            DatabaseOperations.EditPlaylistOutputNodeName(outputNode, newName);
            outputNode.PlaylistName = newName;
            await SpotifyOperations.SyncPlaylistOutputNode(outputNode);
            details = await SpotifyClient.Playlists.Get(outputNode.GeneratedPlaylistId);
            Assert.AreEqual(newName, details.Name);


            void AssertIsFollowing(bool expected)
            {
                var req = new FollowCheckPlaylistRequest(new List<string> { "someUserId" });
                var isFollowing = SpotifyClient.Follow.CheckPlaylist(outputNode.GeneratedPlaylistId, req).Result[0];
                Assert.AreEqual(expected, isFollowing);
            }
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
        [Test]
        public async Task GetUser_DefaultTestUser()
        {
            InitSpotify(null, null, null, null, null, null);

            var user = await SpotifyClient.UserProfile.Current();
            Assert.IsNotNull(user);
            Assert.IsNotNull(user.Id);
            Assert.IsNotNull(user.DisplayName);
            Assert.IsNotNull(user.Country);
            Assert.IsNotNull(user.Product);
        }
        [Test]
        [TestCase("SomeId", "SomeDisplayName", "SomeCountry", "SomeProduct")]
        public async Task GetUser(string id, string displayName, string country, string product)
        {
            InitSpotify(null, null, null, null, null, null, new PrivateUser
            {
                Id = id,
                DisplayName = displayName,
                Country = country,
                Product = product,
            });

            var user = await SpotifyClient.UserProfile.Current();
            Assert.IsNotNull(user);
            Assert.AreEqual(id, user.Id);
            Assert.AreEqual(displayName, user.DisplayName);
            Assert.AreEqual(country, user.Country);
            Assert.AreEqual(product, user.Product);
        }
    }
}