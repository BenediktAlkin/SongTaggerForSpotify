using Backend;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace BackendAPI.Controllers
{
    [ApiController]
    public class TagController : ControllerBase
    {
        private ILogger<TagController> Logger { get; }

        public TagController(ILogger<TagController> logger)
        {
            Logger = logger;
        }

        [HttpGet("tags")]
        public string[] GetTags()
        {
            using var timer = new RequestTimer<TagController>($"Tag/{nameof(GetTags)}", Logger);

            using var db = ConnectionManager.NewContext();
            var allTags = db.Tags.Select(t => t.Name).ToArray();

            timer.DetailMessage = $"result={string.Join(',', allTags)}";
            return allTags;
        }
        [HttpGet("json/tags")]
        public Dictionary<string, string[]> JsonGetTags() => new() { { Constants.JSON_RESULT, GetTags() } };

        [HttpGet("taggroups")]
        public Dictionary<string, string[]> GetTagGroups()
        {
            using var timer = new RequestTimer<TagController>($"Tag/{nameof(GetTagGroups)}", Logger);

            using var db = ConnectionManager.NewContext();

            var tagGroups = DatabaseOperations.GetTagGroups();
            var groups = tagGroups.ToDictionary(tg => tg.Name, tg => tg.Tags.Select(t => t.Name).ToArray());
            
            var msg = string.Join(',', groups.Select(g => $"{g.Key}:[{string.Join(',', groups[g.Key])}]"));
            timer.DetailMessage = $"result={{{msg}}}";
            return groups;
        }
        [HttpGet("json/taggroups")]
        public Dictionary<string, Dictionary<string, string[]>> JsonGetTagGroups() => new() { { Constants.JSON_RESULT, GetTagGroups() } };
    }
}
