using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entities.GraphNodes.AudioFeaturesFilters
{
    public class FilterTimeSignatureNode : FilterRangeNode
    {
        protected override double? GetValue(Track t) => t.AudioFeatures.TimeSignature;
        public override bool RequiresAudioFeatures => true;
    }
}
