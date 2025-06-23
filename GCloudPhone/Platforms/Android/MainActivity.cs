using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using CommunityToolkit.Mvvm.Messaging;
using Firebase;
using GCloudPhone.Models;
//using Plugin.Firebase.CloudMessaging;
using GCloudPhone.Views.Shop;
using GCloudPhone.Views.Shop.Checkout;
using Microsoft.Maui.Controls;
/*using Microsoft.UI.Xaml;*/
using Plugin.NFC;
using System.Security;

namespace GCloudPhone
{
    [Activity(
        Theme = "@style/Maui.FullScreenSplashTheme",
        ScreenOrientation = ScreenOrientation.Locked,
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize
                             | ConfigChanges.Orientation
                             | ConfigChanges.UiMode
                             | ConfigChanges.ScreenLayout
                             | ConfigChanges.SmallestScreenSize
                             | ConfigChanges.Density)]



    // PRODUKCIJA – HTTP
    [IntentFilter(new[] { Intent.ActionView },
        Categories = new[] { Intent.ActionView, Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "http",
        DataHost = "protictest1.willessen.online",
        DataPathPrefix = "/Response/SuccessfulPayment")]
    [IntentFilter(new[] { Intent.ActionView },
        Categories = new[] { Intent.ActionView, Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "http",
        DataHost = "protictest1.willessen.online",
        DataPathPrefix = "/Response/FailedPayment")]

    // PRODUKCIJA – HTTPS
    [IntentFilter(new[] { Intent.ActionView },
        Categories = new[] { Intent.ActionView, Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "https",
        DataHost = "protictest1.willessen.online",
        DataPathPrefix = "/Response/SuccessfulPayment")]
    [IntentFilter(new[] { Intent.ActionView },
        Categories = new[] { Intent.ActionView, Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "https",
        DataHost = "protictest1.willessen.online",
        DataPathPrefix = "/Response/FailedPayment")]

    public class MainActivity : MauiAppCompatActivity
    {
        internal static readonly string Channel_ID = "TestChannel";
        internal static readonly int NotificationID = 101;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            // Plugin NFC: Initialization before base.OnCreate(...) (Important on .NET MAUI)
            CrossNFC.Init(this);

            base.OnCreate(savedInstanceState);

            if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.PostNotifications) == Permission.Denied)
            {
                ActivityCompat.RequestPermissions(
                    this,
                    new String[] { Android.Manifest.Permission.PostNotifications },
                    1);
            }

            HideBottomNavigation();
            CreateNotificationChannel();

            // Obrada deeplink URI-ja
            HandleUri(Intent?.Data);
        }

        private void HideBottomNavigation()
        {
            Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(
                SystemUiFlags.LayoutStable
              | SystemUiFlags.LayoutHideNavigation
              | SystemUiFlags.HideNavigation
              | SystemUiFlags.ImmersiveSticky);
        }

        protected override void OnPause()
        {
            base.OnPause();
            CrossNFC.Current?.StopListening();
        }

        protected override void OnResume()
        {
            base.OnResume();
            CrossNFC.OnResume();
        }

        private void HandleUri(Android.Net.Uri uri)
        {
            if (uri != null)
            {
                // Izvlačenje tokena iz poslednjeg segmenta URL-a
                string apitoken = uri.ToString()
                                     .Substring(uri.ToString().LastIndexOf('/') + 1);

                // Detekcija uspeha ili neuspeha plaćanja
                if (uri.Path.Contains("SuccessfulPayment"))
                {
                    WeakReferenceMessenger.Default
                        .Send(new NotificationItemMessage("Success"));
                }
                else if (uri.Path.Contains("FailedPayment"))
                {
                    WeakReferenceMessenger.Default
                        .Send(new NotificationItemMessage("UnsuccessfulPayment"));
                }
            }
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);

            HandleUri(intent?.Data);

            if (intent.Extras != null)
            {
                foreach (var key in intent.Extras.KeySet())
                {
                    if (key == "NavigationID")
                    {
                        string idValue = intent.Extras.GetString(key);
                        if (Preferences.ContainsKey("NavigationID"))
                            Preferences.Remove("NavigationID");
                        Preferences.Set("NavigationID", idValue);
                    }
                }
            }

            var action = intent.Action;
            var strLink = intent.DataString;
            if (Intent.ActionView == action && !string.IsNullOrWhiteSpace(strLink))
            {
                string apitoken = strLink
                    .Substring(strLink.LastIndexOf('/') + 1);
                WeakReferenceMessenger.Default
                    .Send(new NotificationItemMessage(apitoken));
            }

            // Plugin NFC: Tag Discovery Interception
            CrossNFC.OnNewIntent(intent);
        }

        private void CreateNotificationChannel()
        {
            if (OperatingSystem.IsOSPlatformVersionAtLeast("android", 26))
            {
                var channel = new NotificationChannel(
                    Channel_ID,
                    "Test Notification Channel",
                    NotificationImportance.Max);

                var notificationManager =
                    (NotificationManager)GetSystemService(
                        Android.Content.Context.NotificationService);
                notificationManager.CreateNotificationChannel(channel);
            }
        }

        public override void OnBackPressed()
        {
            // Replace the problematic line with the following:
            var mainPage = (MauiApplication.Current?.Application as App)?.MainPage ?? App.Current.MainPage;

            if (mainPage.Navigation.NavigationStack.LastOrDefault() is DeliveryCheckout currentPage)
            {
                currentPage.OnBackPressed();
            }
            else if (mainPage.Navigation.ModalStack.Count > 0)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                    await mainPage.Navigation.PopModalAsync());
            }
            else if (mainPage.Navigation.NavigationStack.Count > 1)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                    await mainPage.Navigation.PopAsync());
            }
            else
            {
                OnBackPressedDispatcher.OnBackPressed();
            }
        }
    }
}
