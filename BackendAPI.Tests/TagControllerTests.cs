using Backend;
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
    public class TagControllerTests : BackendAPIBaseTests
    {
        protected TagController TagController { get; set; }

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            TagController = new TagController(MockUtil.GetMockedLogger<TagController>(Log.Logger));
        }

        [Test]
        public void GetTags()
        {
            var tags = InsertTags(2);
            
            var apiTags = TagController.GetTags();
            Assert.AreEqual(tags.Count, apiTags.Length);
        }
        [Test]
        public void GetTagGroups()
        {
            var tags = InsertTags(2);

            var tagGroupDict = TagController.GetTagGroups();
            Assert.AreEqual(1, tagGroupDict.Count);
            Assert.AreEqual(tags.Count, tagGroupDict.First().Value.Length);

            var tagGroup = InsertTagGroups(1)[0];
            Assert.AreEqual(2, TagController.GetTagGroups().Count);

            var tagToChange = tags[0];
            Assert.IsTrue(DatabaseOperations.ChangeTagGroup(tagToChange, tagGroup));
            tagGroupDict = TagController.GetTagGroups();
            Assert.AreEqual(2, tagGroupDict.Count);
            Assert.AreEqual(tagToChange.Name, tagGroupDict[tagGroup.Name][0]);
        }
    }
}
