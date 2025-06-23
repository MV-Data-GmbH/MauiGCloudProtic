using GCloudShared.Service;
using GCloudShared.Service.Dto;
using GCloud.Shared.Exceptions;
using GCloudShared.Interface;
using GCloudShared.Domain;
using GCloudShared.Repository;
using GCloudShared.Shared;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using GCloudPhone.Views.WebViews;
//using Microsoft.AppCenter;
//using Microsoft.AppCenter.Analytics;
//using Microsoft.AppCenter.Crashes;


namespace GCloudPhone.Views.Settings.MyAccount;

//XAML file should be compiled into a binary format during the build process,improves performance
[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class LoginPage : ContentPage
{
    private IAuthService _authService;
    public static LoginResponseModel UserLogged { get; set; }

    private string _deviceToken;

    public LoginPage(IAuthService authService)
	{
		InitializeComponent(); 
        _authService = authService;
        CheckConnection();

        if (Preferences.ContainsKey("DeviceToken"))
        {
            _deviceToken = Preferences.Get("DeviceToken", "");
        }
    }
    private async void CheckConnection()
    {
        if (!UrlConnection.CheckInternetConnection())
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            string text = "Sie sind offline";
            ToastDuration duration = ToastDuration.Short;
            double fontSize = 17;

            var toast = Toast.Make(text, duration, fontSize);

            await toast.Show(cancellationTokenSource.Token);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LoginBtn.IsEnabled = false;
                ForgotPasswordBtn.IsEnabled = false;
            });
           
            return;
        }
        else
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LoginBtn.IsEnabled = true;
                ForgotPasswordBtn.IsEnabled = true;
            });
        }
    }
    private async void OnLoginButtonClicked(object sender, EventArgs e)
    {

        string username = usernameEntry.Text;
        string password = passwordEntry.Text;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Fehler", "Bitte geben Sie sowohl E-Mail als auch Passwort ein", "OK");
            return;
        }
       
        var lg = new LoginRequestModel
        {
            Username = username,
            Password = password,
            DeviceId = _deviceToken,
            FirebaseInstanceId = _deviceToken,
        };

        try
        {
            var rel1 = await _authService.LoginService(lg);

            if (rel1 == null)
            {
                throw new NullReferenceException();
            }

            var user = rel1 as LoginResponseModel;
            if (user != null)
            {
                User u = new User();
                u.UserId= user.UserId;
                u.Username= user.Username;
                u.RoleName = user.Role;
                u.Email= user.Email;
                u.invitationCode = user.InvitationCode;
                u.Password = password;
                u.FirstName = user.FirstName;
                u.LastName = user.LastName;
              

                UserRepository ur = new UserRepository(DbBootstraper.Connection);
                ur.Insert(u);
                UserLogged = user;
                await Navigation.PushAsync(new MainPage());
            }
            else
            {
                var error = rel1 as ExceptionHandlerResult;
                if (error != null)
                {
                    await DisplayAlert("Fehler", error.Message, "OK");
                }
            }
        }
        catch (Exception)
        {
            await DisplayAlert("Fehler", "Es ist ein Fehler bei der Anmeldung aufgetreten", "OK");
        }
    }

   

    private void btnRegister_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new RegisterPage(_authService));
    }

    private void forgottenPassworButtonClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new ForgotPassword(_authService));
    }

    private void TapGestureRecognizer_Tapped_1(object sender, EventArgs e)
    {
        Navigation.PushAsync(new DatenschutzWebView());

    }
    protected override bool OnBackButtonPressed()
    {
        return true;
    }

}