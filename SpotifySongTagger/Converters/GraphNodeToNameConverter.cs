using Backend.Entities.GraphNodes;
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
                nameof(FilterYearNode) => name = "Filter Release Year",
                nameof(IntersectNode) => name = "Intersect",
                nameof(PlaylistInputNode) => name = "Playlist Input",
                nameof(PlaylistOutputNode) => name = "Playlist Output",
                nameof(RemoveNode) => name = "Remove",
                _ => name = "Unknown Node",
            };

            return $"{name} ({gn.Id})";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}
