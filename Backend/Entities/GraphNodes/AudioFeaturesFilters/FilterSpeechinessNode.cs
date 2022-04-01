using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes.AudioFeaturesFilters
{
    public class FilterSpeechinessNode : FilterRangeNode
    {
        protected override int? GetValue(Track t) => t.AudioFeatures.SpeechinessPercent;
        public override bool RequiresAudioFeatures => true;
    }
}
