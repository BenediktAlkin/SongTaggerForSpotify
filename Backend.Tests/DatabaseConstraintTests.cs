using Backend.Entities;
using Backend.Entities.GraphNodes;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Linq;
using Tests.Util;

namespace Backend.Tests
{
    public class DatabaseConstraintTests : BaseTests
    {
        [SetUp]
        public override void SetUp()
        {
            REQUIRES_DB = true;
            base.SetUp();
        }

        [Test]
        public void TagGroup_Default_IsInserted()
        {
            using (var db = ConnectionManager.NewContext())
            {
                var tagGroups = db.TagGroups.ToList();
                Assert.AreEqual(1, tagGroups.Count);
                var tagGroup = tagGroups.First();
                Assert.AreEqual(Constants.DEFAULT_TAGGROUP_ID, tagGroup.Id);
                Assert.AreEqual(Constants.DEFAULT_TAGGROUP_NAME, tagGroup.Name);
                Assert.AreEqual(Constants.DEFAULT_TAGGROUP_ID, tagGroup.Order);
            } 
        }
        [Test]
        public void Tag_TagGroup_DefaultIsSet()
        {
            var tag = InsertTags(1).First();
            Assert.AreEqual(Constants.DEFAULT_TAGGROUP_ID, tag.TagGroupId);
        }

        [Test]
        public void MetaPlaylists_AreInserted()
        {
            using (var db = ConnectionManager.NewContext())
            {
                var playlists = db.Playlists.ToList();
                Assert.AreEqual(Constants.META_PLAYLIST_IDS.Length, playlists.Count);
                foreach(var playlistId in Constants.META_PLAYLIST_IDS)
                {
                    var playlist = playlists.FirstOrDefault(p => p.Id == playlistId);
                    Assert.AreEqual(playlistId, playlist.Id);
                    Assert.AreEqual(playlistId, playlist.Name);
                }
            }
        }

        [Test]
        public void Playlist_GraphNode_OnDeleteSetNull()
        {
            var track = new Track { Id = "Track1", Name = "Track1" };
            var playlist = new Playlist { Id = "Playlist1", Name = "Playlist1" };
            playlist.Tracks = new() { track };
            var inputNode = new PlaylistInputLikedNode { Playlist = playlist, GraphGeneratorPage = new GraphGeneratorPage() };
            using (var db = ConnectionManager.NewContext())
            {
                db.Playlists.Add(playlist);
                db.SaveChanges();
                db.GraphNodes.Add(inputNode);
                db.SaveChanges();
            }

            using (var db = ConnectionManager.NewContext())
            {
                db.Playlists.Remove(db.Playlists.First(p => p.Id == playlist.Id));
                db.SaveChanges();
                Assert.IsNull(db.Playlists.FirstOrDefault(p => p.Id == playlist.Id));
                Assert.AreEqual(0, db.Tracks.Include(t => t.Playlists).First(t => t.Id == track.Id).Playlists.Count);
                Assert.IsNotNull(db.GraphNodes.FirstOrDefault(gn => gn.Id == inputNode.Id));
                Assert.IsNull(((PlaylistInputLikedNode)db.GraphNodes.First(gn => gn.Id == inputNode.Id)).Playlist);
                Assert.IsNull(((PlaylistInputLikedNode)db.GraphNodes.First(gn => gn.Id == inputNode.Id)).PlaylistId);
            }
        }

        [Test]
        public void RemoveGraphNode_BaseSet_OnDeleteSetNull()
        {
            var ggp = new GraphGeneratorPage();
            var input1 = new PlaylistInputLikedNode
            {
                Playlist = new Playlist { Id = "Playlist1", Name = "Playlist1" },
                GraphGeneratorPage = ggp
            };
            var input2 = new PlaylistInputLikedNode
            {
                Playlist = new Playlist { Id = "Playlist2", Name = "Playlist2" },
                GraphGeneratorPage = ggp
            };
            var removeNode = new RemoveNode { BaseSet = input1, RemoveSet = input2, GraphGeneratorPage = ggp };

            using (var db = ConnectionManager.NewContext())
            {
                db.GraphNodes.Add(removeNode);
                db.SaveChanges();
            }

            using (var db = ConnectionManager.NewContext())
            {
                db.GraphNodes.Remove(db.GraphNodes.First(gn => gn.Id == input1.Id));
                db.SaveChanges();
                Assert.IsNull(((RemoveNode)db.GraphNodes.First(gn => gn.Id == removeNode.Id)).BaseSetId);
                Assert.IsNotNull(((RemoveNode)db.GraphNodes.First(gn => gn.Id == removeNode.Id)).RemoveSetId);
            }

        }
        [Test]
        public void RemoveGraphNode_RemoveSet_OnDeleteSetNull()
        {
            var ggp = new GraphGeneratorPage();
            var input1 = new PlaylistInputLikedNode
            {
                Playlist = new Playlist { Id = "Playlist1", Name = "Playlist1" },
                GraphGeneratorPage = ggp
            };
            var input2 = new PlaylistInputLikedNode
            {
                Playlist = new Playlist { Id = "Playlist2", Name = "Playlist2" },
                GraphGeneratorPage = ggp
            };
            var removeNode = new RemoveNode { BaseSet = input1, RemoveSet = input2, GraphGeneratorPage = ggp };
            using (var db = ConnectionManager.NewContext())
            {
                db.GraphNodes.Add(removeNode);
                db.SaveChanges();
            }

            using (var db = ConnectionManager.NewContext())
            {
                db.GraphNodes.Remove(db.GraphNodes.First(gn => gn.Id == input2.Id));
                db.SaveChanges();
                Assert.IsNull(((RemoveNode)db.GraphNodes.First(gn => gn.Id == removeNode.Id)).RemoveSetId);
                Assert.IsNotNull(((RemoveNode)db.GraphNodes.First(gn => gn.Id == removeNode.Id)).BaseSetId);
            }
        }
        [Test]
        public void AssignTagNode_AssignTag_OnDeleteSetNull()
        {
            var ggp = new GraphGeneratorPage();
            var tag = new Tag { Name = "Tag1" };
            var assignTagNode = new AssignTagNode { Tag = tag, GraphGeneratorPage = ggp };
            using (var db = ConnectionManager.NewContext())
            {
                db.GraphNodes.Add(assignTagNode);
                db.SaveChanges();
            }

            using (var db = ConnectionManager.NewContext())
            {
                db.Tags.Remove(db.Tags.First(t => t.Id == tag.Id));
                db.SaveChanges();
                Assert.IsNull(db.Tags.FirstOrDefault(t => t.Id == tag.Id));
            }
        }
        [Test]
        public void FilterTagNode_AssignTag_OnDeleteSetNull()
        {
            var ggp = new GraphGeneratorPage();
            var tag = new Tag { Name = "Tag1" };
            var filterTagNode = new FilterTagNode { Tag = tag, GraphGeneratorPage = ggp };
            using (var db = ConnectionManager.NewContext())
            {
                db.GraphNodes.Add(filterTagNode);
                db.SaveChanges();
            }

            using (var db = ConnectionManager.NewContext())
            {
                db.Tags.Remove(db.Tags.First(t => t.Id == tag.Id));
                db.SaveChanges();
                Assert.IsNull(db.Tags.FirstOrDefault(t => t.Id == tag.Id));
            }
        }
    }
}
