using Serilog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

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
            if (InputResult == null || InputResult.Count == 0)
                ValidTags = null;
            else
                ValidTags = InputResult[0].SelectMany(track => track.Tags).Distinct().OrderBy(tag => tag.Name).ToList();
            Logger.Information($"OnInputResultChanged for {this} (validTagsCount={ValidTags?.Count})");
        }

        protected override bool CanAddInput(GraphNode input) => !Inputs.Any();
        protected override void MapInputToOutput() 
            => OutputResult = InputResult[0].Where(t => t.Tags.Contains(Tag)).ToList();

        public override bool IsValid => TagId != null || Tag != null;
        public override bool RequiresTags => true;
    }
}
