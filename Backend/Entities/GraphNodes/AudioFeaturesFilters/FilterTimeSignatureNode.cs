using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes.AudioFeaturesFilters
{
    public class FilterTimeSignatureNode : GraphNode
    {
        private int? timeSignature;
        public int? TimeSignature
        {
            get => timeSignature;
            set
            {
                if (value == timeSignature) return;
                SetProperty(ref timeSignature, value, nameof(TimeSignature));
                OutputResult = null;
                PropagateForward(gn => gn.ClearResult(), applyToSelf: false);
            }
        }

        private List<int> validTimeSignatures;
        [NotMapped]
        public List<int> ValidTimeSignatures
        {
            get => validTimeSignatures;
            private set => SetProperty(ref validTimeSignatures, value, nameof(ValidTimeSignatures));
        }

        protected override void OnInputResultChanged()
        {
            if (InputResult == null || InputResult.Count == 0)
                ValidTimeSignatures = null;
            else
                ValidTimeSignatures = InputResult[0].Select(track => track.AudioFeatures.TimeSignature).Distinct().OrderBy(ts => ts).ToList();
            Logger.Information($"OnInputResultChanged for {this} (ValidTimeSignaturesCount={ValidTimeSignatures?.Count})");
        }

        protected override bool CanAddInput(GraphNode input) => !Inputs.Any();
        protected override void MapInputToOutput()
            => OutputResult = InputResult[0].Where(t => t.AudioFeatures.TimeSignature == TimeSignature.Value).ToList();

        public override bool IsValid => TimeSignature != null;
        public override bool RequiresAudioFeatures => true;
    }
}