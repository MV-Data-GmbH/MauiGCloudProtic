using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace GCloudPhone.Views.Shop
{
    public class BooleanToFontAttributesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected)
            {
                return isSelected ? FontAttributes.Bold : FontAttributes.None;
            }

            return FontAttributes.None;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
