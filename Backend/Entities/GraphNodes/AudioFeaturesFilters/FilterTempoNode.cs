using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes.AudioFeaturesFilters
{
    public class FilterTempoNode : FilterRangeNode
    {
        protected override int? GetValue(Track t) => (int)t.AudioFeatures.Tempo;
        public override bool RequiresAudioFeatures => true;
    }
}
