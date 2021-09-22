using Backend;
using Backend.Entities;
using Backend.Entities.GraphNodes;
using NUnit.Framework;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Tests.Backend
{
    public class OperationsTests
    {
        private DatabaseContext Db;

        [SetUp]
        public void SetUp()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
            ConnectionManager.InitDb("TestDb");
            Db = ConnectionManager.Instance.Database;
        }
        [TearDown]
        public void TearDown()
        {
            Log.CloseAndFlush();
        }

        [Test]
        public void Sync_OutputNode()
        {
            var input = new InputNode { PlaylistId = "37i9dQZF1DWTvNyxOwkztu" }; // Chillout Lounge
            var output = new OutputNode
            {
                PlaylistName = "ChilloutLoungeCopy",
                Inputs = new List<GraphNode> { input },
            };

            SpotifyOperations.SyncOutputNode(output).Wait();
            Assert.AreEqual(100, SpotifyOperations.PlaylistItems(output.GeneratedPlaylistId).Result.Count);
        }

        [Test]
        public void FilterNode_NewPlaylist()
        {
            // sync library
            DatabaseOperations.SyncLibrary().Wait();
            
            // define graphs
            var input = new InputNode { PlaylistId = "37i9dQZF1DWTvNyxOwkztu" }; // Chillout Lounge
            var tag1 = new Tag { Name = "Tag1" };
            var tag2 = new Tag { Name = "Tag2" };
            var filter1 = new TagFilterNode { Tag = tag1, Inputs = new List<GraphNode> { input } };
            var filter2 = new TagFilterNode { Tag = tag2, Inputs = new List<GraphNode> { input } };
            var output1 = new OutputNode
            {
                PlaylistName = "ChilloutLoungeFiltered1",
                Inputs = new List<GraphNode> { filter1 },
            };
            var output2 = new OutputNode
            {
                PlaylistName = "ChilloutLoungeFiltered2",
                Inputs = new List<GraphNode> { filter2 },
            };
            Db.OutputNodes.Add(output1);
            Db.OutputNodes.Add(output2);

            // get tracks and add some tags
            var tracks = input.GetResult().Result;
            foreach (var i in new[] { 1, 5, 9, 13 })
                Db.Tracks.First(t => t.Id == tracks[i].Id).Tags.Add(tag1);
            foreach (var i in new[] { 5, 10, 15, 20, 30 })
                Db.Tracks.First(t => t.Id == tracks[i].Id).Tags.Add(tag2);
            Db.Tags.Add(tag1);
            Db.Tags.Add(tag2);
            Db.SaveChanges();

            // create playlists
            SpotifyOperations.SyncOutputNode(output1).Wait();
            SpotifyOperations.SyncOutputNode(output2).Wait();
            Assert.AreEqual(4, SpotifyOperations.PlaylistItems(output1.GeneratedPlaylistId).Result.Count);
            Assert.AreEqual(5, SpotifyOperations.PlaylistItems(output2.GeneratedPlaylistId).Result.Count);
        }
        [Test]
        public void FilterNode_UpdatePlaylist()
        {
            // sync library
            DatabaseOperations.SyncLibrary().Wait();

            // define graphs
            var input = new InputNode { PlaylistId = "37i9dQZF1DWTvNyxOwkztu" }; // Chillout Lounge
            var tag = new Tag { Name = "Tag" };
            var filter = new TagFilterNode { Tag = tag, Inputs = new List<GraphNode> { input } };
            var output = new OutputNode
            {
                PlaylistName = "ChilloutLoungeFiltered",
                Inputs = new List<GraphNode> { filter },
            };
            Db.OutputNodes.Add(output);

            // get tracks and add some tags
            var tracks = input.GetResult().Result;
            foreach (var i in new[] { 1, 5, 9, 13 })
                Db.Tracks.First(t => t.Id == tracks[i].Id).Tags.Add(tag);
            Db.Tags.Add(tag);
            Db.SaveChanges();

            // create playlists
            SpotifyOperations.SyncOutputNode(output).Wait();
            Assert.AreEqual(4, SpotifyOperations.PlaylistItems(output.GeneratedPlaylistId).Result.Count);

            // add some more tags
            foreach(var i in new[] {10, 20, 30})
                Db.Tracks.First(t => t.Id == tracks[i].Id).Tags.Add(tag);
            Db.SaveChanges();
            SpotifyOperations.SyncOutputNode(output).Wait();
            Assert.AreEqual(7, SpotifyOperations.PlaylistItems(output.GeneratedPlaylistId).Result.Count);
        }

        [Test]
        public void ConcatNode()
        {
            // sync library
            DatabaseOperations.SyncLibrary().Wait();

            // define graphs
            var input = new InputNode { PlaylistId = "37i9dQZF1DWTvNyxOwkztu" }; // Chillout Lounge
            var concat = new ConcatNode { Inputs = new List<GraphNode> { input, input } };
            var output = new OutputNode
            {
                PlaylistName = "ChilloutLounge2x",
                Inputs = new List<GraphNode> { concat },
            };
            Db.OutputNodes.Add(output);

            // create playlists
            SpotifyOperations.SyncOutputNode(output).Wait();
            Assert.AreEqual(200, SpotifyOperations.PlaylistItems(output.GeneratedPlaylistId).Result.Count);
        }
        [Test]
        public void DeduplicateNode()
        {
            // sync library
            DatabaseOperations.SyncLibrary().Wait();

            // define graphs
            var input = new InputNode { PlaylistId = "37i9dQZF1DWTvNyxOwkztu" }; // Chillout Lounge
            var concat = new ConcatNode { Inputs = new List<GraphNode> { input, input } };
            var deduplicate = new DeduplicateNode { Inputs = new List<GraphNode> { concat } };
            var output = new OutputNode
            {
                PlaylistName = "ChilloutLounge1x",
                Inputs = new List<GraphNode> { deduplicate },
            };
            Db.OutputNodes.Add(output);

            // create playlists
            SpotifyOperations.SyncOutputNode(output).Wait();
            Assert.AreEqual(100, SpotifyOperations.PlaylistItems(output.GeneratedPlaylistId).Result.Count);
        }
    }
}