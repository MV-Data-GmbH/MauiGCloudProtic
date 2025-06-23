using GCloud.Shared.Exceptions;
using GCloudShared.Interface;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Service.Dto;
using GCloudShared.Shared;

namespace GCloudPhone.Views.Settings.MyAccount;

public partial class ChangePassword : ContentPage
{
   
    private IAuthService _authService;
    private IWebShopService webShopService;

    public ChangePassword(IAuthService authService)
	{
		InitializeComponent();
        _authService = authService;
        webShopService = new WebShopService();
        ChangePasswordLogged(authService);
    }
    private async void ChangePasswordLogged(IAuthService authService)
    {
        if (!authService.IsLogged())
        {
           await DisplayAlert("Fehler", "Sie müssen angemeldet sein, um fortzufahren!", "OK");
            await Navigation.PushAsync(new LoginPage(authService));
            return;
        }
    }
    private async void OnChangePasswordButtonClicked(object sender, EventArgs e)
    {
        string oldPassword = AltesPasswordEntry.Text;
        string newPassword = NeuesPasswordEntry.Text;
        string confirmPassword = NeuesPassword2Entry.Text;

        if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
        {
            await DisplayAlert("Fehler", "Bitte füllen Sie alle Felder aus", "OK");
            return;
        }

        if (newPassword.Length <= 6)
        {
            await DisplayAlert("Fehler", "Das Passwort muss länger als 6 Zeichen sein!", "OK");
            return;
        }

        if (newPassword != confirmPassword)
        {
            await DisplayAlert("Fehler", "Das neue Passwort und das Bestätigungspasswort stimmen nicht überein", "OK");
            return;
        }

        UserRepository ur = new UserRepository(DbBootstraper.Connection);       
        var user = ur.GetCurrentUser();

        ChangePasswordRequestModel requestModel = new ChangePasswordRequestModel
        {
            OldPassword = oldPassword,
            NewPassword = newPassword,
            ConfirmPassword = confirmPassword,
        };

        try
        {
            var rel1 = await _authService.ChangePassword(requestModel);
            if (rel1 == null)
            {
                throw new NullReferenceException();
            }

            var change = rel1 as string;
            if (change != null)
            {
                RecoveryPasswordToWebShopModel webShopModel = new RecoveryPasswordToWebShopModel
                {
                    Email = user.Email,
                    NewPassword = newPassword,
                    Result = null
                };

                var rel2 = await webShopService.ResetPasswordInWebShopFromGcloud(webShopModel);
                if (rel2 != null)
                {
                    await DisplayAlert("Erfolg", "Passwort erfolgreich geändert", "OK");
                    await Navigation.PushAsync(new LoginPage(_authService));
                }
                else
                {
                    await DisplayAlert("Fehler", "Es ist ein Fehler bei der Passwortänderung im WebShop aufgetreten", "OK");
                }
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
            await DisplayAlert("Fehler", "Es ist ein Fehler bei der Passwortänderung aufgetreten", "OK");
        }


    }

   

    private void btnPasswortZurück_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new ForgotPassword(_authService));
    }

   

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new SettingsPage());
    }
}