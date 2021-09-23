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
        public void Input_Output(int playlistIdx)
        {
            var inputNode = new PlaylistInputNode { Playlist = Playlists[playlistIdx] };
            var outputNode = new PlaylistOutputNode();
            outputNode.AddInput(inputNode);

            using (new DatabaseQueryLogger.Context())
            {
                var nTracks = N_TRACKS / N_PLAYLISTS;
                if (playlistIdx < N_TRACKS - nTracks * N_PLAYLISTS)
                    nTracks++;
                Assert.AreEqual(nTracks, outputNode.GetOutputResult().Result.Count);
                Assert.AreEqual(1, DatabaseQueryLogger.Instance.MessageCount);
            }
        }

        [Test]
        public void ConcatNode()
        {
            var concatNode = new ConcatNode();
            foreach (var playlist in Playlists)
                concatNode.AddInput(new PlaylistInputNode { Playlist = playlist });
            Assert.AreEqual(N_TRACKS, concatNode.GetOutputResult().Result.Count);
        }

        [Test]
        public void DeduplicateNode()
        {
            var concatNode = new ConcatNode();
            foreach (var playlist in Playlists)
                concatNode.AddInput(new PlaylistInputNode { Playlist = playlist });
            foreach (var playlist in Playlists)
                concatNode.AddInput(new PlaylistInputNode { Playlist = playlist });
            var deduplicateNode = new DeduplicateNode();
            deduplicateNode.AddInput(concatNode);
            Assert.AreEqual(N_TRACKS * 2, concatNode.GetOutputResult().Result.Count);
            Assert.AreEqual(N_TRACKS, deduplicateNode.GetOutputResult().Result.Count);
        }

        [Test]
        [TestCaseSource(nameof(TagIdxs))]
        public void AllInputs_TagFilter_Output(int tagIdx)
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
                Assert.AreEqual(nTracks, outputNode.GetOutputResult().Result.Count);
            }
        }
    }
}
