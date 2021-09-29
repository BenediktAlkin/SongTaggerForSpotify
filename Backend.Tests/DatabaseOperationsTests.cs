using Backend.Entities;
using Backend.Entities.GraphNodes;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Tests
{
    public class DatabaseOperationsTests : BaseTests
    {

        [Test]
        public async Task ImportExport_Tags_Simple()
        {
            // create some tags
            var tag = new Tag { Id = 1, Name = "tag1" };
            var track = new Track
            {
                Id = "track1",
                Name = "track1",
                DurationMs = 123,
                Album = new Album
                {
                    Id = "album1",
                    Name = "album1",
                    ReleaseDate = "2016",
                    ReleaseDatePrecision = "day",
                },
            };
            var artist = new Artist {Id = "artist1", Name = "artist1" };
            Db.Artists.Add(artist);
            track.Artists = new() { artist };
            Db.Tracks.Add(track);
            tag.Tracks = new() { track };
            Db.Tags.Add(tag);
            Db.SaveChanges();
            await DatabaseOperations.ExportTags("tagexport.tags");

            // connect to new db
            ConnectionManager.InitDb("OtherDb", dropDb: true, logTo: DatabaseQueryLogger.Instance.Information);
            await DatabaseOperations.ImportTags("tagexport.tags");

            var importedTags = Db.Tags.Include(t => t.Tracks).ThenInclude(tr => tr.Album).Include(t => t.Tracks).ThenInclude(tr => tr.Artists).ToList();
            Assert.AreEqual(1, importedTags.Count);
            Assert.AreEqual(1, importedTags.First().Tracks.Count);
            Assert.AreEqual(1, importedTags.First().Tracks.First().Artists.Count);
            Assert.AreNotEqual(0, importedTags.First().Tracks.First().DurationMs);
        }
    }
}