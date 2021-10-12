using Backend.Entities;
using Backend.Entities.GraphNodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Backend
{
    public static class DatabaseOperations
    {
        private static DatabaseContext Db => ConnectionManager.Instance.Database;
        private static ILogger Logger = Log.ForContext("SourceContext", "DB");

        #region tag
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
            if(db == null)
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
        private static bool IsValidTag(string tagName, DatabaseContext db)
        {
            if (string.IsNullOrEmpty(tagName))
            {
                Logger.Information($"invalid tag {tagName} (null or empty)");
                return false;
            }

            var tagExists = TagExists(tagName, db);
            if (tagExists)
            {
                Logger.Information($"invalid tag {tagName} (already exists)");
                return false;
            }
            return true;
        }
        public static bool CanAddTag(string tagName, DatabaseContext db = null)
        {
            if (!IsValidTag(tagName, db))
                return false;
                
            return true;
        }
        public static bool AddTag(string tagName)
        {
            using var db = ConnectionManager.NewContext();
            if (!CanAddTag(tagName, db)) return false;

            tagName = tagName.ToLower();
            var tag = new Tag { Name = tagName };

            db.Tags.Add(tag);
            db.SaveChanges();
            Logger.Information($"added tag {tagName}");
            DataContainer.Instance.Tags?.Add(tag);
            return true;
        }
        public static bool CanEditTag(Tag tag, string newName, DatabaseContext db = null)
        {
            if (!IsValidTag(newName, db))
                return false;
            return true;
        }
        public static bool EditTag(Tag tag, string newName)
        {
            using var db = ConnectionManager.NewContext();
            if (!CanEditTag(tag, newName, db)) return false;

            var oldName = tag.Name;
            tag.Name = newName;
            db.Update(tag);
            db.SaveChanges();
            Logger.Information($"updated tagname old={oldName} new={newName}");
            return true;
        }
        public static bool DeleteTag(Tag tag)
        {
            if (tag == null)
            {
                Logger.Information($"could not delete tag (is null)");
                return false;
            }

            using var db = ConnectionManager.NewContext();
            try
            {
                db.Tags.Remove(tag);
                db.SaveChanges();
            }
            catch(Exception)
            {
                Logger.Information($"could not delete tag {tag.Name} (does not exist)");
                return false;
            }
            Logger.Information($"deleted tag {tag.Name}");
            DataContainer.Instance.Tags?.Remove(tag);
            return true;
        }
        #endregion

        #region tracks
        public static bool AssignTag(Track track, Tag tag)
        {
            if(track == null)
            {
                Logger.Information($"cannot assign tag {tag?.Name} to track null");
                return false;
            }
            if(tag == null)
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
            }catch(Exception e)
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
            if(track == null)
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
            return Db.GraphGeneratorPages
                .Include(ggp => ggp.GraphNodes)
                .ThenInclude(gn => gn.Outputs)
                .ToList();
        }
        public static void AddGraphGeneratorPage(GraphGeneratorPage page)
        {
            Db.GraphGeneratorPages.Add(page);
            Db.SaveChanges();
        }
        public static void DeleteGraphGeneratorPage(GraphGeneratorPage page)
        {
            Db.GraphGeneratorPages.Remove(page);
            Db.SaveChanges();
        }
        public static void EditGraphGeneratorPage(GraphGeneratorPage page, string newName)
        {
            page.Name = newName;
            Db.SaveChanges();
        }
        #endregion

        #region graphNodes
        public static void DeleteGraphNode(GraphNode node)
        {
            Db.GraphNodes.Remove(node);
            Db.SaveChanges();
        }
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

                using var db = ConnectionManager.NewContext();
                foreach (var track in tracks)
                {
                    if (!track.Tags.Contains(assignTagNode.Tag))
                    {
                        track.Tags.Add(assignTagNode.Tag);
                        db.Update(track);
                    }
                }

                Db.SaveChanges();
                Logger.Information($"assigned tag {assignTagNode.Tag.Name} to {tracks.Count} tracks");
            });
        }
        #endregion

        #region sync library
        private static void ReplacePlaylistsWithDbPlaylists(Dictionary<string, Playlist> playlistDict, Track track)
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
                    Db.Playlists.Add(playlist);
                }
            }
        }
        private static void ReplaceArtistWithDbArtist(Dictionary<string, Artist> artistDict, Track track)
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
                    Db.Artists.Add(artist);
                }
            }
        }
        private static void ReplaceAlbumWithDbAlbum(Dictionary<string, Album> albumDict, Track track)
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
                Db.Albums.Add(track.Album);
            }
        }
        private static void ReplaceTagWithDbTag(Dictionary<int, Tag> tagDict, Track track)
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
                    Db.Tags.Add(tag);
                }
            }
        }

        private static Task SyncLibraryTask { get; set; }
        public static async Task SyncLibrary(CancellationToken cancellationToken = default)
        {
            // wait for old task to finish (if it is cancelled)
            if(SyncLibraryTask != null && !SyncLibraryTask.IsCompleted)
            {
                Log.Information($"Waiting for currently running SyncLibrary to finish");
                try
                {
                    await SyncLibraryTask;
                }
                catch (OperationCanceledException e) 
                {
                    Log.Information($"Error waiting for currently running SyncLibrary to finish {e.Message}");
                }
                
                Log.Information($"Finished waiting for currently running SyncLibrary to finish");
            }
                

            SyncLibraryTask = Task.Run(async () =>
            {
                Log.Information("Syncing library");
                // exclude generated playlists from library
                var playlistOutputNodes = Db.PlaylistOutputNodes.ToList();
                var generatedPlaylistIds = playlistOutputNodes.Select(pl => pl.GeneratedPlaylistId).ToList();
                cancellationToken.ThrowIfCancellationRequested();

                // start fetching spotify library
                var getSpotifyLibraryTask = SpotifyOperations.GetFullLibrary(generatedPlaylistIds);

                // get db data
                var dbTracks = Db.Tracks.Include(t => t.Tags).Include(t => t.Playlists).ToDictionary(t => t.Id, t => t);
                var dbPlaylists = Db.Playlists.ToDictionary(pl => pl.Id, pl => pl);
                var dbArtists = Db.Artists.ToDictionary(a => a.Id, a => a);
                var dbAlbums = Db.Albums.ToDictionary(a => a.Id, a => a);
                cancellationToken.ThrowIfCancellationRequested();

                // await fetching spotify library
                var (spotifyPlaylists, spotifyTracks) = await getSpotifyLibraryTask;
                if (spotifyPlaylists == null && spotifyTracks == null)
                {
                    Log.Error($"Error syncing library: could not retrieve Spotify library");
                    return;
                }
                cancellationToken.ThrowIfCancellationRequested();

                // remove tracks that are no longer in the library and are not tagged
                Log.Information("Start removing untracked tracks from db");
                var nonTaggedTracks = dbTracks.Values.Where(t => t.Tags.Count == 0);
                foreach (var nonTaggedTrack in nonTaggedTracks)
                {
                    if (!spotifyTracks.ContainsKey(nonTaggedTrack.Id))
                    {
                        Db.Tracks.Remove(nonTaggedTrack);
                        Log.Information($"Removed {nonTaggedTrack.Name} - {nonTaggedTrack.ArtistsString} from db (no longer in library & untagged)");
                    }
                }
                Db.SaveChanges();
                Log.Information("Finished removing untracked tracks from db");
                cancellationToken.ThrowIfCancellationRequested();

                // push spotify library to db
                Log.Information("Start pushing library to database");
                foreach (var track in spotifyTracks.Values)
                {
                    // replace spotify playlist objects with db playlist objects
                    ReplacePlaylistsWithDbPlaylists(dbPlaylists, track);
                    if (dbTracks.TryGetValue(track.Id, out var dbTrack))
                    {
                        // update the playlist sources in case the song has been added/removed from a playlist
                        dbTrack.Playlists = track.Playlists;
                    }
                    else
                    {
                        Log.Information($"Adding {track.Name} - {track.ArtistsString} to db");
                        // replace spotify album/artist objects with db album/artist objects
                        ReplaceAlbumWithDbAlbum(dbAlbums, track);
                        ReplaceArtistWithDbArtist(dbArtists, track);
                        // add track to db
                        Db.Tracks.Add(track);
                    }
                    cancellationToken.ThrowIfCancellationRequested();
                }
                Db.SaveChanges();
                Log.Information("Finished pushing library to database");
                cancellationToken.ThrowIfCancellationRequested();

                // remove unlikedplaylists
                Log.Information("Update remove unliked playlists");
                var allPlaylists = Db.Playlists.Include(p => p.Tracks).ToList();
                var spotifyPlaylistIds = spotifyPlaylists.Select(p => p.Id);
                var playlistsToRemove = allPlaylists.Where(p => !spotifyPlaylistIds.Contains(p.Id) && !Constants.META_PLAYLIST_IDS.Contains(p.Id));
                foreach (var playlist in playlistsToRemove)
                {
                    Log.Information($"Removing playlist {playlist.Name} (unliked)");
                    Db.Playlists.Remove(playlist);
                }
                Db.SaveChanges();
                Log.Information("Finished removing unliked playlists");
                cancellationToken.ThrowIfCancellationRequested();

                // update playlist names
                Log.Information("Update liked playlist names");
                var allPlaylistsDict = allPlaylists.ToDictionary(p => p.Id, p => p);
                foreach (var spotifyPlaylist in spotifyPlaylists)
                    allPlaylistsDict[spotifyPlaylist.Id].Name = spotifyPlaylist.Name;
                Db.SaveChanges();
                Log.Information("Finished updating liked playlist names");
            }, cancellationToken);
            await SyncLibraryTask;

            await DataContainer.Instance.LoadSourcePlaylists(forceReload: true);
        }
        #endregion


        #region get playlists
        public static List<Playlist> PlaylistsLiked()
        {
            var playlists = Db.Playlists;
            var generatedPlaylistIds = Db.PlaylistOutputNodes.Select(p => p.GeneratedPlaylistId);
            return playlists.Where(p => !generatedPlaylistIds.Contains(p.Id) && !Constants.META_PLAYLIST_IDS.Contains(p.Id))
                .OrderBy(p => p.Name.ToLower()).ToList();
        }
        public static List<Playlist> PlaylistsMeta() => Db.Playlists.Where(p => Constants.META_PLAYLIST_IDS.Contains(p.Id)).OrderBy(p => p.Name).ToList();
        public static List<Playlist> PlaylistsGenerated()
        {
            var playlistOutputNodes = Db.PlaylistOutputNodes.ToList();
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
                Constants.LIKED_SONGS_PLAYLIST_ID => query.Where(t => t.Playlists.Select(p => p.Id).Contains(playlistId)).ToList(),
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
            var playlistOutputNode = Db.PlaylistOutputNodes.FirstOrDefault(p => p.GeneratedPlaylistId == id);
            if (playlistOutputNode == null)
            {
                Log.Error($"Could not find PlaylistOutputNode with GeneratedPlaylistId {id}");
                return new();
            }
            var spotifyTracks = await SpotifyOperations.PlaylistItems(playlistOutputNode.GeneratedPlaylistId);
            var spotifyTrackIds = spotifyTracks.Select(t => t.Id);

            // replace spotify track with db track
            using var db = ConnectionManager.NewContext();
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


        #region import/export tag
        public static async Task ExportTags(string outPath)
        {
            var tracks = Db.Tracks.Include(t => t.Tags).Include(t => t.Artists).Include(t => t.Album).ToList();
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
            Log.Information($"Exported {tracks.Count} tracks");
        }
        public static async Task ImportTags(string inPath)
        {
            // parse data from json
            var json = await File.ReadAllTextAsync(inPath);
            List<Track> tracks;
            try
            {
                tracks = JsonConvert.DeserializeObject<List<Track>>(json);
            }catch(Exception e)
            {
                Log.Error($"Error deserializing tag dump {inPath}: {e.Message}");
                return;
            }
            if (tracks == null) return;

            // merge with existing tags/tracks
            var dbTracks = Db.Tracks.Include(t => t.Tags).ToList();
            var dbTags = Db.Tags.ToDictionary(t => t.Id, t => t);
            var dbArtists = Db.Artists.ToDictionary(a => a.Id, a => a);
            var dbAlbums = Db.Albums.ToDictionary(a => a.Id, a => a);
            foreach (var track in tracks)
            {
                ReplaceTagWithDbTag(dbTags, track);
                ReplaceArtistWithDbArtist(dbArtists, track);
                ReplaceAlbumWithDbAlbum(dbAlbums, track);

                // get dbTrack or add track to Db
                var dbTrack = dbTracks.FirstOrDefault(t => t.Id == track.Id);
                if (dbTrack == null)
                {
                    Db.Tracks.Add(track);
                    dbTrack = track;
                }
            }
            Db.SaveChanges();
            Log.Information($"Imported {tracks.Count} tracks");
        }
        #endregion
    }
}
