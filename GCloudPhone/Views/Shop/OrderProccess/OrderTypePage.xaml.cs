using GCloudShared.Interface;
using GCloudShared.Repository;
using GCloudShared.Shared;
using GCloudPhone.Services;
using GCloudShared.Service;
using Microsoft.Maui.Storage;         // For Preferences
using Microsoft.Maui.Devices.Sensors;
using GCloudPhone.Views.Settings.MyAccount;  // For Geolocation

namespace GCloudPhone.Views.Shop.OrderProccess;

public partial class OrderTypePage : ContentPage
{
    private IAuthService _authService;
    private NfcService _nfcService = new NfcService();
    private IQrScannerService _qrScannerService; // Native QR scanner service

    public OrderTypePage(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
        // Retrieve native QR scanner service from dependency injection.
        _qrScannerService = ((App)Application.Current).Services.GetService(typeof(IQrScannerService)) as IQrScannerService;

        // Clear previous marker if exists.
        if (Preferences.ContainsKey("PreviousPage"))
        {
            Preferences.Remove("PreviousPage");
        }

        try
        {
            OrderTypeLogged(authService);
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Error during initialization: {ex.Message}", "OK");
        }

        if (Config.ShowFastOrder == "No")
        {
            FastOrder.IsVisible = false;
        }
        if (Config.ShowDelivery == "No")
        {
            Delivery.IsVisible = false;
        }
        if (Config.ShowPickup == "No")
        {
            Pickup.IsVisible = false;
        }
        if (Config.ShowParking == "No")
        {
            ParkAndOrder.IsVisible = false;
        }
    }

