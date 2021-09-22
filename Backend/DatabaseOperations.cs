using Backend.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Backend
{
    public static class DatabaseOperations
    {
        private static DatabaseContext Db => ConnectionManager.Instance.Database;

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
        public static bool RemoveAssignment(Track track, Tag tag)
        {
            if (track == null)
                return false;

            track.Tags.Remove(tag);
            Db.SaveChanges();
            return true;
        }

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
                    // add artist to the dbContext
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

        public static async Task SyncLibrary(CancellationToken cancellationToken=default)
        {
            Log.Information("Syncing library");
            // exclude generated playlists from library
            await DataContainer.Instance.LoadOutputNodes();
            var generatedPlaylistIds = DataContainer.Instance.OutputNodes.Select(pl => pl.GeneratedPlaylistId).ToList();

            // start retrieving all tracks from db
            var dbTracksTask = Db.Tracks.Include(t => t.Tags).Include(t => t.Playlists).ToListAsync(cancellationToken);

            // get full library from spotify
            var tracks = await SpotifyOperations.GetFullLibrary(generatedPlaylistIds);

            // get db data
            var dbTracks = (await dbTracksTask).ToDictionary(t => t.Id, t => t);
            var dbPlaylists = (await Db.Playlists.ToListAsync(cancellationToken)).ToDictionary(pl => pl.Id, pl => pl);
            var dbArtists = (await Db.Artists.ToListAsync(cancellationToken)).ToDictionary(a => a.Id, a => a);
            var dbAlbums = (await Db.Albums.ToListAsync(cancellationToken)).ToDictionary(a => a.Id, a => a);

            // replace duplicate objects within data
            Log.Information("Start removing duplicated objects within library");
            foreach(var track in tracks.Values)
            {
                ReplaceAlbumWithDbAlbum(dbAlbums, track);
                ReplaceArtistWithDbArtist(dbArtists, track);
                ReplacePlaylistsWithDbPlaylists(dbPlaylists, track);
            }
            Log.Information("Finished removing duplicated objects within library");

            // remove tracks that are no longer in the library and are not tagged
            Log.Information("Start removing untracked tracks from db");
            var nonTaggedTracks = dbTracks.Values.Where(t => t.Tags.Count == 0);
            foreach (var nonTaggedTrack in nonTaggedTracks)
            {
                if (!tracks.ContainsKey(nonTaggedTrack.Id))
                    Db.Tracks.Remove(nonTaggedTrack);
            }
            Log.Information("Finished removing untracked tracks from db");

            // push spotify library to db
            Log.Information("Start pushing library to database");
            foreach (var track in tracks.Values)
            {
                if(dbTracks.TryGetValue(track.Id, out var dbTrack))
                {
                    // update the playlist sources in case the song has been added/removed from a playlist
                    dbTrack.Playlists = track.Playlists;
                }
                else
                {
                    // add track to db
                    Db.Tracks.Add(track);
                }
            }

            await Db.SaveChangesAsync(cancellationToken);
            Log.Information("Finished pushing library to database");

            // remove unfollowed playlists
            Log.Information("Remove old playlists from database");
            var allPlaylists = await Db.Playlists.ToListAsync();
            Db.Playlists.RemoveRange(allPlaylists.Where(p => p.Tracks == null || p.Tracks.Count == 0));
            await Db.SaveChangesAsync(cancellationToken);
            Log.Information("Finished removing old playlists from database");

            await DataContainer.Instance.LoadSourcePlaylists(forceReload: true);
        }

        public static async Task<List<Playlist>> SourcePlaylistCurrentUsers()
        {
            var playlists = Db.Playlists;
            var generatedPlaylists = Db.OutputNodes.Select(p => p.PlaylistName);
            return await playlists.Where(p => !generatedPlaylists.Contains(p.Name)).ToListAsync();
        }
    }
}
