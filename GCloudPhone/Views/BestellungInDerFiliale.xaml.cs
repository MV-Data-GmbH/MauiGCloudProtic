using GCloudPhone.Services;
using Microsoft.Maui.Storage;         // For Preferences
using Microsoft.Maui.Devices.Sensors;  // For Geolocation
using Microsoft.Maui.Controls;
using GCloudShared.Interface;
using GCloudShared.Service;
using GCloudPhone.Views.Settings.MyAccount;

namespace GCloudPhone.Views
{
    public partial class BestellungInDerFiliale : ContentPage
    {
        private QRCodeHandler qrCodeHandler;  // Used to process scanned QR code data
        private IAuthService _authService;
        private bool qrCodeScanned = false;
        private IWebShopService webShopService;
        private readonly IQrScannerService _qrScannerService;  // Native QR scanner service

        public BestellungInDerFiliale(IAuthService authService, IQrScannerService qrScannerService)
        {
            InitializeComponent();
            _authService = authService;
            _qrScannerService = qrScannerService;
            webShopService = new WebShopService();
            BindingContext = this;

            QRLogged(authService);
            qrCodeHandler = new QRCodeHandler(Navigation);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (!qrCodeScanned)
            {
                qrCodeScanned = true;
                // Prosleđujemo CameraPreviewContainer i instancu _authService koju ste već dobili u konstruktoru
                await _qrScannerService.StartScanningAsync(async (qrText) =>
                {
                    await qrCodeHandler.HandleQRCode(qrText);
                    qrCodeScanned = false;
                }, CameraPreviewContainer, _authService);
            }
        }

        protected override bool OnBackButtonPressed()
        {
            // Pre nego što se korisnik prebaci na MainPage (ili drugu stranicu), možete dodati eventualno
            // dodatno čišćenje/skidanje preview-a, ako bi to bilo neophodno.
            Navigation.PushAsync(new MainPage());
            return true;
        }

        private void OnBackButtonClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new MainPage());
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Resetuj zastavicu, pa ukoliko korisnik ponovo otvori ovu stranicu, skeniranje će započeti od početka.
            qrCodeScanned = false;
        }

        private async void QRLogged(IAuthService authService)
        {
            try
            {
                if (!authService.IsLogged())
                {
                    await DisplayAlert("Fehler", "Sie müssen angemeldet sein, um fortzufahren!", "OK");
                    await Navigation.PushAsync(new LoginPage(authService));
                    return;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error in QRLogged: {ex.Message}", "OK");
            }
        }

        
    }
}