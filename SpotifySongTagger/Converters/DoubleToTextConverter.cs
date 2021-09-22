using Backend.Entities.GraphNodes;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}
