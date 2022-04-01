using System;
using System.Globalization;
using System.Windows.Data;

namespace SpotifySongTagger.Converters
{
    public class DurationConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
           if (!(value is int ms))
                return "";

            var timespan = TimeSpan.FromMilliseconds(ms);
            return $"{timespan:m\\:ss}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string durationString))
                return null;

            if (durationString == "")
                return null;
            
            
            if (!TimeSpan.TryParseExact(durationString, "m\\:ss", CultureInfo.InvariantCulture, out var timespan))
                return null;

            return timespan.TotalMilliseconds;
        }
    }
}
