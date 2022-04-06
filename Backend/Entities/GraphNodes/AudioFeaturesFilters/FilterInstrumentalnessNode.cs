using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes.AudioFeaturesFilters
{
    public class FilterInstrumentalnessNode : FilterRangeNode
    {
        // if db is pre-AudioFeatures even including AudioFeatures results in AudioFeature being null
        protected override int? GetValue(Track t)
        {
            if (t.AudioFeatures == null)
                ErrorMessageService.TriggerMissingAudioFeatures();
            return t.AudioFeatures?.InstrumentalnessPercent;
        }
        
        public override bool RequiresAudioFeatures => true;
    }
}
