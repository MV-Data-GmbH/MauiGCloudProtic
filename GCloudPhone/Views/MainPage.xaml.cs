using CommunityToolkit.Mvvm.Messaging;
using GCloudPhone.Views.Shop;
using GCloudPhone.Views.Templates;
using GCloudShared.Interface;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudPhone.Models;
using GCloudPhone.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using System.Threading.Tasks;
using GCloudPhone.Views.Points;
using GCloudPhone.Views.Aktionen;
using GCloudPhone.Views.Shop.OrderProccess;
using GCloudPhone.Views.WebViews;
using GCloudPhone.Views.Settings.MyAccount;

namespace GCloudPhone.Views
{
    public partial class MainPage : ContentPage
    {
        public double PictureWidth { get; set; }
        public double PictureHeight { get; set; }
        public double DisplayDensity { get; set; }
        private IAuthService _authService;
        private System.Timers.Timer internetCheckTimer;

        public MainPage()
        {
            InitializeComponent(); // SAMO JEDNOM

            // Definišemo listu proizvoda
            var listaProizvoda = new List<ProductModel>
            {
                new ProductModel { Image = "riesenschnitzel.png", Name = "Schnitzel", Price = 12.99m },
                new ProductModel { Image = "emmentalergebacken.png", Name = "Emmentaler", Price =  8.50m },
                new ProductModel { Image = "chickencheese.png", Name = "Chickencheese", Price = 15.00m },
                new ProductModel { Image = "pljeskavica.png", Name = "Pljeskavica", Price =  9.75m }
            };

            // Dodeljujemo tu listu CollectionView-u
            productsCollection.ItemsSource = listaProizvoda;

            _authService = new AuthService();
            StartInternetCheckTimer();

            if (_authService.IsLogged())
            {
                notificationCenterView.IsVisible = true;
                LoadUnreadNotifications();
            }
            else
            {
                notificationCenterView.IsVisible = false;
            }

            // Primer slušanja poruka (ako negde u kodu šaljete PushNotificationReceived)
            WeakReferenceMessenger.Default.Register<PushNotificationReceived>(this, (r, m) =>
            {
                string msg = m.Value;
                // Globalno rukovanje notifikacijama
            });

            // Provera konekcije
            bool isOnline = Connectivity.NetworkAccess.HasFlag(NetworkAccess.Internet);

            DisplayDensity = DeviceDisplay.Current.MainDisplayInfo.Density;
            double pH = (DeviceDisplay.Current.MainDisplayInfo.Height / DisplayDensity);
            PictureWidth = (pH < 900) ?
                           (DeviceDisplay.Current.MainDisplayInfo.Width / DisplayDensity) * 0.8 :
                           (DeviceDisplay.Current.MainDisplayInfo.Width / DisplayDensity) * 0.9;

            // Pretplata na događaje NavigationBar-a
            navigationBar.HomeTapped += NavigationBar_HomeTapped;
            navigationBar.ProductTapped += NavigationBar_ProductTapped;
            navigationBar.BestellenTapped += NavigationBar_BestellenTapped;
            navigationBar.AktionenTapped += NavigationBar_AktionenTapped;
            navigationBar.PunkteTapped += NavigationBar_PunkteTapped;
        }

        private async void LoadUnreadNotifications()
        {
            var _notificationService = new NotificationDatabaseService();
            int unreadCount = await _notificationService.GetUnreadNotificationCountAsync();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                //notificationBadge.IsVisible = unreadCount > 0;
                //UnreadCountLabel.Text = unreadCount.ToString();
            });
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            if (!Connectivity.NetworkAccess.HasFlag(NetworkAccess.Internet))
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Task.Delay(1000);
                    await DisplayAlert("Fehler", "Sie sind Offline", "OK");
                });
            }

            if (_authService.IsLogged())
            {
                notificationCenterView.IsVisible = true;
                LoadUnreadNotifications();

                DateTime? expirationDate = ParametersRepository.GetAuthTokenExpirationFromParameterTable();
                if (!expirationDate.HasValue || expirationDate.Value <= DateTime.UtcNow)
                {
                    await DisplayAlert("Sitzung abgelaufen", "Ihre Sitzung ist abgelaufen. Bitte melden Sie sich erneut an.", "OK");
                    await _authService.Logout(ParametersRepository.GetDeviceIdFromParameterTable());
                    await Navigation.PushAsync(new LoginPage(_authService));
                }
            }
            else
            {
                notificationCenterView.IsVisible = false;
            }
        }

        private async Task CheckInternetConnection()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await DisplayAlert("Fehler", "Sie sind Offline", "OK");
                });
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StopInternetCheckTimer();
        }

        private void StartInternetCheckTimer()
        {
            internetCheckTimer = new System.Timers.Timer(30000);
            internetCheckTimer.Elapsed += async (sender, e) => await CheckInternetConnection();
            internetCheckTimer.Start();
        }

        private void StopInternetCheckTimer()
        {
            internetCheckTimer?.Stop();
            internetCheckTimer?.Dispose();
        }

        private void ImageButton_Clicked(object sender, EventArgs e)
        {
            if (Connectivity.NetworkAccess.HasFlag(NetworkAccess.Internet))
            {
                Navigation.PushAsync(new SettingsPage());
            }
        }

        private void ImageTapped(object sender, TappedEventArgs e)
        {
            Navigation.PushAsync(new UberUnsWebView());
        }

        private void ImageTapped2(object sender, TappedEventArgs e)
        {
            Navigation.PushAsync(new NewsWebView());
        }

        private void ImageTapped3(object sender, TappedEventArgs e)
        {
            Navigation.PushAsync(new JobsWebView());
        }

        private void ImageTapped4(object sender, TappedEventArgs e)
        {
            Navigation.PushAsync(new InstagramWebView());
        }

        private async void NotificationCenterClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NotificationCenterPage());
        }

        private void TapGestureRecognizer_Tapped_6(object sender, TappedEventArgs e)
        {
            if (Connectivity.NetworkAccess.HasFlag(NetworkAccess.Internet))
            {
                Navigation.PushAsync(new LoginPage(_authService));
            }
        }

        private async void NavigationBar_HomeTapped(object sender, EventArgs e)
        {
            // Već smo na Home, možete ostaviti prazno ili refresovati.
        }

        private async void NavigationBar_ProductTapped(object sender, EventArgs e)
        {
            if (Connectivity.NetworkAccess.HasFlag(NetworkAccess.Internet))
                await Navigation.PushAsync(new SettingsPage());
        }

        private async void NavigationBar_BestellenTapped(object sender, EventArgs e)
        {
            if (Connectivity.NetworkAccess.HasFlag(NetworkAccess.Internet))
                await Navigation.PushAsync(new OrderTypePage(_authService));
        }

        private async void NavigationBar_AktionenTapped(object sender, EventArgs e)
        {
            if (Connectivity.NetworkAccess.HasFlag(NetworkAccess.Internet))
                await Navigation.PushAsync(new AktionenPage(_authService));
        }

        private async void NavigationBar_PunkteTapped(object sender, EventArgs e)
        {
            if (Connectivity.NetworkAccess.HasFlag(NetworkAccess.Internet))
                await Navigation.PushAsync(new MyPointsPage(_authService));
        }
    }

    public class ProductModel
    {
        public string Image { get; set; }   // npr. "edit2.png"
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
