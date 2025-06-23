// HalfWidthConverter.cs
using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace GCloudPhone.Converters
{
    public class HalfWidthConverter : IValueConverter
    {
        // parameter = ukupni horizontalni razmak (u pikselima) između itema i margine
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double totalWidth && double.TryParse(parameter as string, out double spacing))
            {
                // oduzmemo ukupni spacing (margin-left + spacing između + margin-right), pa podelimo na 2
                return (totalWidth - spacing * 3) / 2;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}