﻿using Backend.Entities.GraphNodes;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using static Util.UpdateManager;

namespace Frontend.Wpf.Converters
{
    public class UpdatingStateToTextConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is UpdatingState state))
                return string.Empty;

            switch(state)
            {
                case UpdatingState.Checking:
                case UpdatingState.Preparation:
                    return "Checking for update";
                case UpdatingState.Downloading:
                    return "Downloading update";
                case UpdatingState.Extracting:
                    return "Extracting update";
                case UpdatingState.Restarting:
                    return "Restarting application";
            }; 
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}
