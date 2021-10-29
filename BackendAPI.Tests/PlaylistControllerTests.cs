using Backend.Entities;
using BackendAPI.Controllers;
using NUnit.Framework;
using Serilog;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Util;

namespace BackendAPI.Tests
{
    public class PlaylistControllerTests : BaseTests
    {
        protected PlaylistController PlaylistController { get; set; }
        protected TrackController TrackController { get; set; }

        private List<Tag> Tags { get; set; }
        private List<FullTrack> Tracks { get; set; }
        private List<SimplePlaylist> Playlists { get; set; }
        private Dictionary<string, List<FullTrack>> PlaylistTracks { get; set; }

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            PlaylistController = new PlaylistController(MockUtil.GetMockedLogger<PlaylistController>(Log.Logger));
            TrackController = new TrackController(MockUtil.GetMockedLogger<TrackController>(Log.Logger));

            // insert tags
            Tags = InsertTags(2);

            // setup spotify
            Tracks = Enumerable.Range(1, 10).Select(i => NewTrack(i)).ToList();
            PlaylistTracks = new Dictionary<string, List<FullTrack>>
            {
                { "Playlist1Id", Tracks.Take(5).ToList() }
            };
            Playlists = Enumerable.Range(1, PlaylistTracks.Count).Select(i => NewPlaylist(i)).ToList();
            InitSpotify(Tracks, playlists: Playlists, playlistTracks: PlaylistTracks);
        }

        [Test]
        public async Task AssignTag()
        {
            foreach (var playlist in Playlists)
            {
                var playlistTracks = PlaylistTracks[playlist.Id];
                var allFalse = playlistTracks.Select(t => false);
                var allTrue = playlistTracks.Select(t => true);

                // invalid id
                Assert.IsNull(await PlaylistController.AssignTag(Tags[0].Name, null)); 

                // invalid tag
                AssertUtil.SequenceEqual(allFalse, await PlaylistController.AssignTag("asdf", playlist.Id));

                // success
                AssertUtil.SequenceEqual(allTrue, await PlaylistController.AssignTag(Tags[0].Name, playlist.Id));
                // duplicate
                AssertUtil.SequenceEqual(allFalse, await PlaylistController.AssignTag(Tags[0].Name, playlist.Id));

                // check if ALL are tagged
                Assert.AreEqual(Constants.ALL, await PlaylistController.IsTagged(Tags[0].Name, playlist.Id));
                // remove tag from one track of the album
                AssertUtil.SequenceEqual(new[] { true }, TrackController.DeleteAssignment(Tags[0].Name, new[] { playlistTracks.First().Id }));
                // check if SOME are tagged
                Assert.AreEqual(Constants.SOME, await PlaylistController.IsTagged(Tags[0].Name, playlist.Id));
            }
        }

        [Test]
        public async Task DeleteAssignment()
        {
            foreach (var playlist in Playlists)
            {
                var playlistTracks = PlaylistTracks[playlist.Id];
                var allFalse = playlistTracks.Select(t => false);
                var allTrue = playlistTracks.Select(t => true);

                // assign tags
                AssertUtil.SequenceEqual(allTrue, await PlaylistController.AssignTag(Tags[0].Name, playlist.Id));

                // invalid id
                Assert.IsNull(await PlaylistController.DeleteAssignment(Tags[0].Name, null));

                // invalid tag
                AssertUtil.SequenceEqual(allFalse, await PlaylistController.DeleteAssignment("asdf", playlist.Id));

                // success
                AssertUtil.SequenceEqual(allTrue, await PlaylistController.DeleteAssignment(Tags[0].Name, playlist.Id));
                // duplicate
                AssertUtil.SequenceEqual(allFalse, await PlaylistController.DeleteAssignment(Tags[0].Name, playlist.Id));

                // check if NONE are tagged
                Assert.AreEqual(Constants.NONE, await PlaylistController.IsTagged(Tags[0].Name, playlist.Id));
                // add tag to one track of the album
                AssertUtil.SequenceEqual(new[] { true }, await TrackController.AssignTag(Tags[0].Name, new[] { playlistTracks.First().Id }));
                // check if SOME are tagged
                Assert.AreEqual(Constants.SOME, await PlaylistController.IsTagged(Tags[0].Name, playlist.Id));
            }
        }

        [Test]
        public async Task IsTagged()
        {
            foreach (var playlist in Playlists)
            {
                var playlistTracks = PlaylistTracks[playlist.Id];

                // invalid playlistId
                Assert.IsNull(await PlaylistController.IsTagged(Tags[0].Name, null));
                // invalid tag
                Assert.IsNull(await PlaylistController.IsTagged("asdf", playlist.Id)); 

                // check if NONE are tagged
                Assert.AreEqual(Constants.NONE, await PlaylistController.IsTagged(Tags[0].Name, playlist.Id));

                // assign tags
                await PlaylistController.AssignTag(Tags[0].Name, playlist.Id);

                // check if ALL are tagged
                Assert.AreEqual(Constants.ALL, await PlaylistController.IsTagged(Tags[0].Name, playlist.Id));

                // remove tag from one track of the album
                AssertUtil.SequenceEqual(new[] { true }, TrackController.DeleteAssignment(Tags[0].Name, new[] { playlistTracks.First().Id }));
                // check if SOME are tagged
                Assert.AreEqual(Constants.SOME, await PlaylistController.IsTagged(Tags[0].Name, playlist.Id));
            }
        }
    }
}
