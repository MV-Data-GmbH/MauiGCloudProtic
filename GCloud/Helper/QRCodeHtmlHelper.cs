using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace GCloud.Helper
{
    public static class QrCodeHtmlHelper
    {
        public static MvcHtmlString QRCode(this HtmlHelper htmlHelper, string data, int size = 80, int margin = 4, QrCodeErrorCorrectionLevel errorCorrectionLevel = QrCodeErrorCorrectionLevel.Low, object htmlAttributes = null)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (size < 1)
                throw new ArgumentOutOfRangeException("size", size, "Must be greater than zero.");
            if (margin < 0)
                throw new ArgumentOutOfRangeException("margin", margin, "Must be greater than or equal to zero.");
            if (!Enum.IsDefined(typeof(QrCodeErrorCorrectionLevel), errorCorrectionLevel))
                throw new InvalidEnumArgumentException("errorCorrectionLevel", (int)errorCorrectionLevel, typeof(QrCodeErrorCorrectionLevel));

            var url = string.Format("http://chart.apis.google.com/chart?cht=qr&chld={2}|{3}&chs={0}x{0}&chl={1}", size, HttpUtility.UrlEncode(data), errorCorrectionLevel.ToString()[0], margin);

            var tag = new TagBuilder("img");
            if (htmlAttributes != null)
                tag.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            tag.Attributes.Add("src", url);
            tag.Attributes.Add("width", size.ToString());
            tag.Attributes.Add("height", size.ToString());

            return new MvcHtmlString(tag.ToString(TagRenderMode.SelfClosing));
        }

        public static Image GetQrCodeImage(string data, int size = 80, int margin = 4,QrCodeErrorCorrectionLevel errorCorrectionLevel = QrCodeErrorCorrectionLevel.Low)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (size < 1)
                throw new ArgumentOutOfRangeException("size", size, "Must be greater than zero.");
            if (margin < 0)
                throw new ArgumentOutOfRangeException("margin", margin, "Must be greater than or equal to zero.");
            if (!Enum.IsDefined(typeof(QrCodeErrorCorrectionLevel), errorCorrectionLevel))
                throw new InvalidEnumArgumentException("errorCorrectionLevel", (int)errorCorrectionLevel, typeof(QrCodeErrorCorrectionLevel));

            var url = string.Format("http://chart.apis.google.com/chart?cht=qr&chld={2}|{3}&chs={0}x{0}&chl={1}", size, HttpUtility.UrlEncode(data), errorCorrectionLevel.ToString()[0], margin);

            var webclient = new WebClient();
            var bytes = webclient.DownloadData(url);
            using (var ms = new MemoryStream(bytes))
            {
                return Image.FromStream(ms);
            }
        }
    }

    public enum QrCodeErrorCorrectionLevel
    {
        /// <summary>Recovers from up to 7% erroneous data.</summary>
        Low,
        /// <summary>Recovers from up to 15% erroneous data.</summary>
        Medium,
        /// <summary>Recovers from up to 25% erroneous data.</summary>
        QuiteGood,
        /// <summary>Recovers from up to 30% erroneous data.</summary>
        High
    }
}