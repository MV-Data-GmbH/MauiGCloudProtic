using System;
using System.Globalization;

namespace GCloudPhone.Views.Shop
{
    public class FontSizeToImageSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double fontSize)
            {
                // 2 x Font size
                return fontSize * 2;
            }
            return 30; // Fallback-size
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}