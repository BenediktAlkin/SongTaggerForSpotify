using System;
using System.Globalization;
using System.Windows.Data;

namespace SpotifySongTagger.Converters
{
    public class DoubleToTextConverter : IValueConverter
    {
        public string Suffix { get; set; } = string.Empty;
        public string Format { get; set; } = "N2";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string doubleStr;
            if (value is double mb)
                doubleStr = mb.ToString(Format);
            else
                doubleStr = default(double).ToString(Format);

            return $"{doubleStr}{Suffix}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var valueStr = (value ?? "").ToString();
            if (double.TryParse(valueStr, out var parsedValue))
                return parsedValue;
            return null;
        }
    }
}
