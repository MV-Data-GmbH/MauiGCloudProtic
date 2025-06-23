﻿using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace GCloudPhone.Views.Shop
{
    public class BooleanToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isSelected = (bool)value;
            return isSelected ? Colors.Green : Colors.Gray; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
