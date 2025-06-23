#if ANDROID
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using ZXing.Net.Maui.Controls; // Kontrola za skeniranje
using ZXing.Net.Maui;         // Osnovni namespace za ZXing.Net.MAUI
using GCloudPhone.Services;
using GCloudShared.Interface;

namespace GCloudPhone.Platforms.Android
{
    // Android servis za full-screen QR skeniranje koristeći ZXing.Net.MAUI
    public class QrScannerService_Android : IQrScannerService
    {
        /// <summary>
        /// Pokreće full-screen skeniranje i poziva callback sa skeniranom vrednošću QR koda.
        /// Modalna stranica se kreira i prikazuje, a rezultat se čeka putem TaskCompletionSource.
        /// </summary>
        /// <param name="onScanned">Callback koji prima string rezultat skeniranog QR koda.</param>
        public async Task StartScanningAsync(Action<string> onScanned)
        {
            // Kreiramo TaskCompletionSource da asinkrono čekamo rezultat
            var tcs = new TaskCompletionSource<string>();

            // Kreiramo kontrolu za skeniranje
            var readerView = new CameraBarcodeReaderView
            {
                Options = new BarcodeReaderOptions
                {
                    // Konfigurišemo samo QR kod: direktno koristimo enum član iz ZXing
                    Formats = (BarcodeFormat)ZXing.BarcodeFormat.QR_CODE,
                    AutoRotate = true,
                    Multiple = false
                },
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                // Opcionalno: možete podesiti dodatne vizuelne opcije ako je potrebno
            };

            // Kada se otkriju bar kodovi, proslijedite prvi rezultat i postavite rezultat na TaskCompletionSource
            readerView.BarcodesDetected += (s, e) =>
            {
                if (e.Results != null && e.Results.Count() > 0)
                {
                    var barcodeValue = e.Results.First().Value; // Uzimamo prvi rezultat
                    tcs.TrySetResult(barcodeValue);
                }
            };

            // Kreiramo modalnu stranicu koja sadrži kontrolu za skeniranje
            var scanningPage = new ContentPage
            {
                Content = readerView,
                BackgroundColor = Colors.Black // Opcionalno, postavite boju pozadine
            };

            // Prikazujemo modalnu stranicu
            await Application.Current.MainPage.Navigation.PushModalAsync(scanningPage);

            // Čekamo rezultat skeniranja
            var result = await tcs.Task;

            // Nakon dobijanja rezultata, zatvaramo modalnu stranicu
            await Application.Current.MainPage.Navigation.PopModalAsync();

            // Pozivamo callback sa skeniranom vrednošću
            onScanned?.Invoke(result);
        }

        /// <summary>
        /// Overload koji prima MAUI view kao container, ali za Android se koristi full-screen skeniranje.
        /// </summary>
        public Task StartScanningAsync(Action<string> onScanned, View previewContainer)
        {
            // Ignorišemo prosleđeni container jer koristimo modalnu stranicu za full-screen skeniranje
            return StartScanningAsync(onScanned);
        }

        /// <summary>
        /// Overload koji pre pokretanja skeniranja proverava da li je korisnik autentifikovan.
        /// </summary>
        public async Task StartScanningAsync(Action<string> onScanned, View previewContainer, IAuthService authService)
        {
            if (!authService.IsLogged())
                throw new InvalidOperationException("Korisnik nije autentifikovan.");
            await StartScanningAsync(onScanned);
        }

        /// <summary>
        /// Zaustavlja skeniranje ukoliko je potrebno – ovde se implementacija može dopuniti
        /// ako želite programatski da prekinete skeniranje (npr. zatvaranjem modalne stranice).
        /// </summary>
        public void StopScanning()
        {
            // Ako implementirate dodatne mehanizme za otkazivanje, dodajte odgovarajući kod ovde.
            // Pošto se u ovoj implementaciji koristi modalna stranica, otkazivanje možete postići
            // pozivanjem Navigation.PopModalAsync() iz odgovarajućeg UI okruženja.
        }
    }
}
#endif
