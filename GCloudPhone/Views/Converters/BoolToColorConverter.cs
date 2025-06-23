using System;
using System.Globalization;
using Microsoft.Maui.Controls;


namespace GCloudPhone.Views.Shop
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isHighlighted && isHighlighted)
            {
                return Colors.LightGreen;
            }

            return Colors.White; // Standardfarbe
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
