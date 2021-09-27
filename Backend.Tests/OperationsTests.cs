using Backend.Entities;
using Backend.Entities.GraphNodes;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Tests
{
    public class OperationsTests : BaseTests
    {
        [Test]
        public void Sync_OutputNode()
        {
            var input = new PlaylistInputNode { PlaylistId = "37i9dQZF1DWTvNyxOwkztu" }; // Chillout Lounge
            var output = new PlaylistOutputNode
            {
                PlaylistName = "ChilloutLoungeCopy",
                Inputs = new List<GraphNode> { input },
            };

            SpotifyOperations.SyncPlaylistOutputNode(output).Wait();
            Assert.AreEqual(100, SpotifyOperations.PlaylistItems(output.GeneratedPlaylistId).Result.Count);
        }

        [Test]
        public async Task FilterNode_NewPlaylist()
        {
            // sync library
            await DatabaseOperations.SyncLibrary();

            // define graphs
            var input = new PlaylistInputNode { PlaylistId = "37i9dQZF1DWTvNyxOwkztu" }; // Chillout Lounge
            var tag1 = new Tag { Name = "Tag1" };
            var tag2 = new Tag { Name = "Tag2" };
            var filter1 = new FilterTagNode { Tag = tag1, Inputs = new List<GraphNode> { input } };
            var filter2 = new FilterTagNode { Tag = tag2, Inputs = new List<GraphNode> { input } };
            var output1 = new PlaylistOutputNode
            {
                PlaylistName = "ChilloutLoungeFiltered1",
                Inputs = new List<GraphNode> { filter1 },
            };
            var output2 = new PlaylistOutputNode
            {
                PlaylistName = "ChilloutLoungeFiltered2",
                Inputs = new List<GraphNode> { filter2 },
            };
            Db.PlaylistOutputNodes.Add(output1);
            Db.PlaylistOutputNodes.Add(output2);

            // get tracks and add some tags
            await input.CalculateOutputResult();
            var tracks = input.OutputResult;
            foreach (var i in new[] { 1, 5, 9, 13 })
                Db.Tracks.First(t => t.Id == tracks[i].Id).Tags.Add(tag1);
            foreach (var i in new[] { 5, 10, 15, 20, 30 })
                Db.Tracks.First(t => t.Id == tracks[i].Id).Tags.Add(tag2);
            Db.Tags.Add(tag1);
            Db.Tags.Add(tag2);
            Db.SaveChanges();

            // create playlists
            await SpotifyOperations.SyncPlaylistOutputNode(output1);
            await SpotifyOperations.SyncPlaylistOutputNode(output2);
            Assert.AreEqual(4, SpotifyOperations.PlaylistItems(output1.GeneratedPlaylistId).Result.Count);
            Assert.AreEqual(5, SpotifyOperations.PlaylistItems(output2.GeneratedPlaylistId).Result.Count);
        }
        [Test]
        public async Task FilterNode_UpdatePlaylist()
        {
            // sync library
            await DatabaseOperations.SyncLibrary();

            // define graphs
            var input = new PlaylistInputNode { PlaylistId = "37i9dQZF1DWTvNyxOwkztu" }; // Chillout Lounge
            var tag = new Tag { Name = "Tag" };
            var filter = new FilterTagNode { Tag = tag, Inputs = new List<GraphNode> { input } };
            var output = new PlaylistOutputNode
            {
                PlaylistName = "ChilloutLoungeFiltered",
                Inputs = new List<GraphNode> { filter },
            };
            Db.PlaylistOutputNodes.Add(output);

            // get tracks and add some tags
            await input.CalculateOutputResult();
            var tracks = input.OutputResult;
            foreach (var i in new[] { 1, 5, 9, 13 })
                Db.Tracks.First(t => t.Id == tracks[i].Id).Tags.Add(tag);
            Db.Tags.Add(tag);
            Db.SaveChanges();

            // create playlists
            await SpotifyOperations.SyncPlaylistOutputNode(output);
            Assert.AreEqual(4, SpotifyOperations.PlaylistItems(output.GeneratedPlaylistId).Result.Count);

            // add some more tags
            foreach (var i in new[] { 10, 20, 30 })
                Db.Tracks.First(t => t.Id == tracks[i].Id).Tags.Add(tag);
            Db.SaveChanges();
            await SpotifyOperations.SyncPlaylistOutputNode(output);
            Assert.AreEqual(7, SpotifyOperations.PlaylistItems(output.GeneratedPlaylistId).Result.Count);
        }

        [Test]
        public async Task ConcatNode()
        {
            // sync library
            await DatabaseOperations.SyncLibrary();

            // define graphs
            var input = new PlaylistInputNode { PlaylistId = "37i9dQZF1DWTvNyxOwkztu" }; // Chillout Lounge
            var concat = new ConcatNode { Inputs = new List<GraphNode> { input, input } };
            var output = new PlaylistOutputNode
            {
                PlaylistName = "ChilloutLounge2x",
                Inputs = new List<GraphNode> { concat },
            };
            Db.PlaylistOutputNodes.Add(output);

            // create playlists
            await SpotifyOperations.SyncPlaylistOutputNode(output);
            Assert.AreEqual(200, SpotifyOperations.PlaylistItems(output.GeneratedPlaylistId).Result.Count);
        }
        [Test]
        public async Task DeduplicateNode()
        {
            // sync library
            await DatabaseOperations.SyncLibrary();

            // define graphs
            var input = new PlaylistInputNode { PlaylistId = "37i9dQZF1DWTvNyxOwkztu" }; // Chillout Lounge
            var concat = new ConcatNode { Inputs = new List<GraphNode> { input, input } };
            var deduplicate = new DeduplicateNode { Inputs = new List<GraphNode> { concat } };
            var output = new PlaylistOutputNode
            {
                PlaylistName = "ChilloutLounge1x",
                Inputs = new List<GraphNode> { deduplicate },
            };
            Db.PlaylistOutputNodes.Add(output);

            // create playlists
            await SpotifyOperations.SyncPlaylistOutputNode(output);
            Assert.AreEqual(100, SpotifyOperations.PlaylistItems(output.GeneratedPlaylistId).Result.Count);
        }
    }
}