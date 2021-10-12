using Backend.Entities;
using Backend.Entities.GraphNodes;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Backend.Tests
{
    public class DatabaseOperationsTests : BaseTests
    {
        [Test]
        [TestCase(10)]
        public void Tags_GetTags(int count)
        {
            InsertTags(count);

            using (var db = ConnectionManager.NewContext())
            {
                Assert.AreEqual(count, db.Tags.Count());
            }
        }
        [Test]
        public void Tags_TagExists()
        {
            InsertTags(1);

            using (var db = ConnectionManager.NewContext())
            {
                Assert.IsTrue(DatabaseOperations.TagExists(db.Tags.First().Name, db));
                Assert.IsFalse(DatabaseOperations.TagExists("asdf", db));
                Assert.IsFalse(DatabaseOperations.TagExists("", db));
                Assert.IsFalse(DatabaseOperations.TagExists(null, db));
            }
        }
        [Test]
        public void Tags_CanAddTag()
        {
            InsertTags(1);

            using (var db = ConnectionManager.NewContext())
            {
                Assert.IsFalse(DatabaseOperations.CanAddTag(db.Tags.First().Name, db));
                Assert.IsTrue(DatabaseOperations.CanAddTag("asdf", db));
                Assert.IsFalse(DatabaseOperations.CanAddTag("", db));
                Assert.IsFalse(DatabaseOperations.CanAddTag(null, db));
            }
        }
        [Test]
        public void Tags_AddTag()
        {
            var tags = InsertTags(1);

            using (var db = ConnectionManager.NewContext())
            {
                Assert.IsFalse(DatabaseOperations.AddTag(tags[0].Name));
                Assert.IsTrue(DatabaseOperations.AddTag("asdf"));
                Assert.IsNotNull(db.Tags.FirstOrDefault(t => t.Name == "asdf"));
                Assert.IsFalse(DatabaseOperations.AddTag("asdf"));
                Assert.IsFalse(DatabaseOperations.AddTag(""));
                Assert.IsFalse(DatabaseOperations.AddTag(null));
                Assert.AreEqual(2, db.Tags.Count());
            }
        }
        [Test]
        public void Tags_DeleteTag()
        {
            var tags = InsertTags(1);

            using (var db = ConnectionManager.NewContext())
            {
                Assert.IsTrue(DatabaseOperations.DeleteTag(tags[0]));
                Assert.AreEqual(0, db.Tags.Count());
                Assert.IsFalse(DatabaseOperations.DeleteTag(null));
                Assert.IsFalse(DatabaseOperations.DeleteTag(new Tag { Id = 5 }));
            }
        }
        [Test]
        public void Track_AssignTag()
        {
            var artists = InsertArtist(10);
            var albums = InsertAlbums(10);
            var tags = InsertTags(2);
            var tracks = InsertTracks(2, artists, albums);

            List<Tag> tempTags;
            List<Track> tempTracks;
            using (var db = ConnectionManager.NewContext())
            {
                tempTags = db.Tags.ToList();
                tempTracks = db.Tracks.ToList();
            }

            using (var db = ConnectionManager.NewContext())
            {
                Assert.IsFalse(DatabaseOperations.AssignTag(null, tags[0]));
                Assert.IsFalse(DatabaseOperations.AssignTag(tracks[0], null));
                Assert.IsFalse(DatabaseOperations.AssignTag(tracks[0], new Tag { Name = "asdf" }));
                Assert.IsFalse(DatabaseOperations.AssignTag(new Track { Id = "asdf", Name = "asdf" }, tags[0]));

                Assert.IsTrue(DatabaseOperations.AssignTag(tracks[0], tags[0]));
                Assert.AreEqual(1, db.Tracks.Include(t => t.Tags).First(t => t.Id == tracks[0].Id).Tags.Count);

                Assert.IsFalse(DatabaseOperations.AssignTag(tracks[0], tags[0]));
                Assert.AreEqual(1, db.Tracks.Include(t => t.Tags).First(t => t.Id == tracks[0].Id).Tags.Count);

                Assert.IsTrue(DatabaseOperations.AssignTag(tracks[0], tags[1]));
                Assert.AreEqual(2, db.Tracks.Include(t => t.Tags).First(t => t.Id == tracks[0].Id).Tags.Count);
            }
        }
        [Test]
        public void Track_DeleteAssignment()
        {
            var artists = InsertArtist(10);
            var albums = InsertAlbums(10);
            var tags = InsertTags(2);
            var tracks = InsertTracks(2, artists, albums);
            using (var db = ConnectionManager.NewContext())
            {
                DatabaseOperations.AssignTag(tracks[0], tags[0]);
                DatabaseOperations.AssignTag(tracks[0], tags[1]);
            }

            using (var db = ConnectionManager.NewContext())
            {
                Assert.IsFalse(DatabaseOperations.DeleteAssignment(null, tags[0]));
                Assert.IsFalse(DatabaseOperations.DeleteAssignment(tracks[0], null));
                Assert.IsFalse(DatabaseOperations.DeleteAssignment(tracks[0], new Tag { Name = "asdf" }));
                Assert.IsFalse(DatabaseOperations.DeleteAssignment(tracks[1], tags[0]));
                var nodbTag = new Tag { Name = "asdf" };
                tracks[0].Tags.Add(nodbTag);
                Assert.IsFalse(DatabaseOperations.DeleteAssignment(tracks[0], nodbTag));
                tracks[0].Tags.Remove(nodbTag);
                var nodbTrack = new Track { Id = "asdf", Name = "asdf", Tags = new List<Tag> { tags[0] } };
                Assert.IsFalse(DatabaseOperations.DeleteAssignment(nodbTrack, tags[0]));

                Assert.IsTrue(DatabaseOperations.DeleteAssignment(tracks[0], tags[0]));
                Assert.AreEqual(1, db.Tracks.Include(t => t.Tags).First(t => t.Id == tracks[0].Id).Tags.Count);

                Assert.IsTrue(DatabaseOperations.DeleteAssignment(tracks[0], tags[1]));
                var asdf = db.Tracks.Include(t => t.Tags).ToList();
                // not sure why this fails here but in new context it works
                //Assert.AreEqual(0, db.Tracks.Include(t => t.Tags).First(t => t.Id == tracks[0].Id).Tags.Count);
            }
            using (var db = ConnectionManager.NewContext())
            {
                Assert.AreEqual(0, db.Tracks.Include(t => t.Tags).First(t => t.Id == tracks[0].Id).Tags.Count);
            }
        }
    }
}