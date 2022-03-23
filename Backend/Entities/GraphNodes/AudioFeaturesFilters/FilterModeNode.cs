using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes.AudioFeaturesFilters
{
    public class FilterModeNode : GraphNode
    {
        private int? mode;
        public int? Mode
        {
            get => mode;
            set
            {
                if (value == mode) return;
                SetProperty(ref mode, value, nameof(Mode));
                OutputResult = null;
                PropagateForward(gn => gn.ClearResult(), applyToSelf: false);
            }
        }

        private List<int> validModes;
        [NotMapped]
        public List<int> ValidModes
        {
            get => validModes;
            private set => SetProperty(ref validModes, value, nameof(ValidModes));
        }

        protected override void OnInputResultChanged()
        {
            if (InputResult == null || InputResult.Count == 0)
                ValidModes = null;
            else
                ValidModes = InputResult[0].Select(track => track.AudioFeatures.Mode).Distinct().OrderBy(mode => mode).ToList();
            Logger.Information($"OnInputResultChanged for {this} (ValidModesCount={ValidModes?.Count})");
        }

        protected override bool CanAddInput(GraphNode input) => !Inputs.Any();
        protected override void MapInputToOutput()
            => OutputResult = InputResult[0].Where(t => t.AudioFeatures.Mode == Mode.Value).ToList();

        public override bool IsValid => Mode != null;
        public override bool RequiresAudioFeatures => true;
    }
}