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
        public static List<Tag> GetTags()
        {
            using var db = ConnectionManager.NewContext();
            var tags = db.Tags.ToList();
            Logger.Information($"fetched {tags.Count} tags");
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
                Logger.Information("cannot add tag (is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            if (!IsValidTag(tag.Name, db))
            {
                Logger.Information($"cannot add tag {tag.Name}");
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
                Logger.Information("cannot edit tag null");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            if (!IsValidTag(newName, db)) return false;


            var dbTag = db.Tags.FirstOrDefault(t => t.Id == tag.Id);
            if (dbTag == null)
            {
                Logger.Information($"cannot update tag {tag.Name} (not in db)");
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
                Logger.Information($"cannot delete tag (is null)");
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
                Logger.Information($"cannot delete tag {tag.Name} (does not exist)");
                return false;
            }
            Logger.Information($"deleted tag {tag.Name}");
            return true;
        }
        #endregion

        #region tracks
        public static bool AssignTag(Track track, Tag tag)
        {
            if (track == null)
            {
                Logger.Information($"cannot assign tag {tag?.Name} to track null");
                return false;
            }
            if (tag == null)
            {
                Logger.Information($"cannot assign tag null to track {track.Name}");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            // get tag from db
            var dbTag = db.Tags.FirstOrDefault(t => t.Id == tag.Id);
            if (dbTag == null)
            {
                Logger.Information($"cannot assign tag {tag.Name} to {track.Name} (tag does not exist)");
                return false;
            }

            // get track from db
            var dbTrack = db.Tracks.Include(t => t.Tags).FirstOrDefault(t => t.Id == track.Id);
            if (dbTrack == null)
            {
                Logger.Information($"cannot assign tag {tag.Name} to {track.Name} (track does not exist)");
                return false;
            }

            // avoid duplicates
            if (dbTrack.Tags.Contains(dbTag))
            {
                Logger.Information($"cannot assign tag {tag.Name} to {track.Name} (already assigned)");
                return false;
            }

            dbTrack.Tags.Add(dbTag);
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Logger.Information($"cannot assign tag {tag.Name} to {track.Name}: " +
                    $"{e.Message} - {e.InnerException?.Message}");
                return false;
            }

            Logger.Information($"assigned tag {tag.Name} to {track.Name}");
            return true;
        }
        public static bool DeleteAssignment(Track track, Tag tag)
        {
            if (track == null)
            {
                Logger.Information($"cannot delete track-tag assignment (track is null)");
                return false;
            }
            if (tag == null)
            {
                Logger.Information($"cannot delete track-tag assignment (tag is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            // get tag from db
            var dbTag = db.Tags.FirstOrDefault(t => t.Name == tag.Name.ToLower());
            if (dbTag == null)
            {
                Logger.Information($"cannot assign tag {tag.Name} to {track.Name} (tag does not exist)");
                return false;
            }

            // get track from db
            var dbTrack = db.Tracks.Include(t => t.Tags).FirstOrDefault(t => t.Id == track.Id);
            if (dbTrack == null)
            {
                Logger.Information($"cannot assign tag {tag.Name} to {track.Name} (track does not exist)");
                return false;
            }

            if (!dbTrack.Tags.Contains(dbTag))
            {
                Logger.Information($"cannot delete tag {tag.Name} from track {track.Name} (no assignment)");
                return false;
            }

            dbTrack.Tags.Remove(dbTag);
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Logger.Information($"cannot delete tag {tag.Name} from track {track.Name}: " +
                    $"{e.Message} - {e.InnerException?.Message}");
                return false;
            }

            Logger.Information($"removed tag {tag.Name} from track {track.Name}");
            return true;
        }
        #endregion

        #region graphNodePages
        public static List<GraphGeneratorPage> GetGraphGeneratorPages()
        {
            using var db = ConnectionManager.NewContext();
            return db.GraphGeneratorPages.ToList();
        }
        public static bool AddGraphGeneratorPage(GraphGeneratorPage page)
        {
            if (page == null)
            {
                Logger.Information("cannot add GraphGeneratorPage (page is null)");
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
                Logger.Information("cannot delete GraphGeneratorPage (page is null)");
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
                Logger.Information($"cannot delete GraphGeneratorPage {page.Name} (does not exist)");
                return false;
            }
            Logger.Information($"deleted GraphGeneratorPage {page.Name}");
            return true;
        }
        public static bool EditGraphGeneratorPage(GraphGeneratorPage page, string newName)
        {
            if (page == null)
            {
                Logger.Information("cannot edit GraphGeneratorPage (page is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            var dbGgp = db.GraphGeneratorPages.FirstOrDefault(ggp => ggp.Id == page.Id);
            if (dbGgp == null)
            {
                Logger.Information($"cannot edit GraphGeneratorPage {page.Name} (not in db)");
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
        public static bool AddGraphNode(GraphNode node, GraphGeneratorPage ggp)
        {
            if (node == null)
            {
                Logger.Information("cannot add GraphNode (is null)");
                return false;
            }
            if (ggp == null)
            {
                Logger.Information("cannot add GraphNode (GraphGeneratorPage is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            GraphGeneratorPage dbGgp = db.GraphGeneratorPages.FirstOrDefault(ggp => ggp.Id == ggp.Id);
            if (dbGgp == null)
            {
                Logger.Information("cannot add GraphNode (GraphGeneratorPage does not exist)");
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
                Logger.Information("cannot update GraphNode (is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            var dbNode = db.GraphNodes.FirstOrDefault(n => n.Id == node.Id);
            if (dbNode == null)
            {
                Logger.Information($"cannot update GraphNode {node} (not in db)");
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
                Logger.Information("cannot delete graphNode (is null)");
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
                Logger.Information($"cannot delete GraphNode {node} (does not exist)");
                return false;
            }
            Logger.Information($"deleted GraphNode {node}");
            return true;
        }

        public static bool EditPlaylistOutputNodeName(PlaylistOutputNode node, string newName)
        {
            if (node == null)
            {
                Logger.Information("cannot update PlaylistOutputNode (is null)");
                return false;
            }
            // null value is allowed (graphnode becomes invalid but it is updated in db)
            //if (string.IsNullOrEmpty(newName))
            //{
            //    Logger.Information("cannot update PlaylistOutputNode (newName is null)");
            //    return false;
            //}

            using var db = ConnectionManager.NewContext();
            var dbNode = db.PlaylistOutputNodes.FirstOrDefault(gn => gn.Id == node.Id);
            if (dbNode == null)
            {
                Logger.Information("cannot update PlaylistOutputNode (not in db)");
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
                Logger.Information("cannot update PlaylistOutputNode (is null)");
                return false;
            }
            if (string.IsNullOrEmpty(newId))
            {
                Logger.Information("cannot update PlaylistOutputNode (newId is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            var dbNode = db.PlaylistOutputNodes.FirstOrDefault(gn => gn.Id == node.Id);
            if (dbNode == null)
            {
                Logger.Information("cannot update PlaylistOutputNode (not in db)");
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
                Logger.Information("cannot swap sets of RemoveNode (is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            var dbNode = db.RemoveNodes
                .Include(gn => gn.BaseSet)
                .Include(gn => gn.RemoveSet)
                .FirstOrDefault(gn => gn.Id == node.Id);
            if (dbNode == null)
            {
                Logger.Information("cannot swap sets of RemoveNode (not in db)");
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
                Logger.Information("cannot update FilterYearNode (is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            var dbNode = db.FilterYearNodes.FirstOrDefault(gn => gn.Id == node.Id);
            if (dbNode == null)
            {
                Logger.Information("cannot update FilterYearNode (not in db)");
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
                Logger.Information("cannot update AssignTagNode (node is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            var dbNode = db.AssignTagNodes.FirstOrDefault(gn => gn.Id == node.Id);
            if (dbNode == null)
            {
                Logger.Information("cannot update AssignTagNode (not in db)");
                return false;
            }
            Tag dbTag = null;
            if (tag != null)
            {
                dbTag = db.Tags.FirstOrDefault(t => t.Id == tag.Id);
                if (dbTag == null)
                {
                    Logger.Information("cannot update AssignTagNode (tag not in db)");
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
                Logger.Information("cannot update FilterTagNode (node is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            var dbNode = db.FilterTagNodes.FirstOrDefault(gn => gn.Id == node.Id);
            if (dbNode == null)
            {
                Logger.Information("cannot update FilterTagNode (not in db)");
                return false;
            }
            Tag dbTag = null;
            if (tag != null)
            {
                dbTag = db.Tags.FirstOrDefault(t => t.Id == tag.Id);
                if (dbTag == null)
                {
                    Logger.Information("cannot update FilterTagNode (tag not in db)");
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
                Logger.Information("cannot update FilterArtistNode (node is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            var dbNode = db.FilterArtistNodes.FirstOrDefault(gn => gn.Id == node.Id);
            if (dbNode == null)
            {
                Logger.Information("cannot update FilterArtistNode (not in db)");
                return false;
            }
            Artist dbArtist = null;
            if (artist != null)
            {
                dbArtist = db.Artists.FirstOrDefault(a => a.Id == artist.Id);
                if (dbArtist == null)
                {
                    Logger.Information("cannot update FilterArtistNode (artist not in db)");
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
                Logger.Information("cannot update PlaylistInputNode (node is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            var dbNode = db.PlaylistInputNodes.FirstOrDefault(gn => gn.Id == node.Id);
            if (dbNode == null)
            {
                Logger.Information("cannot update PlaylistInputNode (not in db)");
                return false;
            }
            Playlist dbPlaylist = null;
            if (playlist != null)
            {
                dbPlaylist = db.Playlists.FirstOrDefault(a => a.Id == playlist.Id);
                if (dbPlaylist == null)
                {
                    Logger.Information("cannot update PlaylistInputNode (artist not in db)");
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
                Logger.Information("cannot add GarphNode connection (from is null)");
                return false;
            }
            if (to == null)
            {
                Logger.Information("cannot add GarphNode connection (to is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            // get from db
            var fromDb = db.GraphNodes.Include(gn => gn.Outputs).FirstOrDefault(gn => gn.Id == from.Id);
            if (fromDb == null)
            {
                Logger.Information("cannot add GraphNode connection (from not in db)");
                return false;
            }
            var toDb = db.GraphNodes.Include(gn => gn.Inputs).FirstOrDefault(gn => gn.Id == to.Id);
            if (toDb == null)
            {
                Logger.Information("cannot add GraphNode connection (to not in db)");
                return false;
            }

            if (fromDb.Outputs.Contains(toDb))
            {
                Logger.Information("cannot add GraphNode connection (already connected)");
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
                Logger.Information("cannot remove GarphNode connection (from is null)");
                return false;
            }
            if (to == null)
            {
                Logger.Information("cannot remove GarphNode connection (to is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            // get from db
            var fromDb = db.GraphNodes.Include(gn => gn.Outputs).FirstOrDefault(gn => gn.Id == from.Id);
            if (fromDb == null)
            {
                Logger.Information("cannot remove GraphNode connection (from not in db)");
                return false;
            }
            var toDb = db.GraphNodes.Include(gn => gn.Inputs).FirstOrDefault(gn => gn.Id == to.Id);
            if (toDb == null)
            {
                Logger.Information("cannot remove GraphNode connection (to not in db)");
                return false;
            }

            if (!fromDb.Outputs.Contains(toDb))
            {
                Logger.Information("cannot remove GraphNode connection (not connected)");
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
            using var db = ConnectionManager.NewContext();
            var playlists = db.Playlists;
            var generatedPlaylistIds = db.PlaylistOutputNodes.Select(p => p.GeneratedPlaylistId);
            return playlists
                .Where(p => !generatedPlaylistIds.Contains(p.Id) && !Constants.META_PLAYLIST_IDS.Contains(p.Id))
                .OrderBy(p => p.Name.ToLower())
                .ToList();
        }
        public static Playlist PlaylistsMeta(string id)
        {
            if (!Constants.META_PLAYLIST_IDS.Contains(id)) return null;
            using var db = ConnectionManager.NewContext();
            return db.Playlists.FirstOrDefault(p => p.Id == id);
        }
        public static List<Playlist> PlaylistsMeta()
        {
            using var db = ConnectionManager.NewContext();
            return db.Playlists
                .Where(p => Constants.META_PLAYLIST_IDS.Contains(p.Id))
                .OrderBy(p => p.Name)
                .ToList();
        }
        public static List<Playlist> PlaylistsGenerated()
        {
            using var db = ConnectionManager.NewContext();
            var playlistOutputNodes = db.PlaylistOutputNodes.ToList();
            return playlistOutputNodes
                .Where(node => node.GeneratedPlaylistId != null)
                .Select(node => new Playlist { Id = node.GeneratedPlaylistId, Name = node.PlaylistName })
                .OrderBy(p => p.Name).ToList();
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
            var spotifyTracks = await SpotifyOperations.PlaylistItems(playlistOutputNode.GeneratedPlaylistId);
            var spotifyTrackIds = spotifyTracks.Select(t => t.Id);

            // replace spotify track with db track
            var query = GetTrackIncludeQuery(db, includeAlbums, includeArtists, includeTags);
            return query.Where(t => spotifyTrackIds.Contains(t.Id)).ToList();
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
        public static async Task AssignTags(AssignTagNode assignTagNode)
        {
            if (assignTagNode.AnyBackward(gn => !gn.IsValid))
            {
                Logger.Information("cannot run AssignTagNode (graph contains invalid node)");
                return;
            }

            await Task.Run(() =>
            {
                assignTagNode.CalculateOutputResult();
                var tracks = assignTagNode.OutputResult;

                foreach (var track in tracks)
                    AssignTag(track, assignTagNode.Tag);
                Logger.Information($"assigned tag {assignTagNode.Tag.Name} to {tracks.Count} tracks");
            });
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
                var dbPlaylists = db.Playlists.ToDictionary(pl => pl.Id, pl => pl);
                var dbArtists = db.Artists.ToDictionary(a => a.Id, a => a);
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

        #region import/export tag
        public static async Task ExportTags(string outPath)
        {
            using var db = ConnectionManager.NewContext();
            var tracks = db.Tracks.Include(t => t.Tags).Include(t => t.Artists).Include(t => t.Album).ToList();
            // remove circular dependencies
            foreach (var t in tracks)
            {
                foreach (var artist in t.Artists)
                    artist.Tracks = null;
                foreach (var tag in t.Tags)
                    tag.Tracks = null;
            }
            var json = JsonConvert.SerializeObject(tracks, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            await File.WriteAllTextAsync(outPath, json);
            Logger.Information($"Exported {tracks.Count} tracks");
        }
        public static async Task ImportTags(string inPath, DatabaseContext db = null)
        {
            // parse data from json
            var json = await File.ReadAllTextAsync(inPath);
            List<Track> tracks;
            try
            {
                tracks = JsonConvert.DeserializeObject<List<Track>>(json);
            }
            catch (Exception e)
            {
                Logger.Error($"Error deserializing tag dump {inPath}: {e.Message}");
                return;
            }
            if (tracks == null) return;

            // merge with existing tags/tracks
            var needsDispose = false;
            if (db == null)
            {
                db = ConnectionManager.NewContext();
                needsDispose = true;
            }
            var dbTracks = db.Tracks.Include(t => t.Tags).ToList();
            var dbTags = db.Tags.ToDictionary(t => t.Id, t => t);
            var dbArtists = db.Artists.ToDictionary(a => a.Id, a => a);
            var dbAlbums = db.Albums.ToDictionary(a => a.Id, a => a);
            foreach (var track in tracks)
            {
                ReplaceTagWithDbTag(db, dbTags, track);
                ReplaceArtistWithDbArtist(db, dbArtists, track);
                ReplaceAlbumWithDbAlbum(db, dbAlbums, track);

                // get dbTrack or add track to Db
                var dbTrack = dbTracks.FirstOrDefault(t => t.Id == track.Id);
                if (dbTrack == null)
                {
                    db.Tracks.Add(track);
                    dbTrack = track;
                }
            }
            db.SaveChanges();
            Logger.Information($"Imported {tracks.Count} tracks");
            if (needsDispose)
                db.Dispose();
        }
        #endregion
    }
}
