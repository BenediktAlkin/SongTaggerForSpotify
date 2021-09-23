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
                GraphGeneratorPage?.NotifyIsValidChanged();
            }
        }
        private Tag tag;
        public Tag Tag
        {
            get => tag;
            set
            {
                SetProperty(ref tag, value, nameof(Tag));
                GraphGeneratorPage?.NotifyIsValidChanged();
            }
        }

        protected override bool CanAddInput(GraphNode input) => !Inputs.Any();
        public override async Task<List<Track>> GetResult()
        {
            var tracks = await GetInput();
            return tracks.Where(t => t.Tags.Contains(Tag)).ToList();
        }

        public override bool IsValid => TagId != null || Tag != null;
        public override bool RequiresTags => true;
    }
}
