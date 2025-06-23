using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace GCloud.Helper
{
    public static class ImageUtils
    {
        public static Image ResizeImageToFixedHeight(Image imgPhoto, int height)
        {
            float sourceWidth = imgPhoto.Width;
            float sourceHeight = imgPhoto.Height;
            float destHeight = height;
            float destWidth = 0;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            destWidth = sourceWidth * height / sourceHeight;
            // Width is greater than height, set Width = logoSize and resize height accordingly

            Bitmap bmPhoto = new Bitmap((int)destWidth, (int)destHeight,
                PixelFormat.Format32bppPArgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, (int)destWidth, (int)destHeight),
                new Rectangle(sourceX, sourceY, (int)sourceWidth, (int)sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();

            return bmPhoto;
        }
    }
}
