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
                case nameof(OutputNode): return "Output";
                case nameof(InputNode): return "Input";
                case nameof(ConcatNode): return "Concatenate";
                case nameof(DeduplicateNode): return "Deduplicate";
                case nameof(TagFilterNode): return "TagFilter";
            }
            return "Unknown Node";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}
