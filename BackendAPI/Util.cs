using Backend;
using Backend.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BackendAPI
{
    public static class Util
    {
        public static bool[] AssignTagToTracks(Track[] tracks, string tagName)
        {
            var success = new bool[tracks.Length];
            for (var i = 0; i < tracks.Length; i++)
            {
                // check if track is valid (fetching from spotify might fail e.g. with invalid id)
                if (tracks[i] == null)
                {
                    success[i] = false;
                    continue;
                }
                // make sure track is in db
                DatabaseOperations.AddTrack(tracks[i]);

                // assign tags
                success[i] = DatabaseOperations.AssignTag(tracks[i].Id, tagName);
            }
            return success;
        }

        public static bool[] RemoveAssignmentFromTracks(string[] trackIds, string tagName)
        {
            var success = new bool[trackIds.Length];
            for (var i = 0; i < trackIds.Length; i++)
            {
                // remove assignment
                success[i] = DatabaseOperations.DeleteAssignment(trackIds[i], tagName);
            }
            return success;
        }

        public static string TracksAreTagged(string[] trackIds, string tagName)
        {
            using var db = ConnectionManager.NewContext();
            // get tag from db
            var dbTag = db.Tags.FirstOrDefault(t => t.Name == tagName);
            if (dbTag == null)
                return null;


            // check if tracks have tag
            var taggedCount = 0;
            for (var i = 0; i < trackIds.Length; i++)
            {
                var dbTrack = db.Tracks.Include(t => t.Tags).FirstOrDefault(t => t.Id == trackIds[i]);

                if (dbTrack == null || !dbTrack.Tags.Contains(dbTag))
                {
                    // track is not tagged --> if some other track was already tagged --> SOME
                    if (taggedCount > 0)
                        break;
                }
                else
                    taggedCount++;
            }


            if (taggedCount == 0)
                return Constants.NONE;
            if (taggedCount == trackIds.Length)
                return Constants.ALL;
            return Constants.SOME;
        }
    }
}
