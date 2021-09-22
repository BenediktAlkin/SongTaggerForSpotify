using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes
{
    public class TagFilterNode : GraphNode
    {
        private int? tagId;
        public int? TagId
        {
            get => tagId;
            set
            {
                SetProperty(ref tagId, value, nameof(TagId));
                GraphGeneratorPage.NotifyIsValidChanged();
            }
        }
        private Tag tag;
        public Tag Tag
        {
            get => tag;
            set
            {
                SetProperty(ref tag, value, nameof(Tag));
                GraphGeneratorPage.NotifyIsValidChanged();
            }
        }

        protected override bool CanAddInput(GraphNode input) => Inputs.Count() < 1;
        public override async Task<List<Track>> GetResult()
        {
            if (Inputs == null || Inputs.Count() == 0)
                return new List<Track>();

            var tracks = await Inputs.First().GetResult();

            // populate tracks with tags
            var dbTracks = await ConnectionManager.Instance.Database.Tracks.Include(t => t.Tags).ToDictionaryAsync(t => t.Id, t => t);
            foreach (var track in tracks)
            {
                if (dbTracks.TryGetValue(track.Id, out var dbTrack))
                    track.Tags = dbTrack.Tags;
            }

            // filter
            //return tracks.Where(t => t.Tags.Select(tag => tag.Name).Contains(Tag.Name)).ToList();
            return tracks.Where(t => t.Tags.Contains(Tag)).ToList();
        }
        public override bool IsValid => TagId != null || Tag != null;
    }
}
