using Backend.Entities;
using Backend.Entities.GraphNodes;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

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
                Tags.Add(new Tag { Id = NewId(), Name = $"Tag{i}" });

            for (var i = 0; i < N_TRACKS; i++)
            {
                var track = new Track
                {
                    Id = $"Track{i}",
                    Name = $"Track{i}",
                    Playlists = new List<Playlist> { Playlists[i % Playlists.Count] },
                    Artists = new List<Artist> { Artists[i % Artists.Count] },
                    Album = new Album { Id = $"Album{i}", Name = $"Album{i}", ReleaseDate = $"{2000 + i}" },
                };
                track.Tags.Add(Tags[i % Tags.Count]);
                Tracks.Add(track);
            }
            using (var db = ConnectionManager.NewContext())
            {
                db.Tracks.AddRange(Tracks);
                db.SaveChanges();
            }
        }

        [Test]
        [TestCaseSource(nameof(PlaylistIdxs))]
        public void Input_Output(int playlistIdx)
        {
            var inputNode = new PlaylistInputLikedNode { Id = NewId(), Playlist = Playlists[playlistIdx] };
            var outputNode = new PlaylistOutputNode { Id = NewId() };
            outputNode.AddInput(inputNode);

            using (new DatabaseQueryLogger.Context())
            {
                var nTracks = N_TRACKS / N_PLAYLISTS;
                if (playlistIdx < N_TRACKS - nTracks * N_PLAYLISTS)
                    nTracks++;
                outputNode.CalculateOutputResult();
                Assert.AreEqual(nTracks, outputNode.OutputResult.Count);
            }
        }

        [Test]
        public void ConcatNode()
        {
            var concatNode = new ConcatNode { Id = NewId() };
            foreach (var playlist in Playlists)
                concatNode.AddInput(new PlaylistInputLikedNode { Id = NewId(), Playlist = playlist });
            concatNode.CalculateOutputResult();
            Assert.AreEqual(N_TRACKS, concatNode.OutputResult.Count);
        }

        [Test]
        public void DeduplicateNode()
        {
            var concatNode = new ConcatNode { Id = NewId() };
            foreach (var playlist in Playlists)
                concatNode.AddInput(new PlaylistInputLikedNode { Id = NewId(), Playlist = playlist });
            foreach (var playlist in Playlists)
                concatNode.AddInput(new PlaylistInputLikedNode { Id = NewId(), Playlist = playlist });
            var deduplicateNode = new DeduplicateNode { Id = NewId() };
            deduplicateNode.AddInput(concatNode);
            concatNode.CalculateOutputResult();
            Assert.AreEqual(N_TRACKS * 2, concatNode.OutputResult.Count);
            deduplicateNode.CalculateOutputResult();
            Assert.AreEqual(N_TRACKS, deduplicateNode.OutputResult.Count);
        }

        [Test]
        [TestCaseSource(nameof(TagIdxs))]
        public void AllInputs_TagFilter_Output(int tagIdx)
        {
            var concatNode = new ConcatNode { Id = NewId() };
            foreach (var playlist in Playlists)
                concatNode.AddInput(new PlaylistInputLikedNode { Id = NewId(), Playlist = playlist });
            var tagFilterNode = new FilterTagNode { Id = NewId(), Tag = Tags[tagIdx] };
            tagFilterNode.AddInput(concatNode);
            var outputNode = new PlaylistOutputNode { Id = NewId() };
            outputNode.AddInput(tagFilterNode);

            using (new DatabaseQueryLogger.Context())
            {
                var nTracks = N_TRACKS / N_TAGS;
                if (tagIdx < N_TRACKS - nTracks * N_TAGS)
                    nTracks++;
                outputNode.CalculateOutputResult();
                Assert.AreEqual(nTracks, outputNode.OutputResult.Count);
            }
        }

        [Test]
        [TestCase(2005, 2010)]
        public void AllInputs_YearFilter_Output(int yearFrom, int yearTo)
        {
            var concatNode = new ConcatNode() { Id = NewId() };
            foreach (var playlist in Playlists)
                concatNode.AddInput(new PlaylistInputLikedNode { Id = NewId(), Playlist = playlist });
            var yearFilterNode = new FilterYearNode { Id = NewId(), YearFrom = yearFrom, YearTo = yearTo };
            yearFilterNode.AddInput(concatNode);
            var outputNode = new PlaylistOutputNode { Id = NewId() };
            outputNode.AddInput(yearFilterNode);

            using (new DatabaseQueryLogger.Context())
            {
                outputNode.CalculateOutputResult();
                Assert.AreEqual(yearTo - yearFrom + 1, outputNode.OutputResult.Count);
                foreach (var track in outputNode.OutputResult)
                    Assert.IsTrue(yearFrom <= track.Album.ReleaseYear && track.Album.ReleaseYear <= yearTo);
            }
        }

        [Test]
        public void RemoveNode_NoInputs()
        {
            var removeNode = new RemoveNode { Id = NewId() };
            removeNode.CalculateOutputResult();
            Assert.AreEqual(0, removeNode.OutputResult.Count);
        }
        [Test]
        public void RemoveNode_NoInputSet()
        {
            var playlist = new PlaylistInputLikedNode { Id = NewId(), Playlist = Playlists[0] };
            var removeNode = new RemoveNode { Id = NewId() };
            removeNode.AddInput(playlist);
            removeNode.SwapSets();
            removeNode.CalculateOutputResult();
            Assert.AreEqual(0, removeNode.OutputResult.Count);
        }
        [Test]
        public void RemoveNode_NoRemoveSet()
        {
            var playlist = new PlaylistInputLikedNode { Id = NewId(), Playlist = Playlists[0] };
            var removeNode = new RemoveNode { Id = NewId() };
            removeNode.AddInput(playlist);
            removeNode.CalculateOutputResult();
            Assert.AreEqual(playlist.OutputResult.Count, removeNode.OutputResult.Count);
        }

        [Test]
        public void RemoveNode_RemoveSameNode()
        {
            var playlist1 = new PlaylistInputLikedNode { Id = NewId(), Playlist = Playlists[0] };
            var playlist2 = new PlaylistInputLikedNode { Id = NewId(), Playlist = Playlists[0] };
            var removeNode = new RemoveNode { Id = NewId() };
            removeNode.AddInput(playlist1);
            removeNode.AddInput(playlist2);

            removeNode.CalculateOutputResult();
            Assert.AreEqual(0, removeNode.OutputResult.Count);
        }
        [Test]
        public void RemoveNode_UndoConcat()
        {
            var playlist1 = new PlaylistInputLikedNode { Id = NewId(), Playlist = Playlists[0] };
            var playlist2 = new PlaylistInputLikedNode { Id = NewId(), Playlist = Playlists[1] };
            var concatNode = new ConcatNode { Id = NewId() };
            concatNode.AddInput(playlist1);
            concatNode.AddInput(playlist2);

            var removeNode = new RemoveNode { Id = NewId() };
            removeNode.AddInput(concatNode);
            removeNode.AddInput(playlist2);

            removeNode.CalculateOutputResult();
            Assert.AreEqual(playlist1.OutputResult.Count, removeNode.OutputResult.Count);
            foreach (var track in playlist1.OutputResult)
                Assert.Contains(track, removeNode.OutputResult);
        }


        [Test]
        public void DetectCycles_SameSource_NoCycle()
        {
            var input = new PlaylistInputLikedNode { Id = NewId(), Playlist = Playlists[0] };
            var allArtists = Playlists[0].Tracks.SelectMany(t => t.Artists).ToList();
            var filter1 = new FilterArtistNode { Id = NewId(), Artist = allArtists.First() };
            filter1.AddInput(input);
            var filter2 = new FilterArtistNode { Id = NewId(), Artist = allArtists.Skip(1).First() };
            filter2.AddInput(input);

            var concatNode = new ConcatNode { Id = NewId() };
            concatNode.AddInput(filter1);
            concatNode.AddInput(filter2);

            var outputNode = new PlaylistOutputNode { Id = NewId(), PlaylistName = "testplaylist" };
            outputNode.AddInput(concatNode);

            Assert.AreEqual(1, outputNode.Inputs.Count());
        }

        [Test]
        public void DetectCycles_CycleToSameNode()
        {
            var concatNode = new ConcatNode { Id = NewId() };
            concatNode.AddInput(concatNode);
            Assert.AreEqual(0, concatNode.Inputs.Count());
        }
    }
}