    private async void OrderTypeLogged(IAuthService authService)
    {
        try
        {
            UserRepository ur = new UserRepository(DbBootstraper.Connection);
            var user = ur.GetCurrentUser();

            if (!authService.IsLogged())
            {
                await DisplayAlert("Fehler", "Sie müssen angemeldet sein, um fortzufahren!", "OK");
                await Navigation.PushAsync(new LoginPage(authService));
                return;
            }
            else
            {
                var points = await _authService.GetTotalPointsByUserID(user.UserId);
                var pointnumber = points.Replace("\"", string.Empty);
                pointsLabel.Text = $"Sie haben {pointnumber} Punkte.";
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error in OrderTypeLogged: {ex.Message}", "OK");
        }
    }

    private async void OnDineInClicked(object sender, TappedEventArgs e)
    {
        try
        {
            App.OrderType = "DineIn";
            if (Config.ReaderType == "NFC")
            {
                await Navigation.PushAsync(new NFCReaderPage(_nfcService));
            }
            else if (Config.ReaderType == "QR")
            {
                // Pass both the authentication service and the native QR scanner service.
                await Navigation.PushAsync(new BestellungInDerFiliale(_authService, _qrScannerService));
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error in OnDineInClicked: {ex.Message}", "OK");
        }
    }

    private async void OnPickUpClicked(object sender, TappedEventArgs e)
    {
        try
        {
            App.OrderType = "PickUp";

            // 1) Ako je samo jedna filijala u bazi, snimi je u Preferences
            var stores = await SQL.GetAllStoresAsync();
            if (stores?.Count == 1)
            {
                var store = stores[0];
                Preferences.Set("SelectedStoreId", store.Id.ToString());
                Preferences.Set("SelectedStoreName", store.Name);
             
            }

            // 2) Idi na CategoriesPage
            await Navigation.PushAsync(new CategoriesPage(_authService));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error in OnPickUpClicked: {ex.Message}", "OK");
        }
    }

    private async void OnDeliveryClicked(object sender, TappedEventArgs e)
    {
        App.OrderType = "Delivery";
        try
        {
            // Obtain user's location.
            var location = await Geolocation.GetLastKnownLocationAsync();
            if (location == null)
            {
                location = await Geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.High,
                    Timeout = TimeSpan.FromSeconds(30)
                });
            }

            if (location != null)
            {
                List<Stores> stores = await SQL.GetAllStoresAsync();
                // Find the nearest store.
                var nearestStore = stores.OrderBy(store =>
                    Location.CalculateDistance(
                        new Location(store.Latitude, store.Longitude),
                        new Location(location.Latitude, location.Longitude),
                        DistanceUnits.Kilometers
                    )
                ).FirstOrDefault();

                if (nearestStore != null)
                {
                    Preferences.Set("SelectedStoreId", nearestStore.Id.ToString());
                    Preferences.Set("SelectedStoreName", nearestStore.Name);
                    await Navigation.PushAsync(new CategoriesPage());
                }
                else
                {
                    await DisplayAlert("Fehler", "Kein Geschäft in der Nähe gefunden.", "OK");
                }
            }
            else
            {
                await DisplayAlert("Fehler", "Standort konnte nicht ermittelt werden.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", $"Fehler beim Abrufen des Standorts: {ex.Message}", "OK");
        }
    }

    private async void OrderAgainButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            // Mark that OrderHistory is reached from OrderTypePage.
            Preferences.Set("PreviousPage", "OrderTypePage");
            await Navigation.PushAsync(new OrderHistory(new AuthService()));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error in OrderAgainButton_Clicked: {ex.Message}", "OK");
        }
    }

    private async void PickUpClicked(object sender, EventArgs e)
    {
        try
        {
            App.OrderType = "PickUp";

            // 1) Ako je samo jedna filijala u bazi, snimi je u Preferences
            var stores = await SQL.GetAllStoresAsync();
            if (stores?.Count == 1)
            {
                var store = stores[0];
                Preferences.Set("SelectedStoreId", store.Id.ToString());
                Preferences.Set("SelectedStoreName", store.Name);
             
            }

            // 2) Idi na CategoriesPage
            await Navigation.PushAsync(new CategoriesPage(_authService));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error in PickUpClicked: {ex.Message}", "OK");
        }
    }
    

    private async void DeliveryClicked(object sender, EventArgs e)
    {
        App.OrderType = "Delivery";
        try
        {
            var location = await Geolocation.GetLastKnownLocationAsync();
            if (location == null)
            {
                location = await Geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.High,
                    Timeout = TimeSpan.FromSeconds(30)
                });
            }

            if (location != null)
            {
                List<Stores> stores = await SQL.GetAllStoresAsync();
                var nearestStore = stores.OrderBy(store =>
                    Location.CalculateDistance(
                        new Location(store.Latitude, store.Longitude),
                        new Location(location.Latitude, location.Longitude),
                        DistanceUnits.Kilometers
                    )
                ).FirstOrDefault();

                if (nearestStore != null)
                {
                    Preferences.Set("SelectedStoreId", nearestStore.Id.ToString());
                    Preferences.Set("SelectedStoreName", nearestStore.Name);
                    await Navigation.PushAsync(new CategoriesPage());
                }
                else
                {
                    await DisplayAlert("Fehler", "Kein Geschäft in der Nähe gefunden.", "OK");
                }
            }
            else
            {
                await DisplayAlert("Fehler", "Standort konnte nicht ermittelt werden.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", $"Fehler beim Abrufen des Standorts: {ex.Message}", "OK");
        }
    }

    private async void RestaurantClicked(object sender, EventArgs e)
    {
        try
        {
            App.OrderType = "DineIn";
            if (Config.ReaderType == "NFC")
            {
                await Navigation.PushAsync(new NFCReaderPage(_nfcService));
            }
            else if (Config.ReaderType == "QR")
            {
                await Navigation.PushAsync(new BestellungInDerFiliale(_authService, _qrScannerService));
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error in RestaurantClicked: {ex.Message}", "OK");
        }
    }

    private async void OnParkingClicked(object sender, TappedEventArgs e)
    {
        try
        {
            App.OrderType = "Parking";
            await Navigation.PushAsync(new BestellungInDerFiliale(_authService, _qrScannerService));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error in ParkingClicked: {ex.Message}", "OK");
        }
    }

    private async void ParkingClicked(object sender, EventArgs e)
    {
        try
        {
            App.OrderType = "Parking";
            await Navigation.PushAsync(new BestellungInDerFiliale(_authService, _qrScannerService));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error in ParkingClicked: {ex.Message}", "OK");
        }
    }
}
