using Backend;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendAPI.Controllers
{
    [ApiController]
    public class AlbumController : ControllerBase
    {
        private ILogger<AlbumController> Logger { get; }

        public AlbumController(ILogger<AlbumController> logger)
        {
            Logger = logger;
        }


        [HttpPost("tags/{tag}/album")]
        public async Task<bool[]> AssignTag(string tag, [FromQuery] string id)
        {
            using var timer = new RequestTimer<AlbumController>($"Album/{nameof(AssignTag)} tag={tag} id={id}", Logger);

            if (id == null)
            {
                timer.ErrorMessage = "invalid id";
                return null;
            }


            // get album tracks from spotify
            var tracks = await SpotifyOperations.GetAlbumTracks(id);
            if (tracks == null || tracks.Count == 0)
            {
                timer.ErrorMessage = "invalid id";
                return null;
            }


            // assign tags
            var success = Util.AssignTagToTracks(tracks.ToArray(), tag);
            timer.DetailMessage = $"success={string.Join(',', success)}";

            return success;
        }

        [HttpPost("json/tags/{tag}/album")]
        public async Task<Dictionary<string, bool[]>> JsonAssignTag(string tag, [FromQuery] string id) => new() { { Constants.JSON_RESULT, await AssignTag(tag, id) } };


        [HttpDelete("tags/{tag}/album")]
        public async Task<bool[]> DeleteAssignment(string tag, [FromQuery] string id)
        {
            using var timer = new RequestTimer<AlbumController>($"Album/{nameof(DeleteAssignment)} tag={tag} id={id}", Logger);

            if (id == null)
            {
                timer.ErrorMessage = "invalid id";
                return null;
            }

            // get album tracks from spotify
            var tracks = await SpotifyOperations.GetAlbumTracks(id);
            if (tracks == null || tracks.Count == 0)
            {
                timer.ErrorMessage = "invalid id";
                return null;
            }


            // delete assignments
            var success = Util.RemoveAssignmentFromTracks(tracks.Select(t => t.Id).ToArray(), tag);
            timer.DetailMessage = $"success={string.Join(',', success)}";

            return success;
        }
        [HttpDelete("json/tags/{tag}/album")]
        public async Task<Dictionary<string, bool[]>> JsonDeleteAssignment(string tag, [FromQuery] string id) => new() { { Constants.JSON_RESULT, await DeleteAssignment(tag, id) } };


        [HttpGet("tags/{tag}/album")]
        public async Task<string> IsTagged(string tag, [FromQuery] string id)
        {
            using var timer = new RequestTimer<AlbumController>($"Album/{nameof(IsTagged)} tag={tag} id={id}", Logger);

            if (id == null)
            {
                timer.ErrorMessage = "invalid id";
                return null;
            }

            // get album tracks from spotify
            var tracks = await SpotifyOperations.GetAlbumTracks(id);
            if (tracks == null || tracks.Count == 0)
            {
                timer.ErrorMessage = "invalid id";
                return null;
            }


            var tracksAreTagged = Util.TracksAreTagged(tracks.Select(t => t.Id).ToArray(), tag);
            if (tracksAreTagged == null)
            {
                timer.ErrorMessage = "invalid tag";
                return null;
            }
            timer.DetailMessage = $"result={tracksAreTagged}";
            return tracksAreTagged;
        }

        [HttpGet("json/tags/{tag}/album")]
        public async Task<Dictionary<string, string>> JsonIsTagged(string tag, [FromQuery] string id) => new() { { Constants.JSON_RESULT, await IsTagged(tag, id) } };
    }
}
