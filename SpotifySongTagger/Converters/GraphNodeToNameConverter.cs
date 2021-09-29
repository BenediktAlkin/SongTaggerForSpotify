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

            switch (value.GetType().Name)
            {
                case nameof(AssignTagNode): name = "Assign Tag"; break;
                case nameof(ConcatNode): name = "Concatenate"; break;
                case nameof(DeduplicateNode): name = "Deduplicate"; break;
                case nameof(FilterArtistNode): name = "Filter Artist"; break;
                case nameof(FilterTagNode): name = "Filter Tag"; break;
                case nameof(PlaylistInputNode): name = "Playlist Input"; break;
                case nameof(PlaylistOutputNode): name = "Playlist Output"; break;
                case nameof(RemoveNode): name = "Remove"; break;
            }

            return $"{name} ({gn.Id})";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}
