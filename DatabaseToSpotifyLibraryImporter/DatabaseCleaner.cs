using Backend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseToSpotifyLibraryImporter
{
    public static class DatabaseCleaner
    {
        public static void Clean()
        {
            // remove generated playlist ids (to trigger creation of new playlists when running a generator)
            // else the existing playlist would be overwritten
            using var db = ConnectionManager.NewContext();
            var playlistOutputNodes = db.PlaylistOutputNodes.ToList();
            foreach (var node in playlistOutputNodes)
                node.GeneratedPlaylistId = null;
            db.SaveChanges();
        }
    }
}
