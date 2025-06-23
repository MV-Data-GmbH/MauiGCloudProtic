using GCloudPhone.Services;
using GCloudPhone.Helpers;
using GCloudPhone.Models;
using GCloudPhone.Views.Shop;
using Newtonsoft.Json;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace GCloudPhone.Views
{
    public partial class OrderQRCode : ContentPage
    {
        // Referenca na nativni QR scanner servis
        IQrScannerService _qrScannerService;

        public OrderQRCode(string barcodeData = null)
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(barcodeData))
            {
                // Ako barcodeData nije null – obrađuj ga
                ProcessBarcode(barcodeData);
            }
            else
            {
                // Ako barcodeData nije prosleđen, skeniranje ćemo pokrenuti u OnAppearing
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Ako barcodeData nije prosleđen, pokrećemo skeniranje
            if (true)
            {
                // Preuzmi servis iz DI kontejnera
                _qrScannerService = ((App)Application.Current).Services.GetService(typeof(IQrScannerService)) as IQrScannerService;

                if (_qrScannerService != null)
                {
                    await _qrScannerService.StartScanningAsync(scannedResult =>
                    {
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            var handler = new QRCodeHandler(this.Navigation);
                            await handler.HandleQRCode(scannedResult);
                        });
                    });
                }
            }
        }

        private void ProcessBarcode(string barcodeData)
        {
            var handler = new QRCodeHandler(this.Navigation);
            handler.HandleQRCode(barcodeData);
        }

        private void ResetForRescan()
        {
            OrderTimeLbl.Text = string.Empty;
            qrCodeImage.Barcode = string.Empty;
            qrCodeImage.IsVisible = false;
            GridQrCoder.IsVisible = false;
        }

        private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
        {
            Navigation.PushAsync(new MainPage());
        }
    }
}