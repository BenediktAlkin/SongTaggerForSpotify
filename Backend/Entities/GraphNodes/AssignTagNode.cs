using System.Linq;

namespace Backend.Entities.GraphNodes
{
    public class AssignTagNode : GraphNode
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
        protected override bool CanAddOutput(GraphNode output) => false;

        public override bool IsValid => TagId != null || Tag != null;
        public override bool RequiresTags => true;
    }
}
