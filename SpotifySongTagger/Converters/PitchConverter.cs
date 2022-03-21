using System;
using System.Globalization;
using System.Windows.Data;

namespace SpotifySongTagger.Converters
{
    public class PitchConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int pitch)
            {
                return pitch switch
                {
                    0 => "C",
                    1 => "C#",
                    2 => "D",
                    3 => "D#",
                    4 => "E",
                    5 => "F",
                    6 => "F#",
                    7 => "G",
                    8 => "G#",
                    9 => "A",
                    10 => "A#",
                    11 => "B",
                    _ => "",
                };
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string pitch)
            {
                return pitch switch
                {
                    "C" => 0,
                    "C#" => 1,
                    "D" => 2,
                    "D#" => 3,
                    "E" => 4,
                    "F" => 5,
                    "F#" => 6,
                    "G" => 7,
                    "G#" => 8,
                    "A" => 9,
                    "A#" => 10,
                    "B" => 11,
                    _ => -1,
                };
            }
            return -1;
        }
    }
}
