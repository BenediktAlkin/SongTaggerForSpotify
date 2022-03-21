using System;
using System.Globalization;
using System.Windows.Data;

namespace SpotifySongTagger.Converters
{
    public class TimeSignatureConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int timeSignature)
            {
                if (3 <= timeSignature && timeSignature <= 7)
                    return $"{timeSignature}/4";
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string timeSignature)
            {
                if (timeSignature.Length == 3 && timeSignature.EndsWith("/4"))
                {
                    if (int.TryParse(timeSignature[0..1], out var timeSignatureInt))
                        return timeSignatureInt;
                }
            }
            return -1;
        }
    }
}
