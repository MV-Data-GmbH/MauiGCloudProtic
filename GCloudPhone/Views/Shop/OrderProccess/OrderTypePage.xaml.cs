using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Views;
using GCloudShared.Interface;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Shared;
using GCloudPhone.Models;               // CheckoutType enum
using GCloudPhone.Services;
using GCloudPhone.Views.Settings.MyAccount;  // za Geolocation
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Storage;          // za Preferences

namespace GCloudPhone.Views.Shop.OrderProccess
{
    public partial class OrderTypePage : ContentPage
    {
        private readonly IAuthService _authService;
        private readonly NfcService _nfcService = new();
        private readonly IQrScannerService _qrScannerService;

        public OrderTypePage(IAuthService authService)
        {
            InitializeComponent();
            Debug.WriteLine("[OrderTypePage] ctor: Starting initialization");

            _authService = authService;
            _qrScannerService = ((App)Application.Current)
                .Services.GetService(typeof(IQrScannerService))
                as IQrScannerService;

            // očisti prethodni marker
            if (Preferences.ContainsKey("PreviousPage"))
            {
                Preferences.Remove("PreviousPage");
                Debug.WriteLine("[OrderTypePage] ctor: Cleared PreviousPage");
            }

            // sakrij/prikaži opcije prema konfiguraciji
            FastOrder.IsVisible = Config.ShowFastOrder != "No";
            Delivery.IsVisible = Config.ShowDelivery != "No";
            Pickup.IsVisible = Config.ShowPickup != "No";
            ParkAndOrder.IsVisible = Config.ShowParking != "No";
            Debug.WriteLine($"[OrderTypePage] ctor: Config – ShowFastOrder={Config.ShowFastOrder}, ShowDelivery={Config.ShowDelivery}, ShowPickup={Config.ShowPickup}, ShowParking={Config.ShowParking}");
            Debug.WriteLine($"[OrderTypePage] ctor: Visibility – FastOrder={FastOrder.IsVisible}, Delivery={Delivery.IsVisible}, Pickup={Pickup.IsVisible}, ParkAndOrder={ParkAndOrder.IsVisible}");

            OrderTypeLogged();
            Debug.WriteLine("[OrderTypePage] ctor: Initialization complete");
        }

        private async void OrderTypeLogged()
        {
            try
            {
                var ur = new UserRepository(DbBootstraper.Connection);
                var user = ur.GetCurrentUser();

                if (!_authService.IsLogged())
                {
                    await DisplayAlert("Fehler", "Sie müssen angemeldet sein, um fortzufahren!", "OK");
                    await Navigation.PushAsync(new LoginPage(_authService));
                    return;
                }

                var points = await _authService.GetTotalPointsByUserID(user.UserId);
                // points je string, Replace vraća novi string
                var pointnumber = points.Replace("\"", string.Empty);
                pointsLabel.Text = $"Sie haben {pointnumber} Punkte.";
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error in OrderTypeLogged: {ex.Message}", "OK");
            }
        }


