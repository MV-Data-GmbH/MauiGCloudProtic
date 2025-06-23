using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace GCloudPhone.Views.Converters
{
    public class ReadStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isRead)
            {
                return isRead ? Colors.Green : Colors.DarkRed;
            }
            return Colors.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
