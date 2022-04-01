using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes.AudioFeaturesFilters
{
    public class FilterAcousticnessNode : FilterRangeNode
    {
        protected override int? GetValue(Track t) => t.AudioFeatures.AcousticnessPercent;
        public override bool RequiresAudioFeatures => true;
    }
}
