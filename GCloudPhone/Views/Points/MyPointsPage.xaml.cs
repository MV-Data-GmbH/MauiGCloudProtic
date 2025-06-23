using GCloudPhone.Services;
using GCloudPhone.Views.Aktionen;
using GCloudPhone.Views.Settings.MyAccount;
using GCloudPhone.Views.Shop;
using GCloudPhone.Views.Shop.OrderProccess;
using GCloudShared.Interface;
using GCloudShared.Repository;
using GCloudShared.Shared;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace GCloudPhone.Views.Points
{
    public partial class MyPointsPage : ContentPage
    {
        private IAuthService _authService;
        private bool IsLoging;

        public MyPointsPage(IAuthService authService)
        {
            InitializeComponent();
            _authService = authService;
            MyPointsLogged(authService);
            GetPoints();

            // Pretplata na događaje NavigationBar-a
            navigationBar.HomeTapped += NavigationBar_HomeTapped;
            navigationBar.ProductTapped += NavigationBar_ProductTapped;
            navigationBar.BestellenTapped += NavigationBar_BestellenTapped;
            navigationBar.AktionenTapped += NavigationBar_AktionenTapped;
            navigationBar.PunkteTapped += NavigationBar_PunkteTapped;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            GetPoints();
        }

        private async void MyPointsLogged(IAuthService authService)
        {
            try
            {
                UserRepository ur = new UserRepository(DbBootstraper.Connection);
                var user = ur.GetCurrentUser();
                if (!authService.IsLogged())
                {
                    IsLoging = false;
                    await DisplayAlert("Fehler", "Sie müssen angemeldet sein, um fortzufahren!", "OK");
                    await SafePushAsync(new LoginPage(authService));
                    return;
                }
                else
                {
                    IsLoging = true;
                    var userId = user.UserId;
                    UserQRcode.Value = userId;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fehler", $"Ein Fehler ist aufgetreten: {ex.Message}", "OK");
            }
        }

        private async void GetPoints()
        {
            if (IsLoging)
            {
                try
                {
                    UserRepository ur = new UserRepository(DbBootstraper.Connection);
                    var user = ur.GetCurrentUser();
                    var points = await _authService.GetTotalPointsByUserID(user.UserId);
                    var pointnumber = points.Replace("\"", string.Empty);
                    pointsLabel.Text = pointnumber;
                   
                }
                catch (InvalidCastException)
                {
                    await DisplayAlert("Fehler", "Es ist ein Fehler aufgetreten!", "OK");
                    return;
                }
                catch (Exception)
                {
                    await DisplayAlert("Fehler", "Es ist ein Fehler aufgetreten!", "OK");
                    return;
                }
            }
        }

        // Helper method for safe navigation
        private async Task SafePushAsync(Page page)
        {
            try
            {
                await Navigation.PushAsync(page);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fehler", $"Navigation error: {ex.Message}", "OK");
            }
        }

        // NavigationBar event handler-i

        private async void NavigationBar_HomeTapped(object sender, EventArgs e)
        {
            await SafePushAsync(new MainPage());
        }

        private async void NavigationBar_ProductTapped(object sender, EventArgs e)
        {
            await SafePushAsync(new SettingsPage());
        }

        private async void NavigationBar_BestellenTapped(object sender, EventArgs e)
        {
            await SafePushAsync(new OrderTypePage(_authService));
        }

        private async void NavigationBar_AktionenTapped(object sender, EventArgs e)
        {
            await SafePushAsync(new AktionenPage(_authService));
        }

        private async void NavigationBar_PunkteTapped(object sender, EventArgs e)
        {
            await SafePushAsync(new MyPointsPage(_authService));
        }

        // Ostatak koda ostaje nepromenjen (npr. dugmad, QR kod, itd.)

        private async void TapGestureRecognizer_Tapped_8(object sender, EventArgs e)
        {
            await SafePushAsync(new SoFunktioniert());
        }

        private async void TapGestureRecognizer_Tapped88(object sender, EventArgs e)
        {
            try
            {
                // Dohvati QR skener servis iz DI kontejnera ili DependencyService-a
                var qrService = ((App)Application.Current)
                                    .Services
                                    .GetService(typeof(IQrScannerService))
                                as IQrScannerService;
                // Prosledi oba servisa
                await Navigation.PushAsync(new SpecialProductListSWpts(_authService, qrService));

                borderJetzEinlosbar.BackgroundColor = Color.FromRgba(204, 7, 25, 255);
                await Task.Delay(100);
                borderJetzEinlosbar.BackgroundColor = Colors.Transparent;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fehler", $"Navigation error: {ex.Message}", "OK");
            }
        }

        private async void OnLabelTapped(object sender, EventArgs e)
        {
            await SafePushAsync(new BuyPoints(_authService));
        }

        private async void OnLabelTapped1(object sender, EventArgs e)
        {
            await SafePushAsync(new TransferPoints(_authService));
        }
    }
}
