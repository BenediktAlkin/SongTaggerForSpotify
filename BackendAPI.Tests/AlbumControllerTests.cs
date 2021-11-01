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
    public class AlbumControllerTests : BackendAPIBaseTests
    {
        protected AlbumController AlbumController { get; set; }
        protected TrackController TrackController { get; set; }

        private List<Tag> Tags { get; set; }
        private List<FullTrack> Tracks { get; set; }
        private Dictionary<string, (string, List<SimpleTrack>)> Albums { get; set; }

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            AlbumController = new AlbumController(MockUtil.GetMockedLogger<AlbumController>(Log.Logger));
            TrackController = new TrackController(MockUtil.GetMockedLogger<TrackController>(Log.Logger));

            // insert tags
            Tags = InsertTags(2);

            // setup spotify
            Tracks = Enumerable.Range(1, 10).Select(i => NewTrack(i)).ToList();
            Albums = new Dictionary<string, (string, List<SimpleTrack>)>
            {
                { "Album1Id", ("Album1Name", Tracks.Take(5).Select(t => ToSimpleTrack(t)).ToList()) }
            };
            InitSpotify(Tracks, albums: Albums);
        }

        [Test]
        public async Task AssignTag()
        {
            foreach (var album in Albums)
            {
                Assert.IsNull(await AlbumController.AssignTag(Tags[0].Name, null)); // invalid id

                var allFalse = album.Value.Item2.Select(t => false);
                var allTrue = album.Value.Item2.Select(t => true);
                // invalid tag
                AssertUtil.SequenceEqual(allFalse, await AlbumController.AssignTag("asdf", album.Key));

                // success
                AssertUtil.SequenceEqual(allTrue, await AlbumController.AssignTag(Tags[0].Name, album.Key));
                // duplicate
                AssertUtil.SequenceEqual(allFalse, await AlbumController.AssignTag(Tags[0].Name, album.Key)); 

                // check if ALL are tagged
                Assert.AreEqual(Constants.ALL, await AlbumController.IsTagged(Tags[0].Name, album.Key));
                // remove tag from one track of the album
                AssertUtil.SequenceEqual(new[] { true }, TrackController.DeleteAssignment(Tags[0].Name, new[] { album.Value.Item2.First().Id }));
                // check if SOME are tagged
                Assert.AreEqual(Constants.SOME, await AlbumController.IsTagged(Tags[0].Name, album.Key));
            }
        }

        [Test]
        public async Task DeleteAssignment()
        {
            foreach (var album in Albums)
            {
                var allFalse = album.Value.Item2.Select(t => false);
                var allTrue = album.Value.Item2.Select(t => true);
                // assign tags
                AssertUtil.SequenceEqual(allTrue, await AlbumController.AssignTag(Tags[0].Name, album.Key));

                // invalid id
                Assert.IsNull(await AlbumController.DeleteAssignment(Tags[0].Name, null));

                // invalid tag
                AssertUtil.SequenceEqual(allFalse, await AlbumController.DeleteAssignment("asdf", album.Key));
                
                // success
                AssertUtil.SequenceEqual(allTrue, await AlbumController.DeleteAssignment(Tags[0].Name, album.Key));
                // duplicate
                AssertUtil.SequenceEqual(allFalse, await AlbumController.DeleteAssignment(Tags[0].Name, album.Key));

                // check if NONE are tagged
                Assert.AreEqual(Constants.NONE, await AlbumController.IsTagged(Tags[0].Name, album.Key));
                // add tag to one track of the album
                AssertUtil.SequenceEqual(new[] { true }, await TrackController.AssignTag(Tags[0].Name, new[] { album.Value.Item2.First().Id }));
                // check if SOME are tagged
                Assert.AreEqual(Constants.SOME, await AlbumController.IsTagged(Tags[0].Name, album.Key));
            }
        }

        [Test]
        public async Task IsTagged()
        {
            foreach (var album in Albums)
            {
                // invalid albumId
                Assert.IsNull(await AlbumController.IsTagged(Tags[0].Name, null));
                // invalid tag
                Assert.IsNull(await AlbumController.IsTagged("asdf", album.Key));

                // check if NONE are tagged
                Assert.AreEqual(Constants.NONE, await AlbumController.IsTagged(Tags[0].Name, album.Key));

                // assign tags
                await AlbumController.AssignTag(Tags[0].Name, album.Key);

                // check if ALL are tagged
                Assert.AreEqual(Constants.ALL, await AlbumController.IsTagged(Tags[0].Name, album.Key)); 

                // remove tag from one track of the album
                AssertUtil.SequenceEqual(new[] { true }, TrackController.DeleteAssignment(Tags[0].Name, new[] { album.Value.Item2.First().Id }));
                // check if SOME are tagged
                Assert.AreEqual(Constants.SOME, await AlbumController.IsTagged(Tags[0].Name, album.Key));
            }
        }
    }
}
