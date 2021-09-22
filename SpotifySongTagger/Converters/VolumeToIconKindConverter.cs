using MaterialDesignThemes.Wpf;
using System;
using System.Globalization;
using System.Windows.Data;

namespace SpotifySongTagger.Converters
{
    public class VolumeToIconKindConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double volume))
                return PackIconKind.VolumeMedium;
            if (volume > 66) return PackIconKind.VolumeHigh;
            if (volume > 33) return PackIconKind.VolumeMedium;
            if (volume > 0) return PackIconKind.VolumeLow;
            return PackIconKind.VolumeMute;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}
