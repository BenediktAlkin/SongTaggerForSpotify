using Serilog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes
{
    public class FilterTagNode : GraphNode
    {
        private int? tagId;
        public int? TagId
        {
            get => tagId;
            set
            {
                SetProperty(ref tagId, value, nameof(TagId));
                GraphGeneratorPage?.NotifyIsValidChanged();
                OutputResult = null;
                PropagateForward(gn => gn.ClearResult(), applyToSelf: false);
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
                OutputResult = null;
                PropagateForward(gn => gn.ClearResult(), applyToSelf: false);
            }
        }

        private List<Tag> validTags;
        [NotMapped]
        public List<Tag> ValidTags
        {
            get => validTags;
            private set => SetProperty(ref validTags, value, nameof(ValidTags));
        }
        protected override void OnInputResultChanged()
        {
            if (InputResult == null)
                ValidTags = null;
            else
                ValidTags = InputResult.SelectMany(track => track.Tags).Distinct().OrderBy(tag => tag.Name).ToList();
            Log.Information($"OnInputResultChanged for {this} (validTagsCount={ValidTags?.Count})");
        }

        protected override bool CanAddInput(GraphNode input) => !Inputs.Any();
        protected override Task MapInputToOutput()
        {
            if (Tag != null)
                OutputResult = InputResult.Where(t => t.Tags.Contains(Tag)).ToList();
            return Task.CompletedTask;
        }

        public override bool IsValid => TagId != null || Tag != null;
        public override bool RequiresTags => true;
    }
}
