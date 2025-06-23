using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using GCloudPhone.Models;
using GCloudPhone.Views.Shop;
using GCloudPhone.Views.WebViews;

namespace GCloudPhone.Services
{
    public class PaymentService
    {
        private readonly ApiService _apiService;
        private TaskCompletionSource<bool> _paymentCompletionSource;

        public PaymentService()
        {
            _apiService = new ApiService();
        }

        public async Task<bool> ProcessPaymentAsync(string paymentMethod, decimal totalPrice, ContentPage parentPage, string orderId)
        {
            Debug.WriteLine($"[Payment] Start ProcessPaymentAsync: Method='{paymentMethod}', Amount={totalPrice}, OrderId='{orderId}'");
            switch (paymentMethod)
            {
                case "Google Pay":
                    return await ProcessFastPayAsyncGoogle(totalPrice, parentPage, orderId);

                case "Apple Pay":
                    return await ProcessFastPayAsyncApple(totalPrice, parentPage, orderId);

                case "Kreditkarte":
                    return await ProcessCardPaymentAsync(totalPrice, parentPage, orderId);

                case "Bar bezahlen":
                    Debug.WriteLine("[Payment] Cash on Delivery selected");
                    await parentPage.DisplayAlert("Zahlung", "Bitte bezahlen Sie in bar bei der Lieferung.", "OK");
                    return true;

                default:
                    Debug.WriteLine($"[Payment] Invalid payment method: {paymentMethod}");
                    await parentPage.DisplayAlert("Fehler", "Ungültige Zahlungsmethode ausgewählt.", "OK");
                    return false;
            }
        }

        private async Task<bool> ProcessFastPayAsyncApple(decimal totalPrice, ContentPage parentPage, string orderId)
        {
            Debug.WriteLine($"[Payment][Apple] Begin: totalPrice={totalPrice}, orderId={orderId}");
            int amountInCents = Convert.ToInt32(totalPrice * 100);
            string fastpayId = GetOrGenerateFastPayID();
            string ordernumber = GenerateOrderNumber();

            Debug.WriteLine($"[Payment][Apple] amountInCents={amountInCents}, fastpayId='{fastpayId}', ordernumber='{ordernumber}'");

            UrlResponse result;
            try
            {
                result = await _apiService.GetApiDataAsync(
                    amountInCents.ToString(), ordernumber, fastpayId, "APAY");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Payment][Apple] EXCEPTION in GetApiDataAsync: {ex}");
                await parentPage.DisplayAlert("Fehler", $"API-Fehler: {ex.Message}", "OK");
                return false;
            }

            if (result == null || string.IsNullOrWhiteSpace(result.url))
            {
                Debug.WriteLine($"[Payment][Apple] Invalid UrlResponse: {(result == null ? "NULL" : result.url)}");
                await parentPage.DisplayAlert("Fehler", "Keine Zahlungsdaten erhalten.", "OK");
                return false;
            }

            Debug.WriteLine($"[Payment][Apple] Retrieved URL = {result.url}");

            // Otvaranje WebViewPage
            WebViewPage webViewPage;
            try
            {
                webViewPage = new WebViewPage(result.url, parentPage);
                await parentPage.Navigation.PushAsync(webViewPage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Payment][Apple] EXCEPTION opening WebView: {ex}");
                await parentPage.DisplayAlert("Fehler", $"WebView-Fehler: {ex.Message}", "OK");
                return false;
            }

            // Čekanje na ishod putem vlastitog GetPaymentResultAsync
            bool paymentSuccess;
            try
            {
                paymentSuccess = await webViewPage.GetPaymentResultAsync();
                Debug.WriteLine($"[Payment][Apple] GetPaymentResultAsync = {paymentSuccess}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Payment][Apple] EXCEPTION in GetPaymentResultAsync: {ex}");
                await parentPage.DisplayAlert("Fehler", $"Zahlungsergebnis-Fehler: {ex.Message}", "OK");
                return false;
            }

            return paymentSuccess;
        }


        private async Task<bool> ProcessCardPaymentAsync(decimal totalPrice, ContentPage parentPage, string orderId)
        {
            Debug.WriteLine($"[Payment][Card] Begin: totalPrice={totalPrice}, orderId={orderId}");
            int amountInCents = Convert.ToInt32(totalPrice * 100);
            string fastpayId = GetOrGenerateFastPayID();
            string ordernumber = GenerateOrderNumber();

            Debug.WriteLine($"[Payment][Card] amountInCents={amountInCents}, fastpayId='{fastpayId}', ordernumber='{ordernumber}'");

            UrlResponse result;
            try
            {
                result = await _apiService.GetApiDataAsync(
                    amountInCents.ToString(), ordernumber, fastpayId, "CRD");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Payment][Card] EXCEPTION in GetApiDataAsync: {ex}");
                await parentPage.DisplayAlert("Fehler", $"API-Fehler: {ex.Message}", "OK");
                return false;
            }

            if (result == null || string.IsNullOrWhiteSpace(result.url))
            {
                Debug.WriteLine($"[Payment][Card] Invalid UrlResponse: {(result == null ? "NULL" : result.url)}");
                await parentPage.DisplayAlert("Fehler", "Keine Zahlungsdaten erhalten (NULL oder leere URL).", "OK");
                return false;
            }

            Debug.WriteLine($"[Payment][Card] Retrieved URL = {result.url}");

            WebViewPage webViewPage;
            try
            {
                webViewPage = new WebViewPage(result.url, parentPage);
                await parentPage.Navigation.PushAsync(webViewPage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Payment][Card] EXCEPTION opening WebView: {ex}");
                await parentPage.DisplayAlert("Fehler", $"WebView-Fehler: {ex.Message}", "OK");
                return false;
            }

            bool paymentSuccess;
            try
            {
                paymentSuccess = await webViewPage.GetPaymentResultAsync();
                Debug.WriteLine($"[Payment][Card] GetPaymentResultAsync = {paymentSuccess}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Payment][Card] EXCEPTION in GetPaymentResultAsync: {ex}");
                await parentPage.DisplayAlert("Fehler", $"Zahlungsergebnis-Fehler: {ex.Message}", "OK");
                return false;
            }

            return paymentSuccess;
        }

        private async Task<bool> ProcessFastPayAsyncGoogle(decimal totalPrice, ContentPage parentPage, string orderId)
        {
            Debug.WriteLine($"[Payment][Google] Begin: totalPrice={totalPrice}, orderId={orderId}");
            int amountInCents = Convert.ToInt32(totalPrice * 100);
            string fastpayId = GetOrGenerateFastPayID();
            string ordernumber = GenerateOrderNumber();

            Debug.WriteLine($"[Payment][Google] amountInCents={amountInCents}, fastpayId='{fastpayId}', ordernumber='{ordernumber}'");

            UrlResponse result;
            try
            {
                result = await _apiService.GetApiDataAsync(
                    amountInCents.ToString(), ordernumber, fastpayId, "APAY");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Payment][Google] EXCEPTION in GetApiDataAsync: {ex}");
                await parentPage.DisplayAlert("Fehler", $"API-Fehler: {ex.Message}", "OK");
                return false;
            }

            if (result == null || string.IsNullOrWhiteSpace(result.url))
            {
                Debug.WriteLine($"[Payment][Google] Invalid UrlResponse: {(result == null ? "NULL" : result.url)}");
                await parentPage.DisplayAlert("Fehler", "Keine Zahlungsdaten erhalten (NULL oder leere URL).", "OK");
                return false;
            }

            Debug.WriteLine($"[Payment][Google] Retrieved URL = {result.url}");
            _paymentCompletionSource = new TaskCompletionSource<bool>();

            try
            {
                await Launcher.OpenAsync(new Uri(result.url));
                Debug.WriteLine("[Payment][Google] Launched external URL, awaiting result...");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Payment][Google] EXCEPTION launching URL: {ex}");
                await parentPage.DisplayAlert("Fehler", $"Launcher-Fehler: {ex.Message}", "OK");
                return false;
            }

            bool paymentResult;
            try
            {
                paymentResult = await _paymentCompletionSource.Task;
                Debug.WriteLine($"[Payment][Google] PaymentCompletionSource result = {paymentResult}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Payment][Google] EXCEPTION waiting for result: {ex}");
                await parentPage.DisplayAlert("Fehler", $"Zahlungsergebnis-Fehler: {ex.Message}", "OK");
                return false;
            }

            return paymentResult;
        }

        // Helperi

        private string GetOrGenerateFastPayID()
        {
            if (Preferences.ContainsKey("ordernumber"))
            {
                string id = Preferences.Get("ordernumber", "");
                Debug.WriteLine($"[Payment] Reusing fastpayId = '{id}'");
                return id;
            }
            Debug.WriteLine("[Payment] fastpayId nije pronađen u Preferences");
            return null;
        }

        private string GenerateOrderNumber()
        {
            string ordernumber = new Random().Next(1, 999999999).ToString();
            Preferences.Set("ordernumber", ordernumber);
            Debug.WriteLine($"[Payment] Generated ordernumber = '{ordernumber}'");
            return ordernumber;
        }
    }
}
