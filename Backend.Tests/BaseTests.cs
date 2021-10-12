using Backend.Entities;
using NUnit.Framework;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using Util;

namespace Backend.Tests
{
    public class BaseTests
    {
        protected static DatabaseContext Db => ConnectionManager.Instance.Database;

        [SetUp]
        public virtual void SetUp()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(formatter: new LogFormatter())
                .CreateLogger();
            ConnectionManager.InitDb("TestDb", dropDb: true, logTo: DatabaseQueryLogger.Instance.Information);
        }

        protected static List<Tag> InsertTags(int count)
        {
            using var db = ConnectionManager.NewContext();
            var tags = Enumerable.Range(1, count).Select(i => new Tag { Name = $"tag{i}" }).ToList();
            db.Tags.AddRange(tags);
            db.SaveChanges();

            return tags;
        }
        protected static List<Artist> InsertArtist(int count)
        {
            using var db = ConnectionManager.NewContext();
            var artists = Enumerable.Range(1, count).Select(i => 
            new Artist
            { 
                Id = $"artist{i}", 
                Name = $"artist{i}" 
            }).ToList();
            db.Artists.AddRange(artists);
            db.SaveChanges();

            return artists;
        }
        protected static List<Album> InsertAlbums(int count)
        {
            using var db = ConnectionManager.NewContext();
            var albums = Enumerable.Range(1, count).Select(i => 
            new Album 
            { 
                Id = $"album{i}", 
                Name = $"album{i}" 
            }).ToList();
            db.Albums.AddRange(albums);
            db.SaveChanges();

            return albums;
        }
        protected static List<Track> InsertTracks(int count, List<Artist> artists, List<Album> albums)
        {
            using var db = ConnectionManager.NewContext();
            var tracks = Enumerable.Range(1, count).Select(i =>
            new Track
            {
                Id = $"track{i}",
                Name = $"track{i}",
                //Album = albums[i % albums.Count],
                Artists = new List<Artist> { artists[i % artists.Count] },
            }).ToList();
            db.AttachRange(artists);
            db.AttachRange(albums);
            db.Tracks.AddRange(tracks);
            db.SaveChanges();
            return tracks;
        }

        // TODO
        protected static void RefreshDbConnection()
        {
            Db.Dispose();
            ConnectionManager.InitDb("TestDb", logTo: DatabaseQueryLogger.Instance.Information);
        }

        [TearDown]
        public virtual void TearDown()
        {
            Log.CloseAndFlush();
        }
    }
}
