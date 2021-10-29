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
    public class TrackControllerTests : BaseTests
    {
        protected TrackController TrackController { get; set; }

        private List<Tag> Tags { get; set; }
        private List<FullTrack> Tracks { get; set; }

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            TrackController = new TrackController(MockUtil.GetMockedLogger<TrackController>(Log.Logger));

            // insert tags
            Tags = InsertTags(2);

            // setup spotify
            Tracks = Enumerable.Range(1, 10).Select(i => NewTrack(i)).ToList();
            InitSpotify(Tracks);
        }

        [Test]
        public async Task AssignTag()
        {
            // invalid id
            Assert.IsNull(await TrackController.AssignTag(Tags[0].Name, null)); 

            var trackIds = Tracks.Take(3).Select(t => t.Id).ToArray();
            var allFalse = trackIds.Select(_ => false);
            var allTrue = trackIds.Select(_ => true);
            // invalid tag
            AssertUtil.SequenceEqual(allFalse, await TrackController.AssignTag("asdf", trackIds));

            // success
            AssertUtil.SequenceEqual(allTrue, await TrackController.AssignTag(Tags[0].Name, trackIds));
            // duplicate
            AssertUtil.SequenceEqual(allFalse, await TrackController.AssignTag(Tags[0].Name, trackIds));
        }

        [Test]
        public async Task DeleteAssignment()
        {
            var trackIds = Tracks.Take(3).Select(t => t.Id).ToArray();
            var allFalse = trackIds.Select(_ => false);
            var allTrue = trackIds.Select(_ => true);
            // assign tags
            await TrackController.AssignTag(Tags[0].Name, trackIds);

            // invalid id
            Assert.IsNull(TrackController.DeleteAssignment(Tags[0].Name, null));
            // invalid tag
            AssertUtil.SequenceEqual(allFalse, TrackController.DeleteAssignment("asdf", trackIds));

            // success
            AssertUtil.SequenceEqual(allTrue, TrackController.DeleteAssignment(Tags[0].Name, trackIds));
            // duplicate
            AssertUtil.SequenceEqual(allFalse, TrackController.DeleteAssignment(Tags[0].Name, trackIds));

            // try to remove assignment that does not exist
            trackIds = Tracks.Skip(trackIds.Length).Take(3).Select(t => t.Id).ToArray();
            allFalse = trackIds.Select(_ => false);
            AssertUtil.SequenceEqual(allFalse, TrackController.DeleteAssignment(Tags[0].Name, trackIds));
        }

        [Test]
        public async Task IsTagged()
        {
            var trackIds = Tracks.Take(3).Select(t => t.Id).ToArray();

            // invalid id
            Assert.IsNull(TrackController.IsTagged(Tags[0].Name, null));
            // invalid tag
            Assert.IsNull(TrackController.IsTagged("asdf", trackIds));

            // success
            Assert.AreEqual(Constants.NONE, TrackController.IsTagged(Tags[0].Name, trackIds));

            // assign tags
            await TrackController.AssignTag(Tags[0].Name, trackIds);

            // success
            Assert.AreEqual(Constants.ALL, TrackController.IsTagged(Tags[0].Name, trackIds));

            // add some ids that are not tagged to get SOME
            trackIds = trackIds.Concat(Tracks.Skip(trackIds.Length).Take(2).Select(t => t.Id)).ToArray();
            // success
            Assert.AreEqual(Constants.SOME, TrackController.IsTagged(Tags[0].Name, trackIds)); 
        }
    }
}
