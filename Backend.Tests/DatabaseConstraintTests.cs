using Backend.Entities;
using Backend.Entities.GraphNodes;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Tests
{
    public class DatabaseConstraintTests : BaseTests
    {

        [Test]
        public void Playlist_GraphNode_OnDeleteSetNull()
        {
            var track = new Track { Id = "Track1", Name = "Track1" };
            var playlist = new Playlist { Id = "Playlist1", Name = "Playlist1" };
            playlist.Tracks = new() { track };
            Db.Playlists.Add(playlist);
            Db.SaveChanges();
            var inputNode = new LikedPlaylistInputNode { Playlist = playlist, GraphGeneratorPage = new GraphGeneratorPage() };
            Db.GraphNodes.Add(inputNode);
            Db.SaveChanges();

            RefreshDbConnection();

            Db.Playlists.Remove(Db.Playlists.First(p => p.Id == playlist.Id));
            Db.SaveChanges();
            Assert.IsNull(Db.Playlists.FirstOrDefault(p => p.Id == playlist.Id));
            Assert.AreEqual(0, Db.Tracks.Include(t => t.Playlists).First(t => t.Id == track.Id).Playlists.Count);
            Assert.IsNotNull(Db.GraphNodes.FirstOrDefault(gn => gn.Id == inputNode.Id));
            Assert.IsNull(((LikedPlaylistInputNode)Db.GraphNodes.First(gn => gn.Id == inputNode.Id)).Playlist);
            Assert.IsNull(((LikedPlaylistInputNode)Db.GraphNodes.First(gn => gn.Id == inputNode.Id)).PlaylistId);
        }

        [Test]
        public void RemoveGraphNode_BaseSet_OnDeleteSetNull()
        {
            var ggp = new GraphGeneratorPage();
            var input1 = new LikedPlaylistInputNode 
            { 
                Playlist = new Playlist { Id = "Playlist1", Name = "Playlist1" }, 
                GraphGeneratorPage = ggp 
            };
            var input2 = new LikedPlaylistInputNode 
            { 
                Playlist = new Playlist { Id = "Playlist2", Name = "Playlist2" }, 
                GraphGeneratorPage = ggp 
            };
            var removeNode = new RemoveNode { BaseSet = input1, RemoveSet = input2, GraphGeneratorPage = ggp };
            Db.GraphNodes.Add(removeNode);
            Db.SaveChanges();

            RefreshDbConnection();

            Db.GraphNodes.Remove(Db.GraphNodes.First(gn => gn.Id == input1.Id));
            Db.SaveChanges();
            Assert.IsNull(((RemoveNode)Db.GraphNodes.First(gn => gn.Id == removeNode.Id)).BaseSetId);
            Assert.IsNotNull(((RemoveNode)Db.GraphNodes.First(gn => gn.Id == removeNode.Id)).RemoveSetId);
        }
        [Test]
        public void RemoveGraphNode_RemoveSet_OnDeleteSetNull()
        {
            var ggp = new GraphGeneratorPage();
            var input1 = new LikedPlaylistInputNode
            {
                Playlist = new Playlist { Id = "Playlist1", Name = "Playlist1" },
                GraphGeneratorPage = ggp
            };
            var input2 = new LikedPlaylistInputNode
            {
                Playlist = new Playlist { Id = "Playlist2", Name = "Playlist2" },
                GraphGeneratorPage = ggp
            };
            var removeNode = new RemoveNode { BaseSet = input1, RemoveSet = input2, GraphGeneratorPage = ggp };
            Db.GraphNodes.Add(removeNode);
            Db.SaveChanges();

            RefreshDbConnection();

            Db.GraphNodes.Remove(Db.GraphNodes.First(gn => gn.Id == input2.Id));
            Db.SaveChanges();
            Assert.IsNull(((RemoveNode)Db.GraphNodes.First(gn => gn.Id == removeNode.Id)).RemoveSetId);
            Assert.IsNotNull(((RemoveNode)Db.GraphNodes.First(gn => gn.Id == removeNode.Id)).BaseSetId);
        }
        [Test]
        public void AssignTagNode_AssignTag_OnDeleteSetNull()
        {
            var ggp = new GraphGeneratorPage();
            var tag = new Tag { Name = "Tag1" };
            var assignTagNode = new AssignTagNode { Tag = tag, GraphGeneratorPage = ggp };
            Db.GraphNodes.Add(assignTagNode);
            Db.SaveChanges();

            RefreshDbConnection();

            Db.Tags.Remove(Db.Tags.First(t => t.Id == tag.Id));
            Db.SaveChanges();
            Assert.IsNull(Db.Tags.FirstOrDefault(t => t.Id == tag.Id));
        }
        [Test]
        public void FilterTagNode_AssignTag_OnDeleteSetNull()
        {
            var ggp = new GraphGeneratorPage();
            var tag = new Tag { Name = "Tag1" };
            var filterTagNode = new FilterTagNode { Tag = tag, GraphGeneratorPage = ggp };
            Db.GraphNodes.Add(filterTagNode);
            Db.SaveChanges();

            RefreshDbConnection();

            Db.Tags.Remove(Db.Tags.First(t => t.Id == tag.Id));
            Db.SaveChanges();
            Assert.IsNull(Db.Tags.FirstOrDefault(t => t.Id == tag.Id));
        }
    }
}
