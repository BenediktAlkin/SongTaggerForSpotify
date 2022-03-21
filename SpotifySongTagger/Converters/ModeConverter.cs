using System;
using System.Globalization;
using System.Windows.Data;

namespace SpotifySongTagger.Converters
{
    public class ModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int mode)
            {
                return mode switch
                {
                    0 => "minor",
                    1 => "major",
                    _ => "",
                };
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string mode)
            {
                return mode switch
                {
                    "minor" => 0,
                    "major" => 1,
                    _ => -1,
                };
            }
            return -1;
        }
    }
}
