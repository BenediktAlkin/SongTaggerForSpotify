using System;
using System.Globalization;
using System.Windows.Data;

namespace SpotifySongTagger.Converters
{
    public class ObjectToTypeNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value.GetType().Name;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}
