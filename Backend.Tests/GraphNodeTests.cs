using Backend.Entities;
using Backend.Entities.GraphNodes;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Tests
{
    public class GraphNodeTests : BaseTests
    {
        private List<Playlist> Playlists;
        private List<Artist> Artists;
        private List<Tag> Tags;
        private List<Track> Tracks;

        const int N_PLAYLISTS = 5;
        const int N_ARTISTS = 8;
        const int N_TAGS = 3;
        const int N_TRACKS = 23;

        // test case sources
        private static readonly IEnumerable<int> PlaylistIdxs = Enumerable.Range(0, N_PLAYLISTS);
        private static readonly IEnumerable<int> TagIdxs = Enumerable.Range(0, N_TAGS);

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            Playlists = new();
            Artists = new();
            Tags = new();
            Tracks = new();
            for (var i = 0; i < N_PLAYLISTS; i++)
                Playlists.Add(new Playlist { Id = $"Playlist{i}", Name = $"Playlist{i}" });
            for (var i = 0; i < N_ARTISTS; i++)
                Artists.Add(new Artist { Id = $"Artist{i}", Name = $"Artist{i}" });
            for (var i = 0; i < N_TAGS; i++)
                Tags.Add(new Tag { Name = $"Tag{i}" });

            for (var i = 0; i < N_TRACKS; i++)
            {
                var track = new Track
                {
                    Id = $"Track{i}",
                    Name = $"Track{i}",
                    Playlists = new List<Playlist> { Playlists[i % Playlists.Count] },
                    Artists = new List<Artist> { Artists[i % Artists.Count] },
                };
                track.Tags.Add(Tags[i % Tags.Count]);
                Tracks.Add(track);
            }

            Db.Tracks.AddRange(Tracks);
            Db.SaveChanges();
        }

        [Test]
        [TestCaseSource(nameof(PlaylistIdxs))]
        public async Task Input_Output(int playlistIdx)
        {
            var inputNode = new PlaylistInputNode { Playlist = Playlists[playlistIdx] };
            var outputNode = new PlaylistOutputNode();
            outputNode.AddInput(inputNode);

            using (new DatabaseQueryLogger.Context())
            {
                var nTracks = N_TRACKS / N_PLAYLISTS;
                if (playlistIdx < N_TRACKS - nTracks * N_PLAYLISTS)
                    nTracks++;
                await outputNode.CalculateOutputResult();
                Assert.AreEqual(nTracks, outputNode.OutputResult.Count);
                Assert.AreEqual(1, DatabaseQueryLogger.Instance.MessageCount);
            }
        }

        [Test]
        public async Task ConcatNode()
        {
            var concatNode = new ConcatNode();
            foreach (var playlist in Playlists)
                concatNode.AddInput(new PlaylistInputNode { Playlist = playlist });
            await concatNode.CalculateOutputResult();
            Assert.AreEqual(N_TRACKS, concatNode.OutputResult.Count);
        }

        [Test]
        public async Task DeduplicateNode()
        {
            var concatNode = new ConcatNode();
            foreach (var playlist in Playlists)
                concatNode.AddInput(new PlaylistInputNode { Playlist = playlist });
            foreach (var playlist in Playlists)
                concatNode.AddInput(new PlaylistInputNode { Playlist = playlist });
            var deduplicateNode = new DeduplicateNode();
            deduplicateNode.AddInput(concatNode);
            await concatNode.CalculateOutputResult();
            Assert.AreEqual(N_TRACKS * 2, concatNode.OutputResult.Count);
            await deduplicateNode.CalculateOutputResult();
            Assert.AreEqual(N_TRACKS, deduplicateNode.OutputResult.Count);
        }

        [Test]
        [TestCaseSource(nameof(TagIdxs))]
        public async Task AllInputs_TagFilter_Output(int tagIdx)
        {
            var concatNode = new ConcatNode();
            foreach (var playlist in Playlists)
                concatNode.AddInput(new PlaylistInputNode { Playlist = playlist });
            var tagFilterNode = new FilterTagNode { Tag = Tags[tagIdx] };
            tagFilterNode.AddInput(concatNode);
            var outputNode = new PlaylistOutputNode();
            outputNode.AddInput(tagFilterNode);

            using (new DatabaseQueryLogger.Context())
            {
                var nTracks = N_TRACKS / N_TAGS;
                if (tagIdx < N_TRACKS - nTracks * N_TAGS)
                    nTracks++;
                await outputNode.CalculateOutputResult();
                Assert.AreEqual(nTracks, outputNode.OutputResult.Count);
            }
        }

        [Test]
        public async Task RemoveNode_NoInputs()
        {
            var removeNode = new RemoveNode();
            await removeNode.CalculateOutputResult();
            Assert.AreEqual(0, removeNode.OutputResult.Count);
        }
        [Test]
        public async Task RemoveNode_NoInputSet()
        {
            var playlist = new PlaylistInputNode { Playlist = Playlists[0] };
            var removeNode = new RemoveNode();
            removeNode.AddInput(playlist);
            removeNode.SwapSets();
            await removeNode.CalculateOutputResult();
            Assert.AreEqual(0, removeNode.OutputResult.Count);
        }
        [Test]
        public async Task RemoveNode_NoRemoveSet()
        {
            var playlist = new PlaylistInputNode { Playlist = Playlists[0] };
            var removeNode = new RemoveNode();
            removeNode.AddInput(playlist);
            await removeNode.CalculateOutputResult();
            Assert.AreEqual(playlist.OutputResult.Count, removeNode.OutputResult.Count);
        }

        [Test]
        public async Task RemoveNode_RemoveSameNode()
        {
            var playlist1 = new PlaylistInputNode { Playlist = Playlists[0] };
            var playlist2 = new PlaylistInputNode { Playlist = Playlists[0] };
            var removeNode = new RemoveNode();
            removeNode.AddInput(playlist1);
            removeNode.AddInput(playlist2);

            await removeNode.CalculateOutputResult();
            Assert.AreEqual(0, removeNode.OutputResult.Count);
        }
        [Test]
        public async Task RemoveNode_UndoConcat()
        {
            var playlist1 = new PlaylistInputNode { Playlist = Playlists[0] };
            var playlist2 = new PlaylistInputNode { Playlist = Playlists[1] };
            var concatNode = new ConcatNode();
            concatNode.AddInput(playlist1);
            concatNode.AddInput(playlist2);

            var removeNode = new RemoveNode();
            removeNode.AddInput(concatNode);
            removeNode.AddInput(playlist2);

            await removeNode.CalculateOutputResult();
            Assert.AreEqual(playlist1.OutputResult.Count, removeNode.OutputResult.Count);
            foreach (var track in playlist1.OutputResult)
                Assert.Contains(track, removeNode.OutputResult);
        }


        [Test]
        public async Task DetectCycles_SameSource_NoCycle()
        {
            var input = new PlaylistInputNode { Playlist = Playlists[0] };
            await input.CalculateOutputResult();
            var allArtists = input.OutputResult.SelectMany(t => t.Artists);
            var filter1 = new FilterArtistNode { Artist = allArtists.First() };
            filter1.AddInput(input);
            var filter2 = new FilterArtistNode { Artist = allArtists.Skip(1).First() };
            filter2.AddInput(input);

            var concatNode = new ConcatNode();
            concatNode.AddInput(filter1);
            concatNode.AddInput(filter2);

            var outputNode = new PlaylistOutputNode { PlaylistName = "testplaylist" };
            outputNode.AddInput(concatNode);

            Assert.AreEqual(1, outputNode.Inputs.Count());
        }

        [Test]
        public void DetectCycles_CycleToSameNode()
        {
            var concatNode = new ConcatNode();
            concatNode.AddInput(concatNode);
            Assert.AreEqual(0, concatNode.Inputs.Count());
        }
    }
}
