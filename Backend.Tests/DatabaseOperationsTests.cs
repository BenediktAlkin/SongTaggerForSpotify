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
        #region add/edit/delete tags
        [Test]
        [TestCase(10)]
        public void Tags_GetTags(int count)
        {
            InsertTags(count);

            Assert.AreEqual(count, DatabaseOperations.GetTags().Count);
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
        public void Tags_IsValidTag()
        {
            InsertTags(1);

            using (var db = ConnectionManager.NewContext())
            {
                Assert.IsFalse(DatabaseOperations.IsValidTag(db.Tags.First().Name, db));
                Assert.IsTrue(DatabaseOperations.IsValidTag("asdf", db));
                Assert.IsFalse(DatabaseOperations.IsValidTag("", db));
                Assert.IsFalse(DatabaseOperations.IsValidTag(null, db));
            }
        }
        [Test]
        public void Tags_AddTag()
        {
            var tags = InsertTags(1);

            using (var db = ConnectionManager.NewContext())
            {
                Assert.IsFalse(DatabaseOperations.AddTag(tags[0]));
                Assert.IsTrue(DatabaseOperations.AddTag(new Tag { Name = "asdf" }));
                Assert.IsNotNull(db.Tags.FirstOrDefault(t => t.Name == "asdf"));
                Assert.IsFalse(DatabaseOperations.AddTag(new Tag { Name = "asdf" }));
                Assert.IsFalse(DatabaseOperations.AddTag(new Tag { Name = "" }));
                Assert.IsFalse(DatabaseOperations.AddTag(new Tag { }));
                Assert.IsFalse(DatabaseOperations.AddTag(null));
                Assert.AreEqual(2, db.Tags.Count());
            }
        }
        [Test]
        public void Tags_Edit()
        {
            var tags = InsertTags(1);

            using (var db = ConnectionManager.NewContext())
            {
                Assert.IsFalse(DatabaseOperations.EditTag(null, "asdf"));
                Assert.IsFalse(DatabaseOperations.EditTag(new Tag(), "asdf"));
                Assert.IsTrue(DatabaseOperations.EditTag(tags[0], "asdf"));
                Assert.AreEqual("asdf", db.Tags.First(t => t.Id == tags[0].Id).Name);
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
        #endregion

        #region assign tags to tracks
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
        #endregion

        #region GraphGeneratorPage
        [Test]
        [TestCase(10)]
        public void GraphGeneratorPage_Get(int count)
        {
            InsertGraphGeneratorPages(count);

            Assert.AreEqual(count, DatabaseOperations.GetGraphGeneratorPages().Count);
        }
        [Test]
        public void GraphGeneratorPage_Add()
        {
            using (var db = ConnectionManager.NewContext())
            {
                Assert.IsTrue(DatabaseOperations.AddGraphGeneratorPage(new GraphGeneratorPage { Name = "ggp1" }));
                Assert.IsFalse(DatabaseOperations.AddGraphGeneratorPage(null));
                Assert.AreEqual(1, db.GraphGeneratorPages.Count());
            }
        }
        [Test]
        public void GraphGeneratorPage_Edit()
        {
            var pages = InsertGraphGeneratorPages(1);
            
            using (var db = ConnectionManager.NewContext())
            {
                Assert.IsFalse(DatabaseOperations.EditGraphGeneratorPage(null, "asdf"));
                Assert.IsFalse(DatabaseOperations.EditGraphGeneratorPage(new GraphGeneratorPage(), "asdf"));
                Assert.IsTrue(DatabaseOperations.EditGraphGeneratorPage(pages[0], "asdf"));
                Assert.AreEqual("asdf", db.GraphGeneratorPages.First(ggp => ggp.Id == pages[0].Id).Name);
            }
        }
        [Test]
        public void GraphGeneratorPage_Delete()
        {
            var pages = InsertGraphGeneratorPages(1);

            using (var db = ConnectionManager.NewContext())
            {
                Assert.IsFalse(DatabaseOperations.DeleteGraphGeneratorPage(null));
                Assert.IsFalse(DatabaseOperations.DeleteGraphGeneratorPage(new GraphGeneratorPage()));
                Assert.IsTrue(DatabaseOperations.DeleteGraphGeneratorPage(pages[0]));
                Assert.AreEqual(0, db.GraphGeneratorPages.Count());
            }
        }
        #endregion

        #region GraphNodes
        [Test]
        public void GraphNode_Add()
        {
            var ggp = new GraphGeneratorPage { Name = "testpage" };
            DatabaseOperations.AddGraphGeneratorPage(ggp);
            using (var db = ConnectionManager.NewContext())
            {
                Assert.AreEqual(1, db.GraphGeneratorPages.Count());
            }

            var newNode1 = new ConcatNode
            {
                X = 0.5,
                Y = 0.7,
            }; 
            var newNode2 = new ConcatNode
            {
                X = 0.5,
                Y = 0.7,
            };

            using (var db = ConnectionManager.NewContext())
            {
                Assert.IsTrue(DatabaseOperations.AddGraphNode(newNode1, ggp));
                Assert.IsTrue(DatabaseOperations.AddGraphNode(newNode2, ggp));
                Assert.IsFalse(DatabaseOperations.AddGraphNode(null, ggp));

                Assert.AreEqual(2, db.GraphNodes.Count());
            }
        }
        [Test]
        public void GraphNode_Edit()
        {
            var nodes = InsertGraphNodes(1);

            using (var db = ConnectionManager.NewContext())
            {
                Assert.IsTrue(DatabaseOperations.EditGraphNode(nodes[0], 0.1, 0.2));
                Assert.IsFalse(DatabaseOperations.EditGraphNode(null, 0.1, 0.2));
                Assert.IsFalse(DatabaseOperations.EditGraphNode(new ConcatNode(), 0.1, 0.2));

                Assert.AreEqual(0.1, db.GraphNodes.First(gn => gn.Id == nodes[0].Id).X);
                Assert.AreEqual(0.2, db.GraphNodes.First(gn => gn.Id == nodes[0].Id).Y);
            }
        }

        [Test]
        public void GraphNode_PlaylistOutputNode_Edit()
        {
            var nodes = InsertGraphNodes<PlaylistOutputNode>(1, gn => gn.PlaylistName = $"genPlaylist{gn.Id}");

            using (var db = ConnectionManager.NewContext())
            {
                Assert.IsTrue(DatabaseOperations.EditPlaylistOutputNode(nodes[0], "asdf"));
                Assert.IsFalse(DatabaseOperations.EditPlaylistOutputNode(null, "asdf"));
                Assert.IsFalse(DatabaseOperations.EditPlaylistOutputNode(new PlaylistOutputNode(), "asdf"));

                Assert.AreEqual("asdf", db.PlaylistOutputNodes.First(gn => gn.Id == nodes[0].Id).PlaylistName);
            }
        }
        [Test]
        public void GraphNode_RemoveSet_SwapSets()
        {
            var baseSetNode = InsertGraphNodes(1)[0];
            var nodes = InsertGraphNodes<RemoveNode>(1, gn => gn.BaseSetId = baseSetNode.Id);

            using (var db = ConnectionManager.NewContext())
            {
                Assert.IsTrue(DatabaseOperations.SwapRemoveNodeSets(nodes[0]));
                Assert.IsFalse(DatabaseOperations.SwapRemoveNodeSets(null));
                Assert.IsFalse(DatabaseOperations.SwapRemoveNodeSets(new RemoveNode()));

                var removeSetType = db.RemoveNodes
                    .Include(gn => gn.RemoveSet)
                    .First(gn => gn.Id == nodes[0].Id)
                    .RemoveSet
                    .GetType();
                Assert.AreEqual(typeof(ConcatNode), removeSetType);
                Assert.IsNull(db.RemoveNodes.Include(gn => gn.BaseSet).First(gn => gn.Id == nodes[0].Id).BaseSet);
            }
        }
        [Test]
        public void GraphNode_FilterYearNode_Edit()
        {
            var nodes = InsertGraphNodes<FilterYearNode>(3);

            using (var db = ConnectionManager.NewContext())
            {
                Assert.IsTrue(DatabaseOperations.EditFilterYearNode(nodes[0], 1999, null));
                Assert.IsTrue(DatabaseOperations.EditFilterYearNode(nodes[1], null, 2000));
                Assert.IsTrue(DatabaseOperations.EditFilterYearNode(nodes[2], 2010, 2020));
                Assert.IsFalse(DatabaseOperations.EditFilterYearNode(null, 1990, 2005));
                Assert.IsFalse(DatabaseOperations.EditFilterYearNode(new FilterYearNode(), 1909, 2100));

                Assert.AreEqual(1999, db.FilterYearNodes.First(gn => gn.Id == nodes[0].Id).YearFrom);
                Assert.AreEqual(null, db.FilterYearNodes.First(gn => gn.Id == nodes[0].Id).YearTo);
                Assert.AreEqual(null, db.FilterYearNodes.First(gn => gn.Id == nodes[1].Id).YearFrom);
                Assert.AreEqual(2000, db.FilterYearNodes.First(gn => gn.Id == nodes[1].Id).YearTo);
                Assert.AreEqual(2010, db.FilterYearNodes.First(gn => gn.Id == nodes[2].Id).YearFrom);
                Assert.AreEqual(2020, db.FilterYearNodes.First(gn => gn.Id == nodes[2].Id).YearTo);
            }
        }

        [Test]
        public void GraphNode_AssignTagNode_Edit()
        {
            var tags = InsertTags(1);
            var nodes = InsertGraphNodes<AssignTagNode>(1);

            using (var db = ConnectionManager.NewContext())
            {
                Assert.IsFalse(DatabaseOperations.EditAssignTagNode(null, tags[0]));
                Assert.IsFalse(DatabaseOperations.EditAssignTagNode(new AssignTagNode(), tags[0]));
                Assert.IsFalse(DatabaseOperations.EditAssignTagNode(nodes[0], new Tag { Name = "asdf" }));

                Assert.IsTrue(DatabaseOperations.EditAssignTagNode(nodes[0], tags[0]));
                Assert.AreEqual(tags[0].Id, db.AssignTagNodes.FirstOrDefault(gn => gn.Id == nodes[0].Id).TagId);
            }

            using (var db = ConnectionManager.NewContext())
            {
                // not sure why this needs a new context
                Assert.IsTrue(DatabaseOperations.EditAssignTagNode(nodes[0], null));
                Assert.IsNull(db.AssignTagNodes.FirstOrDefault(gn => gn.Id == nodes[0].Id).TagId);
            }
        }
        [Test]
        public void GraphNode_FilterTagNode_Edit()
        {
            var tags = InsertTags(1);
            var nodes = InsertGraphNodes<FilterTagNode>(1);

            using (var db = ConnectionManager.NewContext())
            {
                Assert.IsFalse(DatabaseOperations.EditFilterTagNode(null, tags[0]));
                Assert.IsFalse(DatabaseOperations.EditFilterTagNode(new FilterTagNode(), tags[0]));
                Assert.IsFalse(DatabaseOperations.EditFilterTagNode(nodes[0], new Tag { Name = "asdf" }));

                Assert.IsTrue(DatabaseOperations.EditFilterTagNode(nodes[0], tags[0]));
                Assert.AreEqual(tags[0].Id, db.FilterTagNodes.FirstOrDefault(gn => gn.Id == nodes[0].Id).TagId);
            }

            using (var db = ConnectionManager.NewContext())
            {
                // not sure why this needs a new context
                Assert.IsTrue(DatabaseOperations.EditFilterTagNode(nodes[0], null));
                Assert.IsNull(db.FilterTagNodes.FirstOrDefault(gn => gn.Id == nodes[0].Id).TagId);
            }
        }
        [Test]
        public void GraphNode_FilterArtistNode_Edit()
        {
            var artists = InsertArtist(1);
            var nodes = InsertGraphNodes<FilterArtistNode>(1);

            using (var db = ConnectionManager.NewContext())
            {
                Assert.IsFalse(DatabaseOperations.EditFilterArtistNode(null, artists[0]));
                Assert.IsFalse(DatabaseOperations.EditFilterArtistNode(new FilterArtistNode(), artists[0]));
                Assert.IsFalse(DatabaseOperations.EditFilterArtistNode(nodes[0], new Artist { Id="asdf", Name = "asdf" }));

                Assert.IsTrue(DatabaseOperations.EditFilterArtistNode(nodes[0], artists[0]));
                Assert.AreEqual(artists[0].Id, db.FilterArtistNodes.FirstOrDefault(gn => gn.Id == nodes[0].Id).ArtistId);
            }

            using (var db = ConnectionManager.NewContext())
            {
                // not sure why this needs a new context
                Assert.IsTrue(DatabaseOperations.EditFilterArtistNode(nodes[0], null));
                Assert.IsNull(db.FilterArtistNodes.FirstOrDefault(gn => gn.Id == nodes[0].Id).ArtistId);
            }
        }
        [Test]
        public void GraphNode_PlaylistInputNode_Edit()
        {
            var playlists = InsertPlaylists(1);
            var nodes = InsertGraphNodes<PlaylistInputMetaNode>(1);

            using (var db = ConnectionManager.NewContext())
            {
                Assert.IsFalse(DatabaseOperations.EditPlaylistInputNode(null, playlists[0]));
                Assert.IsFalse(DatabaseOperations.EditPlaylistInputNode(new PlaylistInputMetaNode(), playlists[0]));
                Assert.IsFalse(DatabaseOperations.EditPlaylistInputNode(nodes[0], new Playlist { Id = "asdf", Name = "asdf" }));

                Assert.IsTrue(DatabaseOperations.EditPlaylistInputNode(nodes[0], playlists[0]));
                Assert.AreEqual(playlists[0].Id, db.PlaylistInputNodes.FirstOrDefault(gn => gn.Id == nodes[0].Id).PlaylistId);
            }

            using (var db = ConnectionManager.NewContext())
            {
                // not sure why this needs a new context
                Assert.IsTrue(DatabaseOperations.EditPlaylistInputNode(nodes[0], null));
                Assert.IsNull(db.PlaylistInputNodes.FirstOrDefault(gn => gn.Id == nodes[0].Id).PlaylistId);
            }
        }

        [Test]
        public void GraphNode_AddConnection()
        {
            var nodes = InsertGraphNodes(2);

            using (var db = ConnectionManager.NewContext())
            {
                Assert.IsFalse(DatabaseOperations.AddGraphNodeConnection(null, nodes[1]));
                Assert.IsFalse(DatabaseOperations.AddGraphNodeConnection(nodes[0], null));
                Assert.IsFalse(DatabaseOperations.AddGraphNodeConnection(new ConcatNode(), nodes[1]));
                Assert.IsFalse(DatabaseOperations.AddGraphNodeConnection(nodes[0], new ConcatNode()));
                Assert.IsTrue(DatabaseOperations.AddGraphNodeConnection(nodes[0], nodes[1]));
                Assert.IsFalse(DatabaseOperations.AddGraphNodeConnection(nodes[0], nodes[1]));

                Assert.AreEqual(1, db.GraphNodes.Include(gn => gn.Outputs).First(gn => gn.Id == nodes[0].Id).Outputs.Count());
                Assert.AreEqual(1, db.GraphNodes.Include(gn => gn.Inputs).First(gn => gn.Id == nodes[1].Id).Inputs.Count());
            }
        }
        [Test]
        public void GraphNode_DeleteConnection()
        {
            var nodes = InsertGraphNodes(2);
            DatabaseOperations.AddGraphNodeConnection(nodes[0], nodes[1]);

            using (var db = ConnectionManager.NewContext())
            {
                Assert.IsFalse(DatabaseOperations.DeleteGraphNodeConnection(null, nodes[1]));
                Assert.IsFalse(DatabaseOperations.DeleteGraphNodeConnection(nodes[0], null));
                Assert.IsFalse(DatabaseOperations.DeleteGraphNodeConnection(new ConcatNode(), nodes[1]));
                Assert.IsFalse(DatabaseOperations.DeleteGraphNodeConnection(nodes[0], new ConcatNode()));
                Assert.IsTrue(DatabaseOperations.DeleteGraphNodeConnection(nodes[0], nodes[1]));
                Assert.IsFalse(DatabaseOperations.DeleteGraphNodeConnection(nodes[0], nodes[1]));

                Assert.AreEqual(0, db.GraphNodes.Include(gn => gn.Outputs).First(gn => gn.Id == nodes[0].Id).Outputs.Count());
                Assert.AreEqual(0, db.GraphNodes.Include(gn => gn.Inputs).First(gn => gn.Id == nodes[1].Id).Inputs.Count());
            }
        }
        #endregion
    }
}