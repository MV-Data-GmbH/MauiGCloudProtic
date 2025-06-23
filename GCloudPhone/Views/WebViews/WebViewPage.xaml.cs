using CommunityToolkit.Maui.Alerts;
using GCloudPhone.Views.Shop.Checkout;
using GCloudPhone.Views.Shop.Payments;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Threading.Tasks;

namespace GCloudPhone.Views.WebViews
{
    public partial class WebViewPage : ContentPage
    {
        private TaskCompletionSource<bool> paymentResultTaskCompletionSource;
        private readonly DeliveryCheckout _deliveryPage;
        private readonly PickupCheckout _pickupPage;
        private readonly InStoreCheckout _inStoreCheckoutPage;

        public WebViewPage(string url, ContentPage parentPage = null)
        {
            InitializeComponent();

            // Postavljamo početni URL u WebView
            MyWebView.Source = url;
            Console.WriteLine($"[WebViewPage] Initial URL: {url}");

            // Pamćenje roditeljske stranice (odakle je pozvano plaćanje)
            if (parentPage is DeliveryCheckout deliveryCheckout)
            {
                _deliveryPage = deliveryCheckout;
                Console.WriteLine($"[WebViewPage] Parent page: DeliveryCheckout");
            }
            else if (parentPage is PickupCheckout pickupCheckout)
            {
                _pickupPage = pickupCheckout;
                Console.WriteLine($"[WebViewPage] Parent page: PickupCheckout");
            }
            else if (parentPage is InStoreCheckout inStoreCheckoutPage)
            {
                _inStoreCheckoutPage = inStoreCheckoutPage;
                Console.WriteLine($"[WebViewPage] Parent page: InStoreCheckout");
            }
            else
            {
                Console.WriteLine($"[WebViewPage] Parent page: none");
            }

            paymentResultTaskCompletionSource = new TaskCompletionSource<bool>();
        }

        public Task<bool> GetPaymentResultAsync()
        {
            return paymentResultTaskCompletionSource.Task;
        }

        private void OnSwipedRight(object sender, SwipedEventArgs e)
        {
            Console.WriteLine($"[WebViewPage] OnSwipedRight - vraćam se nazad");
            Navigation.PopAsync();
        }

        private async void MyWebView_Navigated(object sender, WebNavigatedEventArgs e)
        {
            // 1) Ispis rezultata navigacije i dobijenog URL-a
            Console.WriteLine($"[WebViewPage] Navigated: Result = {e.Result}, Url = '{e.Url}'");

            // 2) Provera da li je navigacija bila uspešna i da li URL nije prazan/null
            if (e.Result != WebNavigationResult.Success || string.IsNullOrEmpty(e.Url))
            {
                Console.WriteLine("[WebViewPage] Navigacija nije uspela ili je URL prazan.");
                return;
            }

            // 3) Pokušavamo da parsiramo e.Url koristeći TryCreate kako bismo izbegli izuzetak
            if (!Uri.TryCreate(e.Url, UriKind.Absolute, out Uri parsedUri))
            {
                Console.WriteLine($"[WebViewPage] Nevalidan URI, preskačem: '{e.Url}'");
                return;
            }

            // 4) Ako je validan, ispišemo detalje o parsiranom URI-ju
            Console.WriteLine($"[WebViewPage] Parsed URI -> Scheme: {parsedUri.Scheme}, Host: {parsedUri.Host}, AbsolutePath: {parsedUri.AbsolutePath}");

            // 5) Definišemo očekivani host i putanje
            const string expectedHost = "errortestingprotic.willessen.online";
            string host = parsedUri.Host;          // npr. "errortestingprotic.willessen.online"
            string path = parsedUri.AbsolutePath;  // npr. "/SuccessfulPayment" ili "/FailedPayment"
            Console.WriteLine($"[WebViewPage] Host = {host}, Path = {path}");

            // 6) Provera da li smo na ispravnoj stranici za uspeh/neuspeh plaćanja
            if (host.Equals(expectedHost, StringComparison.OrdinalIgnoreCase) &&
                (path.Equals("/SuccessfulPayment", StringComparison.OrdinalIgnoreCase) ||
                 path.Equals("/FailedPayment", StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine($"[WebViewPage] URL odgovara uslovu plaćanja ({path}). Evaluiram JavaScript...");

                string result = string.Empty;
                try
                {
                    result = await MyWebView.EvaluateJavaScriptAsync("document.title");
                    Console.WriteLine($"[WebViewPage] document.title = \"{result}\"");
                }
                catch (Exception jsEx)
                {
                    Console.WriteLine($"[WebViewPage] Error evaluating JavaScript: {jsEx.Message}");
                    paymentResultTaskCompletionSource.SetException(jsEx);
                    return;
                }

                // 7) Obrada naslova stranice
                if (result.Equals("SuccessfulPayment", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"[WebViewPage] Payment SUCCESS");
                    paymentResultTaskCompletionSource.SetResult(true);

                    // Vraćamo se nazad
                    await Navigation.PopAsync();

                    // Pozivamo odgovarajući ProcessOrderExternally()
                    if (_deliveryPage != null)
                    {
                        Console.WriteLine($"[WebViewPage] Pozivam DeliveryCheckout.ProcessOrderExternally()");
                        await _deliveryPage.ProcessOrderExternally();
                    }
                    else if (_pickupPage != null)
                    {
                        Console.WriteLine($"[WebViewPage] Pozivam PickupCheckout.ProcessOrderExternally()");
                        await _pickupPage.ProcessOrderExternally();
                    }
                    else if (_inStoreCheckoutPage != null)
                    {
                        Console.WriteLine($"[WebViewPage] Pozivam InStoreCheckout.ProcessOrderExternally()");
                        await _inStoreCheckoutPage.ProcessOrderExternally();
                    }
                }
                else
                {
                    Console.WriteLine($"[WebViewPage] Payment FAILED (title = \"{result}\")");
                    paymentResultTaskCompletionSource.SetResult(false);

                    // Prikazujemo toast sa porukom
                    await Toast.Make(result, CommunityToolkit.Maui.Core.ToastDuration.Long, 30).Show();

                    // Navigacija na stranicu neuspešnog plaćanja
                    Console.WriteLine($"[WebViewPage] Navigiram na UnsuccessfulPayment stranicu");
                    await Navigation.PushAsync(new UnsuccessfulPayment());
                }
            }
            else
            {
                // 8) Ako URL ne odgovara uslovima, ispišemo zašto nije poklapanje
                if (!host.Equals(expectedHost, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"[WebViewPage] Nepodudaran host: \"{host}\" (očekivano: \"{expectedHost}\")");
                }
                if (!(path.Equals("/SuccessfulPayment", StringComparison.OrdinalIgnoreCase) ||
                      path.Equals("/FailedPayment", StringComparison.OrdinalIgnoreCase)))
                {
                    Console.WriteLine($"[WebViewPage] Nepodudaran path: \"{path}\"; nije \"/SuccessfulPayment\" niti \"/FailedPayment\"");
                }
            }
        }
    }
}
