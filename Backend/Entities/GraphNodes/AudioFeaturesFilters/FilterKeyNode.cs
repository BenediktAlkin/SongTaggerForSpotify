using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes.AudioFeaturesFilters
{
    public class FilterKeyNode : GraphNode
    {
        private int? key;
        public int? Key
        {
            get => key;
            set
            {
                if (value == key) return;
                SetProperty(ref key, value, nameof(Key));
                OutputResult = null;
                PropagateForward(gn => gn.ClearResult(), applyToSelf: false);
            }
        }

        private List<int> validKeys;
        [NotMapped]
        public List<int> ValidKeys
        {
            get => validKeys;
            private set => SetProperty(ref validKeys, value, nameof(ValidKeys));
        }

        protected override void OnInputResultChanged()
        {
            if (InputResult == null || InputResult.Count == 0)
                ValidKeys = null;
            else
                ValidKeys = InputResult[0].Select(track => track.AudioFeatures.Key).Distinct().OrderBy(key => key).ToList();
            Logger.Information($"OnInputResultChanged for {this} (ValidKeysCount={ValidKeys?.Count})");
        }

        protected override bool CanAddInput(GraphNode input) => !Inputs.Any();
        protected override void MapInputToOutput()
            => OutputResult = InputResult[0].Where(t => t.AudioFeatures.Key == Key.Value).ToList();

        public override bool IsValid => Key != null;
        public override bool RequiresAudioFeatures => true;
    }
}
