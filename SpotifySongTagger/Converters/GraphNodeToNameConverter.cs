using Backend.Entities.GraphNodes;
using Backend.Entities.GraphNodes.AudioFeaturesFilters;
using System;
using System.Globalization;
using System.Windows.Data;

namespace SpotifySongTagger.Converters
{
    public class GraphNodeToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var name = "Unknown Node";
            if (!(value is GraphNode gn)) return name;

            name = value.GetType().Name switch
            {
                nameof(AssignTagNode) => name = "Assign Tag",
                nameof(ConcatNode) => name = "Concatenate",
                nameof(DeduplicateNode) => name = "Deduplicate",
                nameof(FilterArtistNode) => name = "Filter Artist",
                nameof(FilterTagNode) => name = "Filter Tag",
                nameof(FilterUntaggedNode) => name = "Filter Untagged",
                nameof(FilterYearNode) => name = "Filter Release Year",
                nameof(IntersectNode) => name = "Intersect",
                nameof(PlaylistInputMetaNode) => name = "Meta Playlist Input",
                nameof(PlaylistInputLikedNode) => name = "Liked Playlist Input",
                nameof(PlaylistOutputNode) => name = "Playlist Output",
                nameof(RemoveNode) => name = "Remove",

                nameof(FilterAcousticnessNode) => name = "Acousticness",
                nameof(FilterDanceabilityNode) => name = "Danceability",
                nameof(FilterDurationMsNode) => name = "Duration [m:ss]",
                nameof(FilterEnergyNode) => name = "Energy",
                nameof(FilterInstrumentalnessNode) => name = "Instrumentalness",
                nameof(FilterKeyNode) => name = "Key",
                nameof(FilterLivenessNode) => name = "Liveness",
                nameof(FilterLoudnessNode) => name = "Loudness",
                nameof(FilterModeNode) => name = "Mode",
                nameof(FilterSpeechinessNode) => name = "Speechiness",
                nameof(FilterTempoNode) => name = "Tempo (BPM)",
                nameof(FilterTimeSignatureNode) => name = "Time Signature",
                nameof(FilterValenceNode) => name = "Valence",
                nameof(FilterGenreNode) => name = "Genre",
                _ => name = "Unknown Node",
            };

            return $"{name} ({gn.Id})";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}
