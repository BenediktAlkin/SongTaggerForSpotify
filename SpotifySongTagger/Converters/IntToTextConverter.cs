using System;
using System.Globalization;
using System.Windows.Data;

namespace SpotifySongTagger.Converters
{
    public class IntToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (value ?? "").ToString();

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var valueStr = (value ?? "").ToString();
            if (int.TryParse(valueStr, out var parsedValue))
                return parsedValue;
            return null;
        }
    }
}
