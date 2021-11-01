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

        private List<Tag> Tags { get; set; }

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            TagController = new TagController(MockUtil.GetMockedLogger<TagController>(Log.Logger));

            // insert tags
            Tags = InsertTags(2);
        }

        [Test]
        public void GetTags()
        {
            var apiTags = TagController.GetTags();
            Assert.AreEqual(Tags.Count, apiTags.Length);
        }
        [Test]
        public void GetTagGroups()
        {
            var tagGroups = TagController.GetTagGroups();
            Assert.AreEqual(1, tagGroups.Count);
            Assert.AreEqual(Tags.Count, tagGroups["default"].Length);
        }
    }
}
