using Backend.Entities;
using Backend.Entities.GraphNodes;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Backend
{
    public static class DatabaseOperations
    {
        private static ILogger Logger { get; } = Log.ForContext("SourceContext", "DB");

        #region tags
        public static List<Tag> GetTagsWithGroups()
        {
            Logger.Information($"loading tags");
            using var db = ConnectionManager.NewContext();
            var tags = db.Tags.Include(t => t.TagGroup).ToList();
            Logger.Information($"loaded {tags.Count} tags");
            return tags;
        }
        public static bool TagExists(string tagName, DatabaseContext db = null)
        {
            if (string.IsNullOrEmpty(tagName)) return false;
            tagName = tagName.ToLower();

            var needsDispose = false;
            if (db == null)
            {
                db = ConnectionManager.NewContext();
                needsDispose = true;
            }

            var tagExists = db.Tags.FirstOrDefault(t => t.Name == tagName) != null;
            //Logger.Information($"tag {tagName} exists {tagExists}");

            if (needsDispose)
                db.Dispose();
            return tagExists;
        }
        public static bool IsValidTag(string tagName, DatabaseContext db = null)
        {
            if (string.IsNullOrEmpty(tagName))
            {
                //Logger.Information($"invalid tag {tagName} (null or empty)");
                return false;
            }

            var tagExists = TagExists(tagName, db);
            if (tagExists)
            {
                //Logger.Information($"invalid tag {tagName} (already exists)");
                return false;
            }
            return true;
        }
        public static bool AddTag(Tag tag)
        {
            if (tag == null)
            {
                Logger.Information("can't add tag (is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            if (!IsValidTag(tag.Name, db))
            {
                Logger.Information($"can't add tag {tag.Name}");
                return false;
            }

            tag.Name = tag.Name.ToLower();
            db.Tags.Add(tag);
            db.SaveChanges();
            Logger.Information($"added tag {tag.Name}");
            return true;
        }
        public static bool EditTag(Tag tag, string newName)
        {
            if (tag == null)
            {
                Logger.Information("can't edit tag (is null)");
                return false;
            }
            if(tag.Name == null)
            {
                Logger.Information("can't edit tag (name is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            if (!IsValidTag(newName, db)) return false;

            var dbTag = db.Tags.FirstOrDefault(t => t.Name == tag.Name.ToLower());
            if (dbTag == null)
            {
                Logger.Information($"can't update tag {tag.Name} (not in db)");
                return false;
            }

            dbTag.Name = newName;
            db.SaveChanges();
            Logger.Information($"updated tagname old={tag.Name} new={newName}");
            return true;
        }
        public static bool DeleteTag(Tag tag)
        {
            if (tag == null)
            {
                Logger.Information($"can't delete tag (is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            try
            {
                db.Tags.Remove(tag);
                db.SaveChanges();
            }
            catch (Exception)
            {
                Logger.Information($"can't delete tag {tag.Name} (does not exist)");
                return false;
            }
            Logger.Information($"deleted tag {tag.Name}");
            return true;
        }
        #endregion

        #region TagGroups
        public static List<TagGroup> GetTagGroups()
        {
            Logger.Information($"loading TagGroups");
            using var db = ConnectionManager.NewContext();
            var tagGroups = db.TagGroups.Include(tg => tg.Tags.OrderBy(t => t.Name)).OrderBy(tg => tg.Order).ToList();
            Logger.Information($"loaded {tagGroups.Count} TagGroups with {tagGroups.SelectMany(tg => tg.Tags).Count()} tags");
            return tagGroups;
        }
        public static bool IsValidTagGroupName(string name) => !string.IsNullOrWhiteSpace(name);
        public static bool AddTagGroup(TagGroup tagGroup)
        {
            if (tagGroup == null)
            {
                Logger.Information("can't add tagGroup (is null)");
                return false;
            }
            if (!IsValidTagGroupName(tagGroup.Name))
            {
                Logger.Information("can't add tagGroup (name null/empty/whitespace)");
                return false;
            }

            using var db = ConnectionManager.NewContext();

            db.TagGroups.Add(tagGroup);
            try
            {
                db.SaveChanges();
            }catch(DbUpdateException e)
            {
                Logger.Information($"can't add tagGroup (probably duplicate id)" +
                    $"{e.Message} - {e.InnerException?.Message}");
                return false;
            }
            
            // sqlite does not support autoincrement for non-key properties
            // set order equal to id
            tagGroup.Order = tagGroup.Id;
            db.SaveChanges();

            Logger.Information($"added tagGroup {tagGroup.Name}");
            return true;
        }
        public static bool EditTagGroup(TagGroup tagGroup, string newName)
        {
            if (tagGroup == null)
            {
                Logger.Information("can't edit tagGroup (is null)");
                return false;
            }
            if (string.IsNullOrWhiteSpace(newName))
            {
                Logger.Information("can't edit tagGroup (newName null/empty/whitespace)");
                return false;
            }

            using var db = ConnectionManager.NewContext();

            var dbTagGroup = db.TagGroups.FirstOrDefault(tg => tg.Id == tagGroup.Id);
            if (dbTagGroup == null)
            {
                Logger.Information($"can't update tagGroup {tagGroup.Name} (not in db)");
                return false;
            }

            dbTagGroup.Name = newName;
            db.SaveChanges();
            Logger.Information($"updated tagGroupName old={tagGroup.Name} new={newName}");
            return true;
        }
        public static bool DeleteTagGroup(TagGroup tagGroup)
        {
            if (tagGroup == null)
            {
                Logger.Information($"can't delete tagGroup (is null)");
                return false;
            }
            if(tagGroup.Id == Constants.DEFAULT_TAGGROUP_ID)
            {
                Logger.Information("can't delete default tagGroup");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            try
            {
                db.TagGroups.Remove(tagGroup);
                db.SaveChanges();
            }
            catch (DbUpdateException e)
            {
                Logger.Information($"can't delete tagGroup {tagGroup.Name} (probably doesn't exist): " +
                    $"{e.Message} - {e.InnerException?.Message}");
                return false;
            }
            Logger.Information($"deleted tagGroup {tagGroup.Name} and all its tags ({string.Join(',', tagGroup.Tags.Select(t => t.Name))})");
            return true;
        }

        public static bool ChangeTagGroup(Tag tag, TagGroup tagGroup)
        {
            if (tag == null)
            {
                Logger.Information("can't change TagGroup (tag is null)");
                return false;
            }
            if (tag.Name == null)
            {
                Logger.Information("can't change TagGroup (tagName is null)");
                return false;
            }
            if (tagGroup == null)
            {
                Logger.Information("can't change TagGroup (tagGroup is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            var dbTag = db.Tags.FirstOrDefault(t => t.Name == tag.Name.ToLower());
            if (dbTag == null)
            {
                Logger.Information($"can't change TagGroup (tag {tag.Name} is not in db)");
                return false;
            }
            var dbTagGroup = db.TagGroups.FirstOrDefault(tg => tg.Id == tagGroup.Id);
            if (dbTagGroup == null)
            {
                Logger.Information($"can't change TagGroup (tagGroup {tagGroup.Name} is not in db)");
                return false;
            }

            dbTag.TagGroupId = dbTagGroup.Id;
            db.SaveChanges();

            Logger.Information($"changed tag {dbTag.Name} to tagGroup {dbTagGroup.Name}");
            return true;
        }
        public static bool SwapTagGroupOrder(TagGroup a, TagGroup b)
        {
            if(a == null || b == null)
            {
                Logger.Information("can't swap TagGroups (is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            var dbA = db.TagGroups.FirstOrDefault(tg => tg.Id == a.Id);
            var dbB = db.TagGroups.FirstOrDefault(tg => tg.Id == b.Id);
            if(dbA == null || dbB == null)
            {
                Logger.Information("can't swap TagGroups (not in db)");
                return false;
            }

            var temp = dbA.Order;
            dbA.Order = dbB.Order;
            dbB.Order = temp;
            db.SaveChanges();

            Logger.Information($"swapped order of TagGroups {dbA.Name} and {dbB.Name}");
            return true;
        }
        #endregion

        #region tracks
        public static bool AssignTag(string trackId, string tagName) => AssignTag(new Track { Id = trackId }, new Tag { Name = tagName });
        public static bool AssignTag(Track track, Tag tag)
        {
            if (track == null)
            {
                Logger.Information($"can't assign tag {tag?.Name} to track null");
                return false;
            }
            if (tag == null)
            {
                Logger.Information($"can't assign tag null to track {track.Name}");
                return false;
            }
            if (tag.Name == null)
            {
                Logger.Information($"can't assign tag (tag.Name is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            // get tag from db
            var dbTag = db.Tags.FirstOrDefault(t => t.Name == tag.Name.ToLower());
            if (dbTag == null)
            {
                Logger.Information($"can't assign tag {tag.Name} to {track.Name} (tag does not exist)");
                return false;
            }

            // get track from db
            var dbTrack = db.Tracks.Include(t => t.Tags).FirstOrDefault(t => t.Id == track.Id);
            if (dbTrack == null)
            {
                Logger.Information($"can't assign tag {tag.Name} to {track.Name} (track does not exist)");
                return false;
            }

            // avoid duplicates
            if (dbTrack.Tags.Contains(dbTag))
            {
                Logger.Information($"can't assign tag {tag.Name} to {track.Name} (already assigned)");
                return false;
            }

            dbTrack.Tags.Add(dbTag);
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Logger.Information($"can't assign tag {dbTag.Name} to {dbTrack.Name}: " +
                    $"{e.Message} - {e.InnerException?.Message}");
                return false;
            }

            Logger.Information($"assigned tag {dbTag.Name} to {dbTrack.Name}");
            return true;
        }
        public static bool DeleteAssignment(string trackId, string tagName) => DeleteAssignment(new Track { Id = trackId }, new Tag { Name = tagName });
        public static bool DeleteAssignment(Track track, Tag tag)
        {
            if (track == null)
            {
                Logger.Information($"can't delete track-tag assignment (track is null)");
                return false;
            }
            if (tag == null)
            {
                Logger.Information($"can't delete track-tag assignment (tag is null)");
                return false;
            }
            if(tag.Name == null)
            {
                Logger.Information($"can't delete tag-track assignment (tag.Name is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            // get tag from db
            var dbTag = db.Tags.FirstOrDefault(t => t.Name == tag.Name.ToLower());
            if (dbTag == null)
            {
                Logger.Information($"can't assign tag {tag.Name} to {track.Name} (tag does not exist)");
                return false;
            }

            // get track from db
            var dbTrack = db.Tracks.Include(t => t.Tags).FirstOrDefault(t => t.Id == track.Id);
            if (dbTrack == null)
            {
                Logger.Information($"can't assign tag {tag.Name} to {track.Name} (track does not exist)");
                return false;
            }

            if (!dbTrack.Tags.Contains(dbTag))
            {
                Logger.Information($"can't delete tag {tag.Name} from track {track.Name} (no assignment)");
                return false;
            }

            dbTrack.Tags.Remove(dbTag);
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Logger.Information($"can't delete tag {dbTag.Name} from track {dbTrack.Name}: " +
                    $"{e.Message} - {e.InnerException?.Message}");
                return false;
            }

            Logger.Information($"removed tag {dbTag.Name} from track {dbTrack.Name}");
            return true;
        }
        public static bool AddTrack(Track track)
        {
            if (track == null)
            {
                Logger.Information($"can't add track (track is null)");
                return false;
            }
            if (track.Album == null)
            {
                Logger.Information($"can't add track (album is null)");
                return false;
            }
            if (track.Artists == null || track.Artists.Count == 0)
            {
                Logger.Information($"can't add track (no artists)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            // check if track already exists
            var dbTrack = db.Tracks.FirstOrDefault(t => t.Id == track.Id);
            if (dbTrack != null)
            {
                Logger.Information($"can't add track (already exists)");
                return false;
            }

            // replace album with dbAlbum if it is already in db
            var dbAlbum = db.Albums.FirstOrDefault(a => a.Id == track.Album.Id);
            if (dbAlbum != null)
                track.Album = dbAlbum;

            // replace artists with dbArtists if they are already in db
            for (var i = 0; i < track.Artists.Count; i++)
            {
                var dbArtist = db.Artists.FirstOrDefault(a => a.Id == track.Artists[i].Id);
                if (dbArtist != null)
                    track.Artists[i] = dbArtist;
            }


            db.Tracks.Add(track);
            db.SaveChanges();
            Logger.Information($"added track {track.Name}");
            return true;
        }
        #endregion

        #region graphNodePages
        public static List<GraphGeneratorPage> GetGraphGeneratorPages()
        {
            Logger.Information("loading pages");
            using var db = ConnectionManager.NewContext();
            var pages = db.GraphGeneratorPages.ToList();
            Logger.Information($"loaded {pages.Count} pages");
            return pages;
        }
        public static bool AddGraphGeneratorPage(GraphGeneratorPage page)
        {
            if (page == null)
            {
                Logger.Information("can't add GraphGeneratorPage (page is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            db.GraphGeneratorPages.Add(page);
            db.SaveChanges();
            Logger.Information($"added GraphGeneratorPage {page.Name}");
            return true;
        }
        public static bool DeleteGraphGeneratorPage(GraphGeneratorPage page)
        {
            if (page == null)
            {
                Logger.Information("can't delete GraphGeneratorPage (page is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            try
            {
                db.GraphGeneratorPages.Remove(page);
                db.SaveChanges();
            }
            catch (Exception)
            {
                Logger.Information($"can't delete GraphGeneratorPage {page.Name} (does not exist)");
                return false;
            }
            Logger.Information($"deleted GraphGeneratorPage {page.Name}");
            return true;
        }
        public static bool EditGraphGeneratorPage(GraphGeneratorPage page, string newName)
        {
            if (page == null)
            {
                Logger.Information("can't edit GraphGeneratorPage (page is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            var dbGgp = db.GraphGeneratorPages.FirstOrDefault(ggp => ggp.Id == page.Id);
            if (dbGgp == null)
            {
                Logger.Information($"can't edit GraphGeneratorPage {page.Name} (not in db)");
                return false;
            }

            dbGgp.Name = newName;
            db.SaveChanges();
            Logger.Information($"updated pagename old={page.Name} new={newName}");
            return true;
        }
        #endregion

        #region graphNodes
        public static List<GraphNode> GetGraphNodes(GraphGeneratorPage ggp)
        {
            if (ggp == null) return new();

            var nodes = new List<GraphNode>();
            using var db = ConnectionManager.NewContext();
            var nodeIds = db.GraphNodes.Where(gn => gn.GraphGeneratorPageId == ggp.Id).Select(gn => gn.Id).ToList();

            IQueryable<T> BaseQuery<T>(DbSet<T> set) where T : GraphNode
                => set.Include(gn => gn.Outputs).Where(gn => nodeIds.Contains(gn.Id));

            nodes.AddRange(BaseQuery(db.AssignTagNodes).Include(gn => gn.Tag));
            nodes.AddRange(BaseQuery(db.ConcatNodes));
            nodes.AddRange(BaseQuery(db.DeduplicateNodes));
            nodes.AddRange(BaseQuery(db.FilterArtistNodes).Include(gn => gn.Artist));
            nodes.AddRange(BaseQuery(db.FilterTagNodes).Include(gn => gn.Tag));
            nodes.AddRange(BaseQuery(db.FilterUntaggedNodes));
            nodes.AddRange(BaseQuery(db.FilterYearNodes));
            nodes.AddRange(BaseQuery(db.IntersectNodes));
            nodes.AddRange(BaseQuery(db.PlaylistInputLikedNodes).Include(gn => gn.Playlist));
            nodes.AddRange(BaseQuery(db.PlaylistInputMetaNodes).Include(gn => gn.Playlist));
            nodes.AddRange(BaseQuery(db.PlaylistOutputNodes));
            nodes.AddRange(BaseQuery(db.RemoveNodes).Include(gn => gn.BaseSet).Include(gn => gn.RemoveSet));
            return nodes;
        }
        public static List<IRunnableGraphNode> GetRunnableGraphNodes(GraphGeneratorPage ggp)
        {
            var dbGraphNodes = DatabaseOperations.GetGraphNodes(ggp);
            return dbGraphNodes.Where(gn => gn is IRunnableGraphNode).Cast<IRunnableGraphNode>().ToList();
        }
        public static bool AddGraphNode(GraphNode node, GraphGeneratorPage ggp)
        {
            if (node == null)
            {
                Logger.Information("can't add GraphNode (is null)");
                return false;
            }
            if (ggp == null)
            {
                Logger.Information("can't add GraphNode (GraphGeneratorPage is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            var dbGgp = db.GraphGeneratorPages.FirstOrDefault(dbGgp => dbGgp.Id == ggp.Id);
            if (dbGgp == null)
            {
                Logger.Information("can't add GraphNode (GraphGeneratorPage does not exist)");
                return false;
            }
            node.GraphGeneratorPage = dbGgp;
            db.GraphNodes.Add(node);
            db.SaveChanges();
            Logger.Information($"added GraphNode {node}");
            return true;
        }
        public static bool EditGraphNode(GraphNode node, double posX, double posY)
        {
            if (node == null)
            {
                Logger.Information("can't update GraphNode (is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            var dbNode = db.GraphNodes.FirstOrDefault(n => n.Id == node.Id);
            if (dbNode == null)
            {
                Logger.Information($"can't update GraphNode {node} (not in db)");
                return false;
            }

            var oldX = dbNode.X;
            var oldY = dbNode.Y;
            dbNode.X = posX;
            dbNode.Y = posY;
            db.SaveChanges();
            Logger.Information($"updated GraphNode posX {oldX:N2} --> {posX:N2} posY {oldY:N2} --> {posY:N2}");
            return true;
        }
        public static bool DeleteGraphNode(GraphNode node)
        {
            if (node == null)
            {
                Logger.Information("can't delete graphNode (is null)");
                return false;
            }
            using var db = ConnectionManager.NewContext();
            try
            {
                db.GraphNodes.Remove(node);
                db.SaveChanges();
            }
            catch (Exception)
            {
                Logger.Information($"can't delete GraphNode {node} (does not exist)");
                return false;
            }
            Logger.Information($"deleted GraphNode {node}");
            return true;
        }

        public static bool EditPlaylistOutputNodeName(PlaylistOutputNode node, string newName)
        {
            if (node == null)
            {
                Logger.Information("can't update PlaylistOutputNode (is null)");
                return false;
            }
            // null value is allowed (graphnode becomes invalid but it is updated in db)
            //if (string.IsNullOrEmpty(newName))
            //{
            //    Logger.Information("can't update PlaylistOutputNode (newName is null)");
            //    return false;
            //}

            using var db = ConnectionManager.NewContext();
            var dbNode = db.PlaylistOutputNodes.FirstOrDefault(gn => gn.Id == node.Id);
            if (dbNode == null)
            {
                Logger.Information("can't update PlaylistOutputNode (not in db)");
                return false;
            }

            var oldName = dbNode.PlaylistName;
            dbNode.PlaylistName = newName;
            db.SaveChanges();

            Logger.Information($"updated PlaylistOutputNode name old={oldName} new={newName}");
            return true;
        }
        public static bool EditPlaylistOutputNodeGeneratedPlaylistId(PlaylistOutputNode node, string newId)
        {
            if (node == null)
            {
                Logger.Information("can't update PlaylistOutputNode (is null)");
                return false;
            }
            if (string.IsNullOrEmpty(newId))
            {
                Logger.Information("can't update PlaylistOutputNode (newId is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            var dbNode = db.PlaylistOutputNodes.FirstOrDefault(gn => gn.Id == node.Id);
            if (dbNode == null)
            {
                Logger.Information("can't update PlaylistOutputNode (not in db)");
                return false;
            }

            var oldId = dbNode.GeneratedPlaylistId;
            dbNode.GeneratedPlaylistId = newId;
            db.SaveChanges();

            Logger.Information($"updated PlaylistOutputNode GeneratedPlaylistId old={oldId} new={newId}");
            return true;
        }
        public static bool SwapRemoveNodeSets(RemoveNode node)
        {
            if (node == null)
            {
                Logger.Information("can't swap sets of RemoveNode (is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            var dbNode = db.RemoveNodes
                .Include(gn => gn.BaseSet)
                .Include(gn => gn.RemoveSet)
                .FirstOrDefault(gn => gn.Id == node.Id);
            if (dbNode == null)
            {
                Logger.Information("can't swap sets of RemoveNode (not in db)");
                return false;
            }

            dbNode.SwapSets();
            db.SaveChanges();

            Logger.Information($"swapped sets of RemoveNode oldBaseSet={node.BaseSet} " +
                $"oldRemoveSet={node.RemoveSet} newBaseSet={dbNode.BaseSet} newRemoveSet={dbNode.RemoveSet}");
            return true;
        }
        public static bool EditFilterYearNode(FilterYearNode node, int? newFrom, int? newTo)
        {
            if (node == null)
            {
                Logger.Information("can't update FilterYearNode (is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            var dbNode = db.FilterYearNodes.FirstOrDefault(gn => gn.Id == node.Id);
            if (dbNode == null)
            {
                Logger.Information("can't update FilterYearNode (not in db)");
                return false;
            }

            var oldFrom = dbNode.YearFrom;
            var oldTo = dbNode.YearTo;
            dbNode.YearFrom = newFrom;
            dbNode.YearTo = newTo;
            db.SaveChanges();

            Logger.Information($"updated FilterYearNode oldFrom={oldFrom} oldTo={oldTo} " +
                $"newFrom={node.YearFrom} newTo={dbNode.YearTo}");
            return true;
        }

        public static bool EditAssignTagNode(AssignTagNode node, Tag tag)
        {
            if (node == null)
            {
                Logger.Information("can't update AssignTagNode (node is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            var dbNode = db.AssignTagNodes.FirstOrDefault(gn => gn.Id == node.Id);
            if (dbNode == null)
            {
                Logger.Information("can't update AssignTagNode (not in db)");
                return false;
            }
            Tag dbTag = null;
            if (tag != null)
            {
                dbTag = db.Tags.FirstOrDefault(t => t.Id == tag.Id);
                if (dbTag == null)
                {
                    Logger.Information("can't update AssignTagNode (tag not in db)");
                    return false;
                }
            }

            var oldTagId = dbNode.TagId;
            dbNode.TagId = dbTag?.Id;
            db.SaveChanges();
            Logger.Information($"updated AssignTagNode oldTagId={oldTagId} newTagId={dbNode.TagId} newTag={dbTag}");
            return true;
        }
        public static bool EditFilterTagNode(FilterTagNode node, Tag tag)
        {
            if (node == null)
            {
                Logger.Information("can't update FilterTagNode (node is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            var dbNode = db.FilterTagNodes.FirstOrDefault(gn => gn.Id == node.Id);
            if (dbNode == null)
            {
                Logger.Information("can't update FilterTagNode (not in db)");
                return false;
            }
            Tag dbTag = null;
            if (tag != null)
            {
                dbTag = db.Tags.FirstOrDefault(t => t.Id == tag.Id);
                if (dbTag == null)
                {
                    Logger.Information("can't update FilterTagNode (tag not in db)");
                    return false;
                }
            }

            var oldTagId = dbNode.TagId;
            dbNode.TagId = dbTag?.Id;
            db.SaveChanges();
            Logger.Information($"updated FilterTagNode oldTagId={oldTagId} newTagId={dbNode.TagId} newTag={dbTag}");
            return true;
        }
        public static bool EditFilterArtistNode(FilterArtistNode node, Artist artist)
        {
            if (node == null)
            {
                Logger.Information("can't update FilterArtistNode (node is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            var dbNode = db.FilterArtistNodes.FirstOrDefault(gn => gn.Id == node.Id);
            if (dbNode == null)
            {
                Logger.Information("can't update FilterArtistNode (not in db)");
                return false;
            }
            Artist dbArtist = null;
            if (artist != null)
            {
                dbArtist = db.Artists.FirstOrDefault(a => a.Id == artist.Id);
                if (dbArtist == null)
                {
                    Logger.Information("can't update FilterArtistNode (artist not in db)");
                    return false;
                }
            }

            var oldArtistId = dbNode.ArtistId;
            dbNode.ArtistId = dbArtist?.Id;
            db.SaveChanges();
            Logger.Information($"updated FilterArtistNode oldArtistId={oldArtistId} " +
                $"newArtistId={dbNode.ArtistId} newArtist={dbArtist}");
            return true;
        }
        public static bool EditPlaylistInputNode(PlaylistInputNode node, Playlist playlist)
        {
            if (node == null)
            {
                Logger.Information("can't update PlaylistInputNode (node is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            var dbNode = db.PlaylistInputNodes.FirstOrDefault(gn => gn.Id == node.Id);
            if (dbNode == null)
            {
                Logger.Information("can't update PlaylistInputNode (not in db)");
                return false;
            }
            Playlist dbPlaylist = null;
            if (playlist != null)
            {
                dbPlaylist = db.Playlists.FirstOrDefault(a => a.Id == playlist.Id);
                if (dbPlaylist == null)
                {
                    Logger.Information("can't update PlaylistInputNode (artist not in db)");
                    return false;
                }
            }

            var oldPlaylistId = dbNode.PlaylistId;
            dbNode.PlaylistId = dbPlaylist?.Id;
            db.SaveChanges();
            Logger.Information($"updated PlaylistInputNode oldPlaylistId={oldPlaylistId} " +
                $"newPlaylistId={dbNode.PlaylistId} newPlalist={dbPlaylist}");
            return true;
        }
        #endregion

        #region GraphNode connections
        public static bool AddGraphNodeConnection(GraphNode from, GraphNode to)
        {
            if (from == null)
            {
                Logger.Information("can't add GarphNode connection (from is null)");
                return false;
            }
            if (to == null)
            {
                Logger.Information("can't add GarphNode connection (to is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            // get from db
            var fromDb = db.GraphNodes.Include(gn => gn.Outputs).FirstOrDefault(gn => gn.Id == from.Id);
            if (fromDb == null)
            {
                Logger.Information("can't add GraphNode connection (from not in db)");
                return false;
            }
            var toDb = db.GraphNodes.Include(gn => gn.Inputs).FirstOrDefault(gn => gn.Id == to.Id);
            if (toDb == null)
            {
                Logger.Information("can't add GraphNode connection (to not in db)");
                return false;
            }

            if (fromDb.Outputs.Contains(toDb))
            {
                Logger.Information("can't add GraphNode connection (already connected)");
                return false;
            }

            fromDb.AddOutput(toDb);
            db.SaveChanges();

            Logger.Information($"added connection between {from} and {to}");
            return true;
        }
        public static bool DeleteGraphNodeConnection(GraphNode from, GraphNode to)
        {
            if (from == null)
            {
                Logger.Information("can't remove GarphNode connection (from is null)");
                return false;
            }
            if (to == null)
            {
                Logger.Information("can't remove GarphNode connection (to is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            // get from db
            var fromDb = db.GraphNodes.Include(gn => gn.Outputs).FirstOrDefault(gn => gn.Id == from.Id);
            if (fromDb == null)
            {
                Logger.Information("can't remove GraphNode connection (from not in db)");
                return false;
            }
            var toDb = db.GraphNodes.Include(gn => gn.Inputs).FirstOrDefault(gn => gn.Id == to.Id);
            if (toDb == null)
            {
                Logger.Information("can't remove GraphNode connection (to not in db)");
                return false;
            }

            if (!fromDb.Outputs.Contains(toDb))
            {
                Logger.Information("can't remove GraphNode connection (not connected)");
                return false;
            }

            fromDb.RemoveOutput(toDb);
            db.SaveChanges();

            Logger.Information($"removed connection between {from} and {to}");
            return true;

        }
        #endregion



        #region get playlists
        public static List<Playlist> PlaylistsLiked()
        {
            Logger.Information("loading liked playlists");
            using var db = ConnectionManager.NewContext();
            var generatedPlaylistIds = db.PlaylistOutputNodes.Select(p => p.GeneratedPlaylistId);
            var playlists = db.Playlists
                .Where(p => !generatedPlaylistIds.Contains(p.Id) && !Constants.META_PLAYLIST_IDS.Contains(p.Id))
                .OrderBy(p => p.Name.ToLower())
                .ToList();
            Logger.Information($"loaded {playlists.Count} liked playlists");
            return playlists;
        }
        public static Playlist PlaylistsMeta(string id)
        {
            if (!Constants.META_PLAYLIST_IDS.Contains(id)) return null;
            using var db = ConnectionManager.NewContext();
            return db.Playlists.FirstOrDefault(p => p.Id == id);
        }
        public static List<Playlist> PlaylistsMeta()
        {
            Logger.Information("loading meta playlists");
            using var db = ConnectionManager.NewContext();
            var playlists = db.Playlists
                .Where(p => Constants.META_PLAYLIST_IDS.Contains(p.Id))
                .OrderBy(p => p.Name)
                .ToList();
            Logger.Information($"loaded {playlists.Count} meta playlists");
            return playlists;
        }
        public static List<Playlist> PlaylistsGenerated()
        {
            Logger.Information("loading generated playlists");
            using var db = ConnectionManager.NewContext();
            var playlistOutputNodes = db.PlaylistOutputNodes.ToList();
            var playlists = playlistOutputNodes
                .Where(node => node.GeneratedPlaylistId != null)
                .Select(node => new Playlist { Id = node.GeneratedPlaylistId, Name = node.PlaylistName })
                .OrderBy(p => p.Name).ToList();
            Logger.Information($"loaded {playlists.Count} generated playlists");
            return playlists;
        }
        #endregion

        #region get playlist tracks
        public static List<Track> MetaPlaylistTracks(string playlistId, bool includeAlbums = true, bool includeArtists = true, bool includeTags = true)
        {
            using var db = ConnectionManager.NewContext();
            var query = GetTrackIncludeQuery(db, includeAlbums, includeArtists, includeTags);
            return playlistId switch
            {
                Constants.ALL_SONGS_PLAYLIST_ID => query.ToList(),
                Constants.UNTAGGED_SONGS_PLAYLIST_ID => query.Where(t => t.Tags.Count == 0).ToList(),
                Constants.LIKED_SONGS_PLAYLIST_ID => query.Where(t => t.IsLiked).ToList(),
                _ => new()
            };
        }
        public static List<Track> PlaylistTracks(string playlistId, bool includeAlbums = true, bool includeArtists = true, bool includeTags = true)
        {
            using var db = ConnectionManager.NewContext();
            var query = GetTrackIncludeQuery(db, includeAlbums, includeArtists, includeTags);
            return query.Where(t => t.Playlists.Select(p => p.Id).Contains(playlistId)).ToList();
        }
        public static async Task<List<Track>> GeneratedPlaylistTracks(string id, bool includeAlbums = true, bool includeArtists = true, bool includeTags = true)
        {
            using var db = ConnectionManager.NewContext();
            var playlistOutputNode = db.PlaylistOutputNodes.FirstOrDefault(p => p.GeneratedPlaylistId == id);
            if (playlistOutputNode == null)
            {
                Logger.Error($"Could not find PlaylistOutputNode with GeneratedPlaylistId {id}");
                return new();
            }
            var spotifyTracks = await SpotifyOperations.GetPlaylistTracks(playlistOutputNode.GeneratedPlaylistId);
            var spotifyTrackIds = spotifyTracks.Select(t => t.Id);

            // replace spotify track with db track
            var query = GetTrackIncludeQuery(db, includeAlbums, includeArtists, includeTags);
            return query.Where(t => spotifyTrackIds.Contains(t.Id)).ToList();
        }
        public static List<Track> TagPlaylistTracks(int tagId, bool includeAlbums = true, bool includeArtists = true, bool includeTags = true)
        {
            using var db = ConnectionManager.NewContext();
            var tag = db.Tags.FirstOrDefault(t => t.Id == tagId);
            if(tag == null)
            {
                Logger.Error($"Couldn't find Tag with TagId {tagId}");
                return new();
            }

            var query = GetTrackIncludeQuery(db, includeAlbums, includeArtists, true);
            return query.Where(t => t.Tags.Contains(new Tag { Id = tagId })).ToList();
        }
        private static IQueryable<Track> GetTrackIncludeQuery(DatabaseContext db, bool includeAlbums, bool includeArtists, bool includeTags)
        {
            IQueryable<Track> query = db.Tracks
                .Include(t => t.Playlists);
            if (includeAlbums)
                query = query.Include(t => t.Album);
            if (includeArtists)
                query = query.Include(t => t.Artists);
            if (includeTags)
                query = query.Include(t => t.Tags);
            return query;
        }
        #endregion



        #region AssignTagNode
        public static async Task<bool> AssignTags(AssignTagNode assignTagNode)
        {
            if (assignTagNode.AnyBackward(gn => !gn.IsValid))
            {
                Logger.Information("can't run AssignTagNode (graph contains invalid node)");
                return false;
            }

            await Task.Run(() =>
            {
                assignTagNode.CalculateOutputResult();
                var tracks = assignTagNode.OutputResult;

                foreach (var track in tracks)
                    AssignTag(track, assignTagNode.Tag);
                Logger.Information($"assigned tag {assignTagNode.Tag.Name} to {tracks.Count} tracks");
            });
            return true;
        }
        #endregion

        #region sync library
        private static void ReplacePlaylistsWithDbPlaylists(DatabaseContext db, Dictionary<string, Playlist> playlistDict, Track track)
        {
            for (var i = 0; i < track.Playlists.Count; i++)
            {
                var playlist = track.Playlists[i];
                if (playlistDict.TryGetValue(playlist.Id, out var addedPlaylist))
                {
                    // replace artist with the artist that is already added to the dbContext
                    track.Playlists[i] = addedPlaylist;
                }
                else
                {
                    // add playlist to the dbContext
                    playlistDict[playlist.Id] = playlist;
                    db.Playlists.Add(playlist);
                }
            }
        }
        private static void ReplaceArtistWithDbArtist(DatabaseContext db, Dictionary<string, Artist> artistDict, Track track)
        {
            for (var i = 0; i < track.Artists.Count; i++)
            {
                var artist = track.Artists[i];
                if (artistDict.TryGetValue(artist.Id, out var addedArtist))
                {
                    // replace artist with the artist that is already added to the dbContext
                    track.Artists[i] = addedArtist;
                }
                else
                {
                    // add artist to the dbContext
                    artistDict[artist.Id] = artist;
                    db.Artists.Add(artist);
                }
            }
        }
        private static void ReplaceAlbumWithDbAlbum(DatabaseContext db, Dictionary<string, Album> albumDict, Track track)
        {
            if (albumDict.TryGetValue(track.Album.Id, out var addedAlbum))
            {
                // replace album with the album that is already added to the dbContext
                track.Album = addedAlbum;
            }
            else
            {
                // add artist to the dbContext
                albumDict[track.Album.Id] = track.Album;
                db.Albums.Add(track.Album);
            }
        }
        private static void ReplaceTagWithDbTag(DatabaseContext db, Dictionary<int, Tag> tagDict, Track track)
        {
            for (var i = 0; i < track.Tags.Count; i++)
            {
                var tag = track.Tags[i];
                if (tagDict.TryGetValue(tag.Id, out var addedTag))
                {
                    // replace artist with the artist that is already added to the dbContext
                    track.Tags[i] = addedTag;
                }
                else
                {
                    // add artist to the dbContext
                    tagDict[tag.Id] = tag;
                    db.Tags.Add(tag);
                }
            }
        }

        private static Task SyncLibraryTask { get; set; }
        public static async Task SyncLibrary(CancellationToken cancellationToken = default)
        {
            // wait for old task to finish (if it is cancelled)
            if (SyncLibraryTask != null && !SyncLibraryTask.IsCompleted)
            {
                Logger.Information($"Waiting for currently running SyncLibrary to finish");
                try
                {
                    await SyncLibraryTask;
                }
                catch (OperationCanceledException e)
                {
                    Logger.Information($"Error waiting for currently running SyncLibrary to finish {e.Message}");
                }

                Logger.Information($"Finished waiting for currently running SyncLibrary to finish");
            }

            if (DataContainer.Instance.User == null) return;
            SyncLibraryTask = Task.Run(async () =>
            {
                Logger.Information("Syncing library");
                using var db = ConnectionManager.NewContext();
                // exclude generated playlists from library
                var playlistOutputNodes = db.PlaylistOutputNodes.ToList();
                var generatedPlaylistIds = playlistOutputNodes.Select(pl => pl.GeneratedPlaylistId).ToList();
                cancellationToken.ThrowIfCancellationRequested();

                // start fetching spotify library
                var getSpotifyLibraryTask = SpotifyOperations.GetFullLibrary(generatedPlaylistIds);

                // get db data
                var dbTracks = db.Tracks.Include(t => t.Tags).Include(t => t.Playlists).ToDictionary(t => t.Id, t => t);
                cancellationToken.ThrowIfCancellationRequested();
                var dbPlaylists = db.Playlists.ToDictionary(pl => pl.Id, pl => pl);
                var dbArtists = db.Artists.ToDictionary(a => a.Id, a => a);
                cancellationToken.ThrowIfCancellationRequested();
                var dbAlbums = db.Albums.ToDictionary(a => a.Id, a => a);
                cancellationToken.ThrowIfCancellationRequested();

                // await fetching spotify library
                var (spotifyPlaylists, spotifyTracks) = await getSpotifyLibraryTask;
                if (spotifyPlaylists == null && spotifyTracks == null)
                {
                    Logger.Error($"Error syncing library: could not retrieve Spotify library");
                    return;
                }
                cancellationToken.ThrowIfCancellationRequested();


                // push spotify library to db
                Logger.Information("Start pushing library to database");
                var nonTrackedTrackIds = dbTracks.Select(kv => kv.Value.Id).ToList();
                foreach (var track in spotifyTracks.Values)
                {
                    nonTrackedTrackIds.Remove(track.Id);
                    // replace spotify playlist objects with db playlist objects
                    ReplacePlaylistsWithDbPlaylists(db, dbPlaylists, track);
                    if (dbTracks.TryGetValue(track.Id, out var dbTrack))
                    {
                        // update the playlist sources in case the song has been added/removed from a playlist
                        dbTrack.Playlists = track.Playlists;
                        // update IsLiked
                        dbTrack.IsLiked = track.IsLiked;
                    }
                    else
                    {
                        Logger.Information($"Adding {track.Name} - {track.ArtistsString} to db");
                        // replace spotify album/artist objects with db album/artist objects
                        ReplaceAlbumWithDbAlbum(db, dbAlbums, track);
                        ReplaceArtistWithDbArtist(db, dbArtists, track);
                        // add track to db
                        db.Tracks.Add(track);
                    }
                    cancellationToken.ThrowIfCancellationRequested();
                }
                db.SaveChanges();
                Logger.Information("Finished pushing library to database");
                cancellationToken.ThrowIfCancellationRequested();

                // update IsLiked of unliked tracks that have a tag
                Logger.Information("Start removing unliked tracks with tag from LikedSongs");
                var unlikedTracks = dbTracks.Values.Where(t => nonTrackedTrackIds.Contains(t.Id));
                foreach (var dbTrack in unlikedTracks)
                    dbTrack.IsLiked = false;
                db.SaveChanges();
                Logger.Information("Finished removing unliked tracks with tag from LikedSongs");
                cancellationToken.ThrowIfCancellationRequested();


                // remove tracks that are no longer in the library and are not tagged
                Logger.Information("Start removing untracked tracks from db");
                var nonTaggedTracks = dbTracks.Values.Where(t => nonTrackedTrackIds.Contains(t.Id) && t.Tags.Count == 0);
                foreach (var nonTaggedTrack in nonTaggedTracks)
                {
                    if (!spotifyTracks.ContainsKey(nonTaggedTrack.Id))
                    {
                        db.Tracks.Remove(nonTaggedTrack);
                        Logger.Information($"Removed {nonTaggedTrack.Name} - {nonTaggedTrack.ArtistsString} " +
                            $"from db (no longer in library & untagged)");
                    }
                }
                db.SaveChanges();
                Logger.Information("Finished removing untracked tracks from db");
                cancellationToken.ThrowIfCancellationRequested();


                // remove unlikedplaylists
                Logger.Information("Update remove unliked playlists");
                var allPlaylists = db.Playlists.Include(p => p.Tracks).ToList();
                var spotifyPlaylistIds = spotifyPlaylists.Select(p => p.Id);
                var playlistsToRemove = allPlaylists.Where(p => !spotifyPlaylistIds.Contains(p.Id) && !Constants.META_PLAYLIST_IDS.Contains(p.Id));
                foreach (var playlist in playlistsToRemove)
                {
                    Logger.Information($"Removing playlist {playlist.Name} (unliked)");
                    db.Playlists.Remove(playlist);
                }
                db.SaveChanges();
                Logger.Information("Finished removing unliked playlists");
                cancellationToken.ThrowIfCancellationRequested();


                // update playlist names
                Logger.Information("Update liked playlist names");
                var allPlaylistsDict = allPlaylists.ToDictionary(p => p.Id, p => p);
                foreach (var spotifyPlaylist in spotifyPlaylists)
                    allPlaylistsDict[spotifyPlaylist.Id].Name = spotifyPlaylist.Name;
                db.SaveChanges();
                Logger.Information("Finished updating liked playlist names");
            }, cancellationToken);
            await SyncLibraryTask;

            await DataContainer.Instance.LoadSourcePlaylists(forceReload: true);
        }
        #endregion
    }
}