        private async void OnDineInClicked(object sender, EventArgs e)
        {
            Debug.WriteLine("[OrderTypePage] OnDineInClicked: handler start");
            try
            {
                App.OrderType = "DineIn";
                Preferences.Set("CheckoutMode", CheckoutType.InStore.ToString());
                Debug.WriteLine($"[OrderTypePage] OnDineInClicked: App.OrderType={App.OrderType}, CheckoutMode={CheckoutType.InStore}");

                Debug.WriteLine($"[OrderTypePage] OnDineInClicked: Config.ReaderType={Config.ReaderType}");
                if (Config.ReaderType == "NFC")
                {
                    Debug.WriteLine("[OrderTypePage] OnDineInClicked: Navigating to NFCReaderPage");
                    await Navigation.PushAsync(new NFCReaderPage(_nfcService));
                }
                else
                {
                    Debug.WriteLine("[OrderTypePage] OnDineInClicked: Navigating to BestellungInDerFiliale");
                    await Navigation.PushAsync(new BestellungInDerFiliale(_authService, _qrScannerService));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OrderTypePage] OnDineInClicked error: {ex}");
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void OnPickUpClicked(object sender, EventArgs e)
        {
            Debug.WriteLine("[OrderTypePage] OnPickUpClicked: handler start");
            try
            {
                App.OrderType = "PickUp";
                Preferences.Set("CheckoutMode", CheckoutType.Pickup.ToString());
                Debug.WriteLine($"[OrderTypePage] OnPickUpClicked: App.OrderType={App.OrderType}, CheckoutMode={CheckoutType.Pickup}");

                var stores = await SQL.GetAllStoresAsync();
                Debug.WriteLine($"[OrderTypePage] OnPickUpClicked: stores count={(stores?.Count ?? 0)}");
                if (stores?.Count == 1)
                {
                    var s = stores[0];
                    Preferences.Set("SelectedStoreId", s.Id.ToString());
                    Preferences.Set("SelectedStoreName", s.Name);
                    Debug.WriteLine($"[OrderTypePage] OnPickUpClicked: only one store – Id={s.Id}, Name={s.Name}");
                }

                Debug.WriteLine("[OrderTypePage] OnPickUpClicked: Navigating to CategoriesPage");
                await Navigation.PushAsync(new CategoriesPage(_authService));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OrderTypePage] OnPickUpClicked error: {ex}");
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void OnDeliveryClicked(object sender, EventArgs e)
        {
            Debug.WriteLine("[OrderTypePage] OnDeliveryClicked: handler start");
            try
            {
                App.OrderType = "Delivery";
                Preferences.Set("CheckoutMode", CheckoutType.Delivery.ToString());
                Debug.WriteLine($"[OrderTypePage] OnDeliveryClicked: App.OrderType={App.OrderType}, CheckoutMode={CheckoutType.Delivery}");

                var location = await Geolocation.GetLastKnownLocationAsync()
                              ?? await Geolocation.GetLocationAsync(new GeolocationRequest
                              {
                                  DesiredAccuracy = GeolocationAccuracy.High,
                                  Timeout = TimeSpan.FromSeconds(30)
                              });
                if (location == null)
                {
                    Debug.WriteLine("[OrderTypePage] OnDeliveryClicked: Location == null");
                    await DisplayAlert("Fehler", "Standort konnte nicht ermittelt werden.", "OK");
                    return;
                }
                Debug.WriteLine($"[OrderTypePage] OnDeliveryClicked: Location lat={location.Latitude}, lon={location.Longitude}");

                var stores = await SQL.GetAllStoresAsync();
                Debug.WriteLine($"[OrderTypePage] OnDeliveryClicked: stores count={(stores?.Count ?? 0)}");
                var nearest = stores
                    .OrderBy(s => Location.CalculateDistance(
                        new Location(s.Latitude, s.Longitude),
                        new Location(location.Latitude, location.Longitude),
                        DistanceUnits.Kilometers))
                    .FirstOrDefault();

                if (nearest != null)
                {
                    Preferences.Set("SelectedStoreId", nearest.Id.ToString());
                    Preferences.Set("SelectedStoreName", nearest.Name);
                    Debug.WriteLine($"[OrderTypePage] OnDeliveryClicked: nearest store Id={nearest.Id}, Name={nearest.Name}");
                    Debug.WriteLine("[OrderTypePage] OnDeliveryClicked: Navigating to CategoriesPage");
                    await Navigation.PushAsync(new CategoriesPage(_authService));
                }
                else
                {
                    Debug.WriteLine("[OrderTypePage] OnDeliveryClicked: No nearby store found");
                    await DisplayAlert("Fehler", "Kein Geschäft in der Nähe gefunden.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OrderTypePage] OnDeliveryClicked error: {ex}");
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void OnParkingClicked(object sender, EventArgs e)
        {
            Debug.WriteLine("[OrderTypePage] OnParkingClicked: handler start");
            try
            {
                App.OrderType = "Parking";
                Preferences.Set("CheckoutMode", CheckoutType.InStore.ToString());
                Debug.WriteLine($"[OrderTypePage] OnParkingClicked: App.OrderType={App.OrderType}, CheckoutMode={CheckoutType.InStore}");

                Debug.WriteLine("[OrderTypePage] OnParkingClicked: Navigating to BestellungInDerFiliale");
                await Navigation.PushAsync(new BestellungInDerFiliale(_authService, _qrScannerService));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OrderTypePage] OnParkingClicked error: {ex}");
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void OrderAgainButton_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("[OrderTypePage] OrderAgainButton_Clicked: handler start");
            try
            {
                Preferences.Set("PreviousPage", "OrderTypePage");
                Debug.WriteLine("[OrderTypePage] OrderAgainButton_Clicked: Set PreviousPage=OrderTypePage");
                await Navigation.PushAsync(new OrderHistory(new AuthService()));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OrderTypePage] OrderAgainButton_Clicked error: {ex}");
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        // Duplikatni handler-i za slučaj ako ih koristiš umesto OnPickUp/OnDelivery
        private async void PickUpClicked(object sender, EventArgs e)
        {
            Debug.WriteLine("[OrderTypePage] PickUpClicked: handler start");
            try
            {
                App.OrderType = "PickUp";
                Debug.WriteLine("[OrderTypePage] PickUpClicked: App.OrderType=PickUp");

                var stores = await SQL.GetAllStoresAsync();
                Debug.WriteLine($"[OrderTypePage] PickUpClicked: stores count={(stores?.Count ?? 0)}");
                if (stores?.Count == 1)
                {
                    var store = stores[0];
                    Preferences.Set("SelectedStoreId", store.Id.ToString());
                    Preferences.Set("SelectedStoreName", store.Name);
                    Preferences.Set("CheckoutMode", CheckoutType.Pickup.ToString());
                    Debug.WriteLine($"[OrderTypePage] PickUpClicked: Only one store – Id={store.Id}, Name={store.Name}");
                }

                Debug.WriteLine("[OrderTypePage] PickUpClicked: Navigating to CategoriesPage");
                await Navigation.PushAsync(new CategoriesPage(_authService));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OrderTypePage] PickUpClicked error: {ex}");
                await DisplayAlert("Error", $"Error in PickUpClicked: {ex.Message}", "OK");
            }
        }

        private async void DeliveryClicked(object sender, EventArgs e)
        {
            Debug.WriteLine("[OrderTypePage] DeliveryClicked: handler start");
            App.OrderType = "Delivery";
            try
            {
                Debug.WriteLine("[OrderTypePage] DeliveryClicked: Getting last known location");
                var location = await Geolocation.GetLastKnownLocationAsync();
                if (location == null)
                {
                    Debug.WriteLine("[OrderTypePage] DeliveryClicked: LastKnownLocation null – requesting fresh location");
                    location = await Geolocation.GetLocationAsync(new GeolocationRequest
                    {
                        DesiredAccuracy = GeolocationAccuracy.High,
                        Timeout = TimeSpan.FromSeconds(30)
                    });
                }

                if (location != null)
                {
                    Debug.WriteLine($"[OrderTypePage] DeliveryClicked: Location lat={location.Latitude}, lon={location.Longitude}");
                    List<Stores> stores = await SQL.GetAllStoresAsync();
                    Debug.WriteLine($"[OrderTypePage] DeliveryClicked: stores count={(stores?.Count ?? 0)}");

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
                        Preferences.Set("CheckoutMode", CheckoutType.Delivery.ToString());
                        Debug.WriteLine($"[OrderTypePage] DeliveryClicked: nearest store Id={nearestStore.Id}, Name={nearestStore.Name}");
                        Debug.WriteLine("[OrderTypePage] DeliveryClicked: Navigating to CategoriesPage");
                        await Navigation.PushAsync(new CategoriesPage());
                    }
                    else
                    {
                        Debug.WriteLine("[OrderTypePage] DeliveryClicked: No nearby store found");
                        await DisplayAlert("Fehler", "Kein Geschäft in der Nähe gefunden.", "OK");
                    }
                }
                else
                {
                    Debug.WriteLine("[OrderTypePage] DeliveryClicked: Location still null");
                    await DisplayAlert("Fehler", "Standort konnte nicht ermittelt werden.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OrderTypePage] DeliveryClicked error: {ex}");
                await DisplayAlert("Fehler", $"Fehler beim Abrufen des Standorts: {ex.Message}", "OK");
            }
        }

        private async void RestaurantClicked(object sender, EventArgs e)
        {
            Debug.WriteLine("[OrderTypePage] RestaurantClicked: handler start");
            try
            {
                App.OrderType = "DineIn";
                Preferences.Set("CheckoutMode", CheckoutType.InStore.ToString());
                Debug.WriteLine($"[OrderTypePage] RestaurantClicked: App.OrderType={App.OrderType}, CheckoutMode={CheckoutType.InStore}");

                if (Config.ReaderType == "NFC")
                {
                    Debug.WriteLine("[OrderTypePage] RestaurantClicked: Navigating to NFCReaderPage");
                    await Navigation.PushAsync(new NFCReaderPage(_nfcService));
                }
                else if (Config.ReaderType == "QR")
                {
                    Debug.WriteLine("[OrderTypePage] RestaurantClicked: Navigating to BestellungInDerFiliale");
                    await Navigation.PushAsync(new BestellungInDerFiliale(_authService, _qrScannerService));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OrderTypePage] RestaurantClicked error: {ex}");
                await DisplayAlert("Error", $"Error in RestaurantClicked: {ex.Message}", "OK");
            }
        }

        private async void ParkingClicked(object sender, EventArgs e)
        {
            Debug.WriteLine("[OrderTypePage] ParkingClicked: handler start");
            try
            {
                App.OrderType = "Parking";
                Preferences.Set("CheckoutMode", CheckoutType.InStore.ToString());
                Debug.WriteLine($"[OrderTypePage] ParkingClicked: App.OrderType={App.OrderType}, CheckoutMode={CheckoutType.InStore}");
                Debug.WriteLine("[OrderTypePage] ParkingClicked: Navigating to BestellungInDerFiliale");
                await Navigation.PushAsync(new BestellungInDerFiliale(_authService, _qrScannerService));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OrderTypePage] ParkingClicked error: {ex}");
                await DisplayAlert("Error", $"Error in ParkingClicked: {ex.Message}", "OK");
            }
        }
    }
}
