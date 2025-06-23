using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using GCloud.Helper;
using Microsoft.AspNet.Identity;

namespace GCloud.Controllers.api
{
    [Authorize]
    public class UsersApiController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage GetQrCode()
        {
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            var image = QrCodeHtmlHelper.GetQrCodeImage(User.Identity.GetUserId(),250,1);

            using (var ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Png);
                result.Content = new ByteArrayContent(ms.ToArray());
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                return result;
            }
        }
    }
}