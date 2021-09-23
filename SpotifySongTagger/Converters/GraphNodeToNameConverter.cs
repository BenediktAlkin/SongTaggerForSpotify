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
            switch (value.GetType().Name)
            {
                case nameof(AssignTagNode): return "Assign Tag";
                case nameof(ConcatNode): return "Concatenate";
                case nameof(DeduplicateNode): return "Deduplicate";
                case nameof(FilterArtistNode): return "Filter Artist";
                case nameof(FilterTagNode): return "Filter Tag";
                case nameof(PlaylistInputNode): return "Playlist Input";
                case nameof(PlaylistOutputNode): return "Playlist Output";
            }
            return "Unknown Node";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}
