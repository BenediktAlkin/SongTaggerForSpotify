using Backend;
using Backend.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendAPI.Controllers
{
    [ApiController]
    public class TrackController : ControllerBase
    {
        private ILogger<TrackController> Logger { get; }

        public TrackController(ILogger<TrackController> logger)
        {
            Logger = logger;
        }


        [HttpPost("tags/{tag}/tracks")]
        public async Task<bool[]> AssignTag(string tag, [FromQuery(Name = "id")] string[] ids)
        {
            using var timer = new RequestTimer<TrackController>($"Track/{nameof(AssignTag)} tag={tag} ids={string.Join(',', ids ?? Enumerable.Empty<string>())}", Logger);

            if (ids == null || ids.Length == 0)
            {
                timer.ErrorMessage = "invalid ids";
                return null;
            }


            // get track data from spotify
            var tracks = new List<Track>();
            foreach (var id in ids)
                tracks.Add(await SpotifyOperations.GetTrack(id));


            // assign tags
            var success = Util.AssignTagToTracks(tracks.ToArray(), tag);
            timer.DetailMessage = $"success={string.Join(',', success)}";

            return success;
        }
        [HttpPost("json/tags/{tag}/tracks")]
        public async Task<Dictionary<string, bool[]>> JsonAssignTag(string tag, [FromQuery(Name = "id")] string[] ids) => new() { { Constants.JSON_RESULT, await AssignTag(tag, ids) } };


        [HttpDelete("tags/{tag}/tracks")]
        public bool[] DeleteAssignment(string tag, [FromQuery(Name = "id")] string[] ids)
        {
            using var timer = new RequestTimer<TrackController>($"Track/{nameof(DeleteAssignment)} tag={tag} ids={string.Join(',', ids ?? Enumerable.Empty<string>())}", Logger);

            if (ids == null || ids.Length == 0)
            {
                timer.ErrorMessage = "invalid ids";
                return null;
            }


            // delete assignments
            var success = Util.RemoveAssignmentFromTracks(ids, tag);
            timer.DetailMessage = $"success={string.Join(',', success)}";

            return success;
        }
        [HttpDelete("json/tags/{tag}/tracks")]
        public Dictionary<string, bool[]> JsonDeleteAssignment(string tag, [FromQuery(Name = "id")] string[] ids) => new() { { Constants.JSON_RESULT, DeleteAssignment(tag, ids) } };


        [HttpGet("tags/{tag}/tracks")]
        public string IsTagged(string tag, [FromQuery(Name = "id")] string[] ids)
        {
            using var timer = new RequestTimer<TrackController>($"Track/{nameof(IsTagged)} tag={tag} ids={string.Join(',', ids ?? Enumerable.Empty<string>())}", Logger);

            if (ids == null || ids.Length == 0)
            {
                timer.ErrorMessage = "invalid ids";
                return null;
            }


            var tracksAreTagged = Util.TracksAreTagged(ids, tag);
            if (tracksAreTagged == null)
            {
                timer.ErrorMessage = "invalid tag";
                return null;
            }
            timer.DetailMessage = $"result={tracksAreTagged}";
            return tracksAreTagged;
        }
        [HttpGet("json/tags/{tag}/tracks")]
        public Dictionary<string, string> JsonIsTagged(string tag, [FromQuery(Name = "id")] string[] ids) => new() { { Constants.JSON_RESULT, IsTagged(tag, ids) } };
    }
}
