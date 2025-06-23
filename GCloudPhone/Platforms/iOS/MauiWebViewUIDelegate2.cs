using Microsoft.Maui.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebKit;
using System;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Platform;
using System.Runtime.Versioning;
using UIKit;
#nullable enable

namespace GCloudPhone.Platforms.iOS
{
    sealed class MauiWebViewUIDelegate2 : MauiWebViewUIDelegate
    {
        public MauiWebViewUIDelegate2(IWebViewHandler handler) : base(handler)
        {
        }

        [SupportedOSPlatform("maccatalyst13.1")]
        [SupportedOSPlatform("ios13.0")]
        [UnsupportedOSPlatform("macos")]
        [UnsupportedOSPlatform("tvos")]


        public override void SetContextMenuConfiguration(WKWebView webView, WKContextMenuElementInfo elementInfo, Action<UIContextMenuConfiguration> completionHandler)
        {
            bool completionHandlerIsInvoked = false;

            base.SetContextMenuConfiguration(webView, elementInfo, cfg =>
            {
                completionHandlerIsInvoked = true;
                completionHandler(cfg);
            });

            if (completionHandlerIsInvoked == false)
                completionHandler(null!);
        }
    }

    sealed class WebViewHandler2 : WebViewHandler
    {
        WKUIDelegate? _delegate;

        //protected override WKWebView CreatePlatformView()
        //{
        //    var webView = base.CreatePlatformView();
        //    webView.AllowsLinkPreview = false;  // Set AllowsLinkPreview to false
        //    return webView;
        //}

        public static void MapWKUIDelegate2(IWebViewHandler handler, IWebView webView)
        {
            if (handler is WebViewHandler2 platformHandler)
                handler.PlatformView.UIDelegate = platformHandler._delegate ??= new MauiWebViewUIDelegate2(handler);
        }
    }

 
}
