using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes.AudioFeaturesFilters
{
    public class FilterInstrumentalnessNode : FilterRangeNode
    {
        protected override double? GetValue(Track t) => t.AudioFeatures.Instrumentalness;
        public override bool RequiresAudioFeatures => true;
    }
}
