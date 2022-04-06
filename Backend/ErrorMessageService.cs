using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    public static class ErrorMessageService
    {
        public delegate void MissingAudioFeaturesEventHandler();
        public static event MissingAudioFeaturesEventHandler OnMissingAudioFeatures;

        private static DateTime? LastOnMissingAudioFeaturesTrigger;
        public static void TriggerMissingAudioFeatures()
        {
            // running a GraphNode would trigger this for every song where AudioFeatures are missing
            var now = DateTime.Now;
            if (LastOnMissingAudioFeaturesTrigger == null || (now - LastOnMissingAudioFeaturesTrigger.Value).TotalSeconds > 10)
            {
                OnMissingAudioFeatures?.Invoke();
                LastOnMissingAudioFeaturesTrigger = now;
            }
                
        }
    }
}
