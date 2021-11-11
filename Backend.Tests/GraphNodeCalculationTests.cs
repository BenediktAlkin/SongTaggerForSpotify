using Backend.Entities;
using Backend.Entities.GraphNodes;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Tests.Util;

namespace Backend.Tests
{
    public class GraphNodeCalculationTests : BaseTests
    {
        [SetUp]
        public override void SetUp()
        {
            REQUIRES_DB = true;
            base.SetUp();
        }


        private static readonly Type[] ALL_GRAPH_NODE_TYPES = new[]
        {
            typeof(AssignTagNode),
            typeof(ConcatNode),
            typeof(DeduplicateNode),
            typeof(FilterArtistNode),
            typeof(FilterTagNode),
            typeof(FilterUntaggedNode),
            typeof(FilterYearNode),
            typeof(IntersectNode),
            typeof(PlaylistInputLikedNode),
            typeof(PlaylistInputMetaNode),
            typeof(PlaylistOutputNode),
            typeof(RemoveNode),
        };
        private static readonly Type[] INVALIDABLE_GRAPH_NODE_TYPES = new[]
        {
            typeof(AssignTagNode), // requires tag
            typeof(FilterArtistNode), // requires artist
            typeof(FilterTagNode), // requires tag
            typeof(PlaylistInputLikedNode), // requires playlist
            typeof(PlaylistInputMetaNode), // requires playlist
            typeof(PlaylistOutputNode), // requires playlistname
        };


        private static void AssertEmptyInputResult(List<List<Track>> inputResult)
        {
            Assert.IsNotNull(inputResult);
            Assert.AreEqual(1, inputResult.Count);
            Assert.AreEqual(0, inputResult[0].Count);
        }
        private static void AssertNonEmptyInputResult(List<List<Track>> inputResult, int expected)
        {
            Assert.IsNotNull(inputResult);
            Assert.AreEqual(1, inputResult.Count);
            Assert.AreEqual(expected, inputResult[0].Count);
        }
        private static void AssertEmptyOutputResult(List<Track> outputResult)
        {
            Assert.IsNotNull(outputResult);
            Assert.AreEqual(0, outputResult.Count);
        }
        private static void AssertNonEmptyOutputResult(List<Track> outputResult, int expected)
        {
            Assert.IsNotNull(outputResult);
            Assert.AreEqual(expected, outputResult.Count);
        }

        [Test]
        [TestCaseSource(nameof(ALL_GRAPH_NODE_TYPES))]
        public void NoInput_IsEmptyList(Type t)
        {
            var graphNode = (GraphNode)Activator.CreateInstance(t);

            // check input results
            graphNode.CalculateInputResult();
            AssertEmptyInputResult(graphNode.InputResult);
            graphNode.ClearResult();

            // check output results
            graphNode.CalculateOutputResult();
            AssertEmptyInputResult(graphNode.InputResult);
            AssertEmptyOutputResult(graphNode.OutputResult);
            graphNode.ClearResult();
        }

        [Test]
        [TestCaseSource(nameof(INVALIDABLE_GRAPH_NODE_TYPES))]
        public void CalculateInputResult_Invalid_IsEmptyList(Type t)
        {
            var validInput = new PlaylistInputMetaNode
            {
                Playlist = new Playlist
                {
                    Id = Constants.LIKED_SONGS_PLAYLIST_ID,
                    Name = Constants.LIKED_SONGS_PLAYLIST_ID
                }
            };
            // insert some liked songs
            const int nTracks = 5;
            var artists = InsertArtist(nTracks);
            var albums = InsertAlbums(nTracks);
            InsertTracks(5, artists, albums, isLiked: true);

            var invalidNode = (GraphNode)Activator.CreateInstance(t);
            Assert.IsFalse(invalidNode.IsValid);

            // try to add valid input to it
            invalidNode.AddInput(validInput);

            // calculate input results
            invalidNode.CalculateInputResult();
            if (invalidNode.Inputs.Any())
            {
                // invalidNode inputResult should be non empty if it is not a PlaylistInputNode (i.e. has a input)
                AssertNonEmptyInputResult(invalidNode.InputResult, nTracks);
                // validInput inputResult/outputResult should be calculated
                AssertNonEmptyInputResult(validInput.InputResult, nTracks);
                AssertNonEmptyOutputResult(validInput.OutputResult, nTracks);
            }
            else
            {
                // invalidNode inputResult should be empty if it is a PlaylistInputNode (i.e. has no input)
                AssertEmptyInputResult(invalidNode.InputResult);
            }
            // clear calculation
            invalidNode.ClearResult();
            validInput.ClearResult();

            // calculate output result
            invalidNode.CalculateOutputResult();
            if (invalidNode.Inputs.Any())
            {
                // inputResult should be non empty if it is not a PlaylistInputNode (i.e. has a input)
                AssertNonEmptyInputResult(invalidNode.InputResult, nTracks);
                // valid input/output results should be calculated of inputNode
                AssertNonEmptyInputResult(validInput.InputResult, nTracks);
                AssertNonEmptyOutputResult(validInput.OutputResult, nTracks);
            }
            else
            {
                // invalidNode inputResult should be empty if it is a PlaylistInputNode (i.e. has no input)
                AssertEmptyInputResult(invalidNode.InputResult);
            }
            // outputResult should be empty (node is invalid)
            AssertEmptyOutputResult(invalidNode.OutputResult);
            // clear calculation
            invalidNode.ClearResult();
            validInput.ClearResult();
        }
    }
}
