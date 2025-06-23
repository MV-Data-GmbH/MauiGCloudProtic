using GCloudPhone.Models;
using GCloudPhone.Services;
using GCloudPhone.Views.Aktionen;
using GCloudPhone.Views.Shop;
using GCloudPhone.Views.Shop.OrderProccess;
using GCloudShared.Interface;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Shared;
using Newtonsoft.Json;

namespace GCloudPhone.Views.Points;
public partial class BuyPoints : ContentPage
{
    private IAuthService _authService;
    private readonly UserPointsService _userPointsService;
    private readonly PaymentService _paymentService;
    private string selectedPaymentMethod;
    private readonly ApiService _apiService;
    private TaskCompletionSource<bool> _paymentCompletionSource;
    string orderId = Guid.NewGuid().ToString();
    private readonly OrderCommunicationService _orderCommunicationService;

    public BuyPoints(IAuthService authService)
    {
        
        InitializeComponent();
        _userPointsService = new UserPointsService();
        _authService = authService;
        _paymentService = new PaymentService();
        _apiService = new ApiService();
        _orderCommunicationService = new OrderCommunicationService();

       
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        SetPaymentMethods();
    }

    private void SetPaymentMethods()
    {
        PaymentMethodPicker.Items.Clear();

#if IOS
        PaymentMethodPicker.Items.Add("Apple Pay");
#endif

#if ANDROID
        PaymentMethodPicker.Items.Add("Google Pay");
#endif
        PaymentMethodPicker.Items.Add("Kreditkarte");
    }

    private void OnPaymentMethodChanged(object sender, EventArgs e)
    {
        var picker = sender as Picker;
        if (picker != null && picker.SelectedItem != null)
        {
            selectedPaymentMethod = picker.SelectedItem.ToString();
        }
    }



    private async void OnConfirmClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(selectedPaymentMethod) || string.IsNullOrWhiteSpace(pointsEntry.Text))
        {
            await DisplayAlert("Fehler", "Bitte wählen Sie eine Zahlungsmethode und die Anzahl der Punkte aus, die Sie kaufen möchten.", "OK");
            return;
        }

       
        decimal decimalValue;
        if (decimal.TryParse(pointsEntry.Text, out decimalValue))
        {
            // Successful conversion
            Console.WriteLine($"Converted value: {decimalValue}");
        }
        else
        {
            await DisplayAlert("Fehler", "Bitte geben Sie eine gültige Anzahl von Punkten ein.", "OK");
            return
                ;
        }

        bool paymentSuccess = await _paymentService.ProcessPaymentAsync(selectedPaymentMethod, decimalValue, this, orderId);

        if (paymentSuccess)
        {
            await ProcessPaymentExternally();
        }
        else
        {
            await DisplayAlert("Zahlung fehlgeschlagen", "Ihre Zahlung war nicht erfolgreich. Bitte versuchen Sie es erneut.", "OK");
        }

    }
  
    public async Task ProcessPaymentExternally()
    {
        UserRepository ur = new UserRepository(DbBootstraper.Connection);
        var user = ur.GetCurrentUser();
        string userId = user.UserId;

        string storeId = "EDC9A387-A963-EB11-8EF6-48F17F295823";
        if (int.TryParse(pointsEntry.Text, out int pointsAmount))
        {
            var resultMessage = await _userPointsService.BuyPoints(userId, pointsAmount, storeId);

            if (resultMessage.Contains("Points bought"))
            {
                await DisplayAlert("Erfolg", "Die Punkte wurden erfolgreich bezahlt.", "OK");
                //slanje ordera
                await SendOrderToServer();
            }
            else
            {    
                Console.WriteLine(resultMessage);
                await DisplayAlert("Fehler", "Es ist ein Fehler aufgetreten!", "OK");
            }
        }
        else
        {
            await DisplayAlert("Eingabefehler", "Bitte geben Sie eine gültige Anzahl an Punkten ein.", "OK");
        }
        await Navigation.PopAsync();
    }

    public async Task SendOrderToServer()
    {
        UserRepository ur = new UserRepository(DbBootstraper.Connection);
        var user = ur.GetCurrentUser();
        string userId = user.UserId;

        var storeId = Preferences.Get("SelectedStoreId", string.Empty);

        decimal totalAmount = 0;

        if (decimal.TryParse(pointsEntry.Text, out decimal parsedAmount))
        {
            totalAmount = parsedAmount;
        }
        else
        {
            await DisplayAlert("Eingabefehler", "Bitte geben Sie einen gültigen Betrag ein.", "OK");
            return; 
        }

        var order = new Orders
        {
            OrderID = orderId,
            UserID = user.UserId,
            StoreID = storeId,
            OrderDate = DateTime.Now,
            DeliveryDate = DateTime.Now.AddHours(1),
            DeliveryTime = "",
            DeliveryAddress = "",
            DeliveryCity = "",
            DeliveryZip = "",
            DeliveryCountry = "Austria",
            DeliveryPhone = "123456789",
            DeliveryEmail = user.Email,
            DeliveryContact = "",
            DeliveryNotes = "",
            PaymentMethod = "",
            PaymentStatus = "Done",
            OrderStatus = "Processed",
            TotalAmount = totalAmount, 
            TotalVAT = 0,
            OrderType = "Buying Points",
            Reference = "",
            Tip = 0
        };

        await SQL.SaveItemAsync(order);

        await _orderCommunicationService.SendOrderToServerAsync(order, new List<OrderItems>());
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
}