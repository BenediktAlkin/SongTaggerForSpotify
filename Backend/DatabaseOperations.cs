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


        #region tag
        public static List<Tag> GetTags()
        {
            return Db.Tags.ToList();
        }
        public static bool TagExists(string tagName)
        {
            tagName = tagName.ToLower();
            return Db.Tags.FirstOrDefault(t => t.Name == tagName) != null;
        }
        public static bool IsValidTag(string tagName)
        {
            if (string.IsNullOrEmpty(tagName))
                return true;

            return TagExists(tagName);
        }
        public static bool CanAddTag(string tagName)
        {
            if (IsValidTag(tagName))
                return false;
            return true;
        }
        public static void AddTag(string tagName)
        {
            if (!CanAddTag(tagName)) return;

            tagName = tagName.ToLower();
            var tag = new Tag { Name = tagName };
            Db.Tags.Add(tag);
            Db.SaveChanges();
            DataContainer.Instance.Tags.Add(tag);
        }
        public static bool CanEditTag(Tag tag, string newName)
        {
            if (IsValidTag(newName))
                return false;
            return true;
        }
        public static void EditTag(Tag tag, string newName)
        {
            if (!CanEditTag(tag, newName)) return;

            tag.Name = newName;
            Db.SaveChanges();
        }
        public static void DeleteTag(Tag tag)
        {
            Db.Tags.Remove(tag);
            Db.SaveChanges();
            DataContainer.Instance.Tags.Remove(tag);
        }
        public static bool AssignTag(Track track, string tagName)
        {
            // avoid duplicates
            if (track.Tags.Select(t => t.Name).Contains(tagName))
                return false;

            // check if tag exists in db
            var dbTag = Db.Tags.FirstOrDefault(t => t.Name == tagName.ToLower());
            if (dbTag == null)
                return false;

            track.Tags.Add(dbTag);
            Db.SaveChanges();
            return true;
        }
        public static async Task AssignTags(AssignTagNode assignTagNode)
        {
            if (assignTagNode.AnyBackward(gn => !gn.IsValid)) return;

            assignTagNode.CalculateOutputResult();
            var tracks = assignTagNode.OutputResult;
            foreach (var track in tracks)
                track.Tags.Add(assignTagNode.Tag);
            await Db.SaveChangesAsync();
        }
        public static bool RemoveAssignment(Track track, Tag tag)
        {
            if (track == null)
                return false;

            track.Tags.Remove(tag);
            Db.SaveChanges();
            return true;
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
        public static async Task SyncLibrary(CancellationToken cancellationToken = default)
        {
            Log.Information("Syncing library");
            // exclude generated playlists from library
            var playlistOutputNodes = await ConnectionManager.Instance.Database.PlaylistOutputNodes.ToListAsync(cancellationToken);
            var generatedPlaylistIds = playlistOutputNodes.Select(pl => pl.GeneratedPlaylistId).ToList();

            // start retrieving all tracks from db
            var dbTracksTask = Db.Tracks.Include(t => t.Tags).Include(t => t.Playlists).ToListAsync(cancellationToken);

            // get full library from spotify
            var (spotifyPlaylists, spotifyTracks) = await SpotifyOperations.GetFullLibrary(generatedPlaylistIds);
            if (spotifyPlaylists == null && spotifyTracks == null)
            {
                Log.Error($"Error syncing library: could not retrieve Spotify library");
                return;
            }

            // get db data
            var dbTracks = (await dbTracksTask).ToDictionary(t => t.Id, t => t);
            var dbPlaylists = (await Db.Playlists.ToListAsync(cancellationToken)).ToDictionary(pl => pl.Id, pl => pl);
            var dbArtists = (await Db.Artists.ToListAsync(cancellationToken)).ToDictionary(a => a.Id, a => a);
            var dbAlbums = (await Db.Albums.ToListAsync(cancellationToken)).ToDictionary(a => a.Id, a => a);


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
            await Db.SaveChangesAsync(cancellationToken);
            Log.Information("Finished removing untracked tracks from db");

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
            }
            await Db.SaveChangesAsync(cancellationToken);
            Log.Information("Finished pushing library to database");

            // remove unlikedplaylists
            Log.Information("Update remove unliked playlists");
            var allPlaylists = await Db.Playlists.Include(p => p.Tracks).ToListAsync(cancellationToken);
            var spotifyPlaylistIds = spotifyPlaylists.Select(p => p.Id);
            var playlistsToRemove = allPlaylists.Where(p => !spotifyPlaylistIds.Contains(p.Id) && !Constants.META_PLAYLIST_IDS.Contains(p.Id));
            foreach(var playlist in playlistsToRemove)
            {
                Log.Information($"Removing playlist {playlist.Name} (unliked)");
                Db.Playlists.Remove(playlist);
            }
            await Db.SaveChangesAsync(cancellationToken);
            Log.Information("Finished removing unliked playlists");
            Log.Information("Update liked playlist names");

            // update playlist names
            var allPlaylistsDict = allPlaylists.ToDictionary(p => p.Id, p => p);
            foreach (var spotifyPlaylist in spotifyPlaylists)
                allPlaylistsDict[spotifyPlaylist.Id].Name = spotifyPlaylist.Name;
            await Db.SaveChangesAsync(cancellationToken);
            Log.Information("Finished updating liked playlist names");

            await DataContainer.Instance.LoadSourcePlaylists();
        }
        #endregion


        #region get playlists
        public static List<Playlist> PlaylistsLiked()
        {
            var playlists = Db.Playlists;
            var generatedPlaylists = Db.PlaylistOutputNodes.Select(p => p.PlaylistName);
            return playlists.Where(p => !generatedPlaylists.Contains(p.Name) && !Constants.META_PLAYLIST_IDS.Contains(p.Id))
                .OrderBy(p => p.Name).ToList();
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
            var query = GetTrackIncludeQuery(includeAlbums, includeArtists, includeTags);
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
            var query = GetTrackIncludeQuery(includeAlbums, includeArtists, includeTags);
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
            var query = GetTrackIncludeQuery(includeAlbums, includeArtists, includeTags);
            return query.Where(t => spotifyTrackIds.Contains(t.Id)).ToList();
        }
        private static IQueryable<Track> GetTrackIncludeQuery(bool includeAlbums, bool includeArtists, bool includeTags)
        {
            IQueryable<Track> query = Db.Tracks
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
