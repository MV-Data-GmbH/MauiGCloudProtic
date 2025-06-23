
using GCloudShared.Interface;
using System.Net.Mail;

namespace GCloudPhone.Views.Settings.MyAccount;

public partial class ForgotPassword : ContentPage
{
    private IAuthService _authService;
    public ForgotPassword(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
        
    }
    
    private async void OnResetPasswordButtonClicked(object sender, EventArgs e)
    {
        string email = emailEntry.Text;

        if (string.IsNullOrWhiteSpace(email))
        {
            await DisplayAlert("Fehler", "Bitte geben Sie Ihre E-Mail-Adresse ein", "OK");
            return;
        }

        if (!IsValidEmail(email))
        {
            await DisplayAlert("Fehler", "Ungültiges E-Mail-Format!", "OK");
            return;
        }

        bool isPasswordResetSuccessful = await SendPasswordResetRequest(email);

        if (isPasswordResetSuccessful)
        {
            //true
            await DisplayAlert("Erfolg", "Die Anfrage zur Passwortrücksetzung wurde an Ihre E-Mail-Adresse gesendet", "OK");

            await Navigation.PushAsync(new LoginPage(_authService));
        }
        else
        {
            //error message
            await DisplayAlert("Fehler", "Fehler beim Senden der Anfrage zur Passwortrücksetzung", "OK");
        }
    }

    private async Task<bool> SendPasswordResetRequest(string email)
    {
        try
        {
            var result = await _authService.ResetPassword(email);

            if (result is bool resetSuccess)
            {
                return resetSuccess;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        { 
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    public static bool IsValidEmail(string emailaddress)
    {
        try
        {
            MailAddress m = new MailAddress(emailaddress);

            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

   

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        Navigation.PushAsync(new LoginPage(_authService));
    }
}