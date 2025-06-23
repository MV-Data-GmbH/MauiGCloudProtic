using FirebaseAdmin.Messaging;
using GCloudPhone.Views.Shop;
using GCloudShared.Interface;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Shared;
using Newtonsoft.Json;
using CommunityToolkit.Maui.Core.Platform;
using GCloudPhone.Services;
using GCloudPhone.Views.Aktionen;
using GCloudPhone.Views.Points;
using GCloudPhone.Views.Settings.MyAccount;
using GCloudPhone.Views.Shop.OrderProccess;


namespace GCloudPhone.Views.Points;
public partial class TransferPoints : ContentPage
{
    private IAuthService _authService;
    private UserPointsService _userPointsService; 
    private bool IsLoging;
    private int userTotalPoints;
    public TransferPoints(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
        _userPointsService = new UserPointsService(); 
        MyPointsLogged(authService);
        GetPoints();
    }
    private async void MyPointsLogged(IAuthService authService)
    {
        UserRepository ur = new UserRepository(DbBootstraper.Connection);
        var user = ur.GetCurrentUser();
        if (!authService.IsLogged())
        {
            IsLoging = false;
            await DisplayAlert("Fehler", "Sie müssen angemeldet sein, um fortzufahren!", "OK");
            await Navigation.PushAsync(new LoginPage(authService));
            return;
        }
        else
        {
            IsLoging = true;
            var userId = user.UserId;
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
                var pointnumber = points.Replace("\"", String.Empty);

                if (int.TryParse(pointnumber, out int totalPoints))
                {
                    userTotalPoints = totalPoints; 
                    pointsLabel.Text = pointnumber;
                }
                else
                {
                    await DisplayAlert("Fehler", "Punkte konnten nicht abgerufen werden.", "OK");
                }
            }
            catch (Exception)
            {
                await DisplayAlert("Fehler", "Es ist ein Fehler aufgetreten!", "OK");
                return;
            }
        }
    }

    private async void OnConfirmClicked(object sender, EventArgs e)
    {
        if (!IsLoging)
        {
            await DisplayAlert("Fehler", "Sie müssen angemeldet sein, um fortzufahren!", "OK");
            return;
        }

        string emailReceiver = emailEntry.Text;
        string pointsToTransferText = pointsEntry.Text;

        if (string.IsNullOrEmpty(emailReceiver) || !emailReceiver.Contains("@"))
        {
            await DisplayAlert("Fehler", "Bitte geben Sie eine gültige E-Mail-Adresse ein.", "OK");
            return;
        }

        if (!int.TryParse(pointsToTransferText, out int pointsToTransfer) || pointsToTransfer <= 0)
        {
            await DisplayAlert("Fehler", "Bitte geben Sie eine gültige Anzahl von Punkten ein.", "OK");
            return;
        }

        if (pointsToTransfer > userTotalPoints)
        {
            await DisplayAlert("Fehler", "Sie haben nicht genügend Punkte für diese Übertragung.", "OK");
            return;
        }

        UserRepository ur = new UserRepository(DbBootstraper.Connection);
        var user = ur.GetCurrentUser();
        string userIdSender = user.UserId;

        string resultMessage = await _userPointsService.TransferPointsToUser(userIdSender, emailReceiver, pointsToTransfer);

        if (resultMessage.Contains("Points transferred"))
        {
            await DisplayAlert("Erfolg", "Punkte erfolgreich übertragen.", "OK");
            emailEntry.Text = string.Empty;
            pointsEntry.Text = string.Empty;
            GetPoints();
        }
        else if (resultMessage.Contains("User not found"))
        {
            await DisplayAlert("Fehler", "Benutzer mit dieser E-Mail-Adresse nicht gefunden.", "OK");

        }
        else
        {
            await DisplayAlert("Fehler", resultMessage, "OK");
        }
    }

    private void OnBackgroundTapped(object sender, EventArgs e)
    {
        // Unfocus the Entry when tapping anywhere on the background
        pointsEntry.Unfocus();
        emailEntry.Unfocus();
    }

    private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
    {
        Navigation.PushAsync(new MyPointsPage(_authService));
    }
    private void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    {
        Navigation.PushAsync(new AktionenPage(_authService));
    }
    private void TapGestureRecognizer_Tapped_2(object sender, TappedEventArgs e)
    {
        Navigation.PushAsync(new OrderTypePage(_authService));
    }
    private void TapGestureRecognizer_Tapped_3(object sender, TappedEventArgs e)
    {
        Navigation.PushAsync(new ProduktePage(_authService));
    }
    private void TapGestureRecognizer_Tapped_4(object sender, TappedEventArgs e)
    {
        Navigation.PushAsync(new MainPage());
    }
    private void TapGestureRecognizer_Tapped_5(object sender, TappedEventArgs e)
    {
        var qrScannerService = ((App)Application.Current).Services.GetService(typeof(IQrScannerService)) as IQrScannerService;
        Navigation.PushAsync(new BestellungInDerFiliale(_authService, qrScannerService));
    }
}










