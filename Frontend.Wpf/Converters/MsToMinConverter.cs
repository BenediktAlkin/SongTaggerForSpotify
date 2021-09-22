using Backend.Entities.GraphNodes;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Frontend.Wpf.Converters
{
    public class MsToMinConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int ms))
                return "0:00";

            var timespan = TimeSpan.FromMilliseconds(ms);
            return $"{timespan:m\\:ss}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}
