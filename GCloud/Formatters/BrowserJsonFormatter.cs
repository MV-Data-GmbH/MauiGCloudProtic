using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web;
using Microsoft.ApplicationInsights.Web;
using Newtonsoft.Json;

namespace GCloud.Formatters
{
    public class BrowserJsonFormatter : JsonMediaTypeFormatter
    {
        public BrowserJsonFormatter()
        {
            this.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            this.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
            this.SerializerSettings.Formatting = Formatting.Indented;
            this.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        }

        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            headers.ContentType = new MediaTypeHeaderValue("application/json");
            base.SetDefaultContentHeaders(type,headers,mediaType);
        }
    }
}