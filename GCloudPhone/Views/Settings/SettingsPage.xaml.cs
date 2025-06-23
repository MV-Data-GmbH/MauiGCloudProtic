using GCloudShared.Shared;
using GCloudShared.Service;
using GCloudShared.Interface;
using GCloudShared.Repository;
using GCloudPhone.Views.Shop;
using GCloudPhone.Services;
using GCloudPhone.Views.Settings.MyAccount;
using GCloudPhone.Views.Points;
using GCloudPhone.Views.Aktionen;
using GCloudPhone.Views.Shop.OrderProccess;
using GCloudPhone.Views.WebViews;
using GCloudPhone.Views.Settings.Legal;


namespace GCloudPhone.Views
{

    public partial class SettingsPage : ContentPage
    {
        private IAuthService _authService;

       
        public SettingsPage()
        {
            InitializeComponent();

            _authService = new AuthService();

            UserRepository ur = new UserRepository(DbBootstraper.Connection);
            var user = ur.GetCurrentUser();

            if (!_authService.IsLogged())
            {
                UsernameString.Text = "Sie sind nicht eingeloggt.";
                FreundesCodeText.Text = "kein einladungs-code";
                FreundesCodeText.FontSize = 16;
                lblAbmelden.IsVisible = false;
                lblDeleteUser.IsVisible = false;
            }
            else
            {
                UsernameString.Text = user.Username;
                FreundesCodeText.Text = user.invitationCode;
                if (String.IsNullOrEmpty(user.invitationCode))
                {
                    FreundesCodeText.Text = "kein einladungs-code";
                }
            }

            if (user!=null)
            {
                if(user.RoleName == "Managers")
                {
                    sendNotificationAdminButton.IsVisible = true;
                }
               
            }

            var deviceToken = Preferences.Get("DeviceToken", "");


            navigationBar.HomeTapped += NavigationBar_HomeTapped;
            navigationBar.ProductTapped += NavigationBar_ProductTapped;
            navigationBar.BestellenTapped += NavigationBar_BestellenTapped;
            navigationBar.AktionenTapped += NavigationBar_AktionenTapped;
            navigationBar.PunkteTapped += NavigationBar_PunkteTapped;
        }

        //namespace Microsoft.Maui.ApplicationModel.DataTransfer;
        private async void OnShareButtonClicked(object sender, EventArgs e)
        {

            UserRepository ur = new UserRepository(DbBootstraper.Connection);
            var user = ur.GetCurrentUser();

            if (user == null)
            {
                await DisplayAlert("Fehler", "Sie müssen angemeldet sein, um fortzufahren!", "OK");
                return;
            }

            string userName = user.FirstName;

            string invitationCode = user.invitationCode;

            if (!String.IsNullOrEmpty(user.invitationCode))
            {
                string shareBodyText = userName + " schickt dir einen Freundes-Code: " + invitationCode + " für die Anwendung: iOS: https://apps.apple.com/app/schnitzelwelt/id6468953828 oder Android: https://play.google.com/store/apps/details?id=com.mvdata.gcloudschnitzelweltV5&pcampaignid=web_share" + " Bitte diesen Code bei der Registrierung angeben.";

                await Share.RequestAsync(new ShareTextRequest
                {
                    Text = shareBodyText,
                    Title = "Share Content"
                });
            }

        }


        private void TapGestureRecognizer_Tapped_1(object sender, EventArgs e)
        {
            Navigation.PushAsync(new DatenschutzWebView());
        }
        private void TapGestureRecognizer_Tapped_2(object sender, EventArgs e)
        {
            Navigation.PushAsync(new AGBsWebView());
        }
        private void TapGestureRecognizer_Tapped_3(object sender, EventArgs e)
        {
            Navigation.PushAsync(new Impressum());
        }


        private void btnChangePassw_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new ChangePassword(_authService));
        }

        private void RechnungenClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new BillList(_authService));
        }

        private async void LogoutCalled(object sender, TappedEventArgs e)
        {

            var logout = await _authService.Logout(ParametersRepository.GetDeviceIdFromParameterTable());
            if (logout == null)
            {
                throw new NullReferenceException();
            }
            var user = logout as HttpResponseMessage;
            if (user != null)
            {
                await Navigation.PushAsync(new LoginPage(_authService));
            }
            else
            {
                await DisplayAlert("Fehler", "Beim Abmeldevorgang ist ein Fehler aufgetreten", "OK");
            }
        }


        private async void NavigationBar_HomeTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MainPage());
        }

        private async void NavigationBar_ProductTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }

        private async void NavigationBar_BestellenTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new OrderTypePage(_authService));
        }

        private async void NavigationBar_AktionenTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AktionenPage(_authService));
        }

        private async void NavigationBar_PunkteTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MyPointsPage(_authService));
        }

        private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
        {

            bool userConfirmed = await DisplayAlert("Bestätigung", "Sind Sie sicher, dass Sie Ihr Konto löschen möchten?", "Ja", "Abbrechen");

            if (userConfirmed)
            {
                UserRepository ur = new UserRepository(DbBootstraper.Connection);
                var user = ur.GetCurrentUser();
                var result = await _authService.DeleteUser(user.Email);

                if (result == 1)
                {
                    ParametersRepository.DeletedAuthToken();
                    ur.DeleteAll();
                    await Navigation.PushAsync(new RegisterPage(_authService));
                }
                else
                {
                    await DisplayAlert("Fehler", "Beim Abmeldevorgang ist ein Fehler aufgetreten", "OK");
                }

            }
        }

        private void TapGestureRecognizer_Tapped_9(object sender, TappedEventArgs e)
        {
            var qrScannerService = ((App)Application.Current).Services.GetService(typeof(IQrScannerService)) as IQrScannerService;
             Navigation.PushAsync(new BestellungInDerFiliale(_authService, qrScannerService));
        }

        private void SendNotification(object sender, EventArgs e)
        {
            Navigation.PushAsync(new SendNotificationForm(_authService));
        }

        private async void OnManageAddressesTapped(object sender, TappedEventArgs e)
        {
            await Navigation.PushAsync(new ManageAddressesPage());
        }
    }
}