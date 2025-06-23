using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using GCloud.Shared.Exceptions;
using GCloudPhone.Views.WebViews;
using GCloudShared.Interface;
using GCloudShared.Service;
using GCloudShared.Service.Dto;
using Microsoft.Maui.Controls;

namespace GCloudPhone.Views.Settings.MyAccount
{
    public partial class RegisterPage : ContentPage
    {
        private readonly IAuthService _authService;
        public DateTime? SelectedBirthday { get; set; }
        private static readonly Random _random = new Random();

        public RegisterPage(IAuthService authService)
        {
            InitializeComponent();
            _authService = authService;
            birthdayDatePicker.IsVisible = false;
            CheckConnection();
        }

        private async void CheckConnection()
        {
            if (!UrlConnection.CheckInternetConnection())
            {
                var toast = Toast.Make("Sie sind offline", ToastDuration.Short, 17);
                await toast.Show(new CancellationTokenSource().Token);
                MainThread.BeginInvokeOnMainThread(() => RegisterBtn.IsEnabled = false);
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() => RegisterBtn.IsEnabled = true);
            }
        }

        private async void PerformUserRegistration(object sender, EventArgs e)
        {
            try
            {
                // 1) Unos i validacija
                string email = emailEntry.Text?.Trim();
                string password = passwordEntry.Text;
                string passwordAgain = passwordAgainEntry.Text;
                string freundesCode = freundesCodeEntry.Text?.Trim();
                string firstName = FirstNameEntry.Text?.Trim();
                string lastName = LastNameEntry.Text?.Trim();
                DateTime dtDefault = new DateTime(1900, 1, 1);

                if (string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(password) ||
                    string.IsNullOrWhiteSpace(firstName) ||
                    string.IsNullOrWhiteSpace(lastName))
                {
                    await DisplayAlert("Fehler", "Bitte füllen Sie alle Pflichtfelder aus!", "OK");
                    return;
                }

                if (!IsValidEmail(email))
                {
                    await DisplayAlert("Fehler", "Ungültiges E-Mail-Format!", "OK");
                    return;
                }

                if (password != passwordAgain)
                {
                    await DisplayAlert("Fehler", "Passwörter stimmen nicht überein!", "OK");
                    return;
                }

                if (password.Length <= 6)
                {
                    await DisplayAlert("Fehler", "Das Passwort muss länger als 6 Zeichen sein!", "OK");
                    return;
                }

                if (!switchAGBs.IsToggled || !switchDatenschutz.IsToggled)
                {
                    await DisplayAlert("Fehler",
                        "Bitte akzeptieren Sie die Datenschutzerklärung und AGB, um fortzufahren.",
                        "OK");
                    return;
                }

                // 2) Poziv glavnog Auth servisa
                var authModel = new RegisterRequestModel
                {
                    Username = email,
                    Email = email,
                    Password = password,
                    ConfirmPassword = passwordAgain,
                    InvitationCode = RandomString(9),
                    Birthday = SelectedBirthday ?? dtDefault,
                    FirstName = firstName,
                    LastName = lastName,
                    InvitationCodeSender = string.IsNullOrEmpty(freundesCode) ? "unknown" : freundesCode
                };

                Debug.WriteLine("[AuthService] Poziv RegisterService");
                var authRaw = await _authService.RegisterService(authModel);

                if (authRaw is string authMsg)
                {
                    Debug.WriteLine("AuthService: registracija uspela");
                    await DisplayAlert("Erfolg", authMsg, "OK");
                    await Navigation.PushAsync(new ConfirmEmailSent(_authService));
                }
                else if (authRaw is List<string> authErrors)
                {
                    Debug.WriteLine($"AuthService greške: {string.Join(", ", authErrors)}");
                    await DisplayAlert("Fehler", string.Join("\n", authErrors), "OK");
                }
                else if (authRaw is ExceptionHandlerResult authErr)
                {
                    Debug.WriteLine($"AuthService exception: {authErr.Message}");
                    await DisplayAlert("Fehler", $"Fehler: {authErr.Message}", "OK");
                }
                else
                {
                    Debug.WriteLine("AuthService neočekivan povratni tip");
                    await DisplayAlert("Fehler", "Registrierung fehlgeschlagen!", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Greška pri registraciji: {ex}");
                await DisplayAlert("Fehler", $"Unerwarteter Fehler: {ex.Message}", "OK");
            }
        }

        private static bool IsValidEmail(string email)
        {
            try { _ = new MailAddress(email); return true; }
            catch { return false; }
        }

        private void OnBirthdayDateSelected(object sender, DateChangedEventArgs e)
            => SelectedBirthday = e.NewDate;

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(_ => chars[_random.Next(chars.Length)]).ToArray());
        }

        private void OnAlreadyMemberLabelTapped(object sender, EventArgs e)
            => Navigation.PushAsync(new LoginPage(_authService));

        private void TapGestureRecognizer_Tapped_1(object sender, EventArgs e)
            => Navigation.PushAsync(new DatenschutzWebView());

        private void TapGestureRecognizer_Tapped_2(object sender, EventArgs e)
        {
            birthdayDatePicker.IsVisible = true;
            birthdayEntry.IsVisible = false;
            borderBelowLabel.IsVisible = false;
        }
    }
}
