using NUnit.Framework;
using SpotifyAPI.Web;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tests.Util;

namespace Tests.Util
{
    public class SpotifyClientMockTests : BaseTests
    {
        private ISpotifyClient Client { get; set; }


        private const int nTracks = 1000;
        private const int nLikedTracks = 500;
        private const int nPlaylists = 1000;
        private const int nLikedPlaylists = 500;

        private List<FullTrack> Tracks;
        private List<FullTrack> LikedTracks;
        private List<SimplePlaylist> Playlists;
        private List<SimplePlaylist> LikedPlaylists;
        private Dictionary<string, List<FullTrack>> PlaylistTracks;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            Tracks = Enumerable.Range(1, nTracks)
                .Select(i => NewTrack(i)).ToList();
            LikedTracks = Tracks.Take(nLikedTracks).ToList();
            Playlists = Enumerable.Range(1, nPlaylists)
                .Select(i => NewPlaylist(i)).ToList();
            LikedPlaylists = Playlists.Take(nLikedPlaylists).ToList();
            PlaylistTracks = Enumerable.Range(1, nPlaylists)
                .ToDictionary(
                i => $"Playlist{i}",
                i => Enumerable.Range(1, i).Select(j => NewTrack(j)).ToList());
            Client = new SpotifyClientMock().SetUp(Tracks, LikedTracks, Playlists, LikedPlaylists, PlaylistTracks);
        }

        [Test]
        public async Task Library_GetTracks()
        {
            var page = await Client.Library.GetTracks(new LibraryTracksRequest { Market = "EU" });
            var allItems = new List<FullTrack>();
            await foreach (var item in Client.Paginate(page))
                allItems.Add(item.Track);
            Assert.AreEqual(nLikedTracks, allItems.Count);
        }
        [Test]
        public async Task Playlists_CurrentUsers()
        {
            var page = await Client.Playlists.CurrentUsers(new PlaylistCurrentUsersRequest { Limit = 50 });
            var allItems = new List<SimplePlaylist>();
            await foreach (var item in Client.Paginate(page))
                allItems.Add(item);
            Assert.AreEqual(nLikedPlaylists, allItems.Count);
        }
        [Test]
        public async Task Playlists_GetItems()
        {
            for (var i = 0; i < nLikedPlaylists; i++)
            {
                var page = await Client.Playlists.GetItems(LikedPlaylists[i].Id, new PlaylistGetItemsRequest { Limit = 100 });
                var allItems = new List<FullTrack>();
                await foreach (var item in Client.Paginate(page))
                    allItems.Add(item.Track as FullTrack);
                Assert.AreEqual(i + 1, allItems.Count);
            }
        }

        [Test]
        public async Task Playlists_Create()
        {
            var newPlaylist = await Client.Playlists.Create("someUser", new PlaylistCreateRequest("somename"));
            Assert.IsNotNull(await Client.Playlists.Get(newPlaylist.Id));
        }

        [Test]
        public async Task Playlists_Get()
        {
            for (var i = 0; i < nPlaylists; i++)
            {
                var details = await Client.Playlists.Get(Playlists[i].Id);
                Assert.AreEqual(i + 1, details.Tracks.Total);
                Assert.AreEqual(Playlists[i].Id, details.Id);
                Assert.AreEqual(Playlists[i].Name, details.Name);
            }
        }
        [Test]
        public async Task Follow_CheckPlaylist_FollowPlaylist()
        {
            var newPlaylist = await Client.Playlists.Create("someUserId", new PlaylistCreateRequest("somename"));
            AssertIsFollowing(true);

            await Client.Follow.UnfollowPlaylist(newPlaylist.Id);
            AssertIsFollowing(false);

            await Client.Follow.FollowPlaylist(newPlaylist.Id);
            AssertIsFollowing(true);

            void AssertIsFollowing(bool expected)
            {
                var req = new FollowCheckPlaylistRequest(new List<string> { "someUserId" });
                var isFollowing = Client.Follow.CheckPlaylist(newPlaylist.Id, req).Result[0];
                Assert.AreEqual(expected, isFollowing);
            }
        }

        [Test]
        public async Task Playlists_ChangeDetails()
        {
            const string playlistName = "someName";
            const string newPlaylistName = "someNewName";
            var newPlaylist = await Client.Playlists.Create("someUserId", new PlaylistCreateRequest(playlistName));
            Assert.AreEqual(playlistName, (await Client.Playlists.Get(newPlaylist.Id)).Name);

            await Client.Playlists.ChangeDetails(newPlaylist.Id, new PlaylistChangeDetailsRequest { Name = newPlaylistName });
            Assert.AreEqual(newPlaylistName, (await Client.Playlists.Get(newPlaylist.Id)).Name);
        }

        [Test]
        public async Task Playlists_AddItems_RemoveItems()
        {
            const int nTracks = 200;
            var newPlaylist = await Client.Playlists.Create("someUserId", new PlaylistCreateRequest("someName"));

            var addReq = new PlaylistAddItemsRequest(
                Enumerable.Range(0, nTracks)
                .Select(i => $"spotify:track:{Tracks[i].Id}")
                .ToList());
            await Client.Playlists.AddItems(newPlaylist.Id, addReq);

            var page = await Client.Playlists.GetItems(newPlaylist.Id, new PlaylistGetItemsRequest { Limit = 100 });
            var tracksAfterAdd = await Client.Paginate(page).ToListAsync();
            Assert.AreEqual(nTracks, tracksAfterAdd.Count);

            var removeReq = new PlaylistRemoveItemsRequest
            {
                Positions = Enumerable.Range(0, nTracks).ToList(),
                SnapshotId = "someSnapshotId",
            };
            await Client.Playlists.RemoveItems(newPlaylist.Id, removeReq);

            var tracksAfterRemove = await Client.Paginate(await Client.Playlists.GetItems(newPlaylist.Id)).ToListAsync();
            Assert.AreEqual(0, tracksAfterRemove.Count);
        }
    }
}
