using Backend.Entities;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Tests
{
    public class ImportExportTests : BaseTests
    {

        [Test]
        public async Task ImportExport_Tags_Simple()
        {
            using var db = ConnectionManager.NewContext();

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
            var artist = new Artist { Id = "artist1", Name = "artist1" };
            db.Artists.Add(artist);
            track.Artists = new() { artist };
            db.Tracks.Add(track);
            tag.Tracks = new() { track };
            db.Tags.Add(tag);
            db.SaveChanges();
            await DatabaseOperations.ExportTags("tagexport.tags");

            // connect to new db
            var otherDbOptions = ConnectionManager.GetOptionsBuilder("OtherDb",
                logTo: DatabaseQueryLogger.Instance.Information).Options;
            using (var otherDb = new DatabaseContext(otherDbOptions, ensureCreated: true, dropDb: true))
            {
                await DatabaseOperations.ImportTags("tagexport.tags", otherDb);

                var importedTags = otherDb.Tags
                    .Include(t => t.Tracks)
                    .ThenInclude(tr => tr.Album)
                    .Include(t => t.Tracks)
                    .ThenInclude(tr => tr.Artists)
                    .ToList();
                Assert.AreEqual(1, importedTags.Count);
                Assert.AreEqual(1, importedTags.First().Tracks.Count);
                Assert.AreEqual(1, importedTags.First().Tracks.First().Artists.Count);
                Assert.AreNotEqual(0, importedTags.First().Tracks.First().DurationMs);
            }
        }
    }
}