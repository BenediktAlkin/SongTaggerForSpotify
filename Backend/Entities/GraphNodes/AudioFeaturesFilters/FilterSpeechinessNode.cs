using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes.AudioFeaturesFilters
{
    public class FilterSpeechinessNode : FilterRangeNode
    {
        protected override double? GetValue(Track t) => t.AudioFeatures.Speechiness;
        public override bool RequiresAudioFeatures => true;
    }
}
