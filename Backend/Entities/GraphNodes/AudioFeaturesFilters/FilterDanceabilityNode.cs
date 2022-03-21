using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes.AudioFeaturesFilters
{
    public class FilterDanceabilityNode : FilterRangeNode
    {
        protected override double? GetValue(Track t) => t.AudioFeatures.Danceability;
        public override bool RequiresAudioFeatures => true;
    }
}
