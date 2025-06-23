using System;
using System.Globalization;
using System.IO;

namespace GCloudPhone.Views.Shop
{
    public class Base64ToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var base64String = value.ToString();
            byte[] imageBytes = System.Convert.FromBase64String(base64String);
            return ImageSource.FromStream(() => new MemoryStream(imageBytes));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
