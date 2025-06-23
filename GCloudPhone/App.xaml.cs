using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;

using CommunityToolkit.Mvvm.Messaging;
using GCloudPhone.Models;
using GCloudPhone.Services;
using GCloudPhone.Views;
using GCloudPhone.Views.Shop;
using GCloudShared.Interface;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Shared;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;

#if IOS
using WebKit;
using GCloudPhone.Platforms.iOS;
using Foundation;
#endif

namespace GCloudPhone
{
    public partial class App : Application
    {
        // DI – Property za servis provider; omogućava pristup registrovanim servisima
        public IServiceProvider Services { get; }

        private IAuthService _authService;

        public static string OrderType { get; set; }
        public static bool IsRepeatOrder { get; set; }  // Označava da je u pitanju ponovljena narudžbina

        public static TaskCompletionSource<bool> InitializationComplete { get; } = new TaskCompletionSource<bool>();
        public static TaskCompletionSource<bool> _paymentCompletionSource;

        public static SignalRClient SignalR { get; private set; }

        private readonly NotificationDatabaseService _notificationService;
        private DateTime _lastNotificationCheck = DateTime.MinValue;

        // Konstruktor s DI
        public App(IServiceProvider services)
        {
            // ----- REGISTRACIJA GLOBALNIH HVAĆAČA IZUZETAKA -----
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Debug.WriteLine($"[UNHANDLED EXCEPTION] {e.ExceptionObject}");
            };
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                Debug.WriteLine($"[UNOBSERVED TASK EXCEPTION] {e.Exception}");
                e.SetObserved();
            };
            // ---------------------------------------------------

            Debug.WriteLine("[App] Constructor start");
            try
            {
                InitializeComponent();
                Services = services;

#if IOS
                WebViewHandler.Mapper.Add(nameof(WKUIDelegate), WebViewHandler2.MapWKUIDelegate2);
                Debug.WriteLine("[App][iOS] WKUIDelegate mapper added");
#endif

                _authService = new AuthService();
                _notificationService = new NotificationDatabaseService();

                MainPage = new NavigationPage(new SplashScreenPage());
                Debug.WriteLine("[App] Postavljen SplashScreenPage kao MainPage");

                // Pokreni inicijalizaciju aplikacije
                InitializeAppAsync(_authService);

                UserAppTheme = AppTheme.Light;
                Debug.WriteLine("[App] Theme set to Light");

                WeakReferenceMessenger.Default.Register<PushNotificationReceived>(this, (r, m) =>
                {
                    Debug.WriteLine($"[PushNotification] Received: {m.Value}");
                });

                // Obrisati stare preference
                Preferences.Remove("UsedPoints");
                Debug.WriteLine("[App] Obrisane preference 'UsedPoints'");

                // Postavi kulturu
                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("de-DE");
                CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("de-DE");
                Debug.WriteLine("[App] Culture set to de-DE");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[App][Constructor][Error] {ex}");
                // Konstruktor ne prikazuje DisplayAlert jer MainPage možda nije postavljen
            }
            Debug.WriteLine("[App] Constructor end");
        }

        protected override async void OnStart()
        {
            Debug.WriteLine("[App] OnStart()");
            try
            {
                var savedNotifications = GetSavedNotifications();
                Debug.WriteLine($"[App][OnStart] Retrieved {savedNotifications.Count} saved notifications");
                foreach (var notification in savedNotifications)
                {
                    Debug.WriteLine($"[App][OnStart] Saved Notification: {notification}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[App][OnStart][Error] {ex}");
            }
        }

        public List<string> GetSavedNotifications()
        {
#if IOS
            Debug.WriteLine("[GetSavedNotifications][iOS] Reading from NSUserDefaults");
            try
            {
                var defaults = Foundation.NSUserDefaults.StandardUserDefaults;
                var notifications = defaults.StringForKey("saved_notifications") ?? "[]";
                Debug.WriteLine($"[GetSavedNotifications] Raw JSON: {notifications}");
                return JsonSerializer.Deserialize<List<string>>(notifications);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GetSavedNotifications][iOS][Error] {ex}");
                return new List<string>();
            }
#else
            return new List<string>();
#endif
        }

        private async void InitializeAppAsync(IAuthService authService)
        {
            Debug.WriteLine("[InitializeAppAsync] Start");
            try
            {
                var loadingPage = MainPage as NavigationPage;
                var loadingPageContent = loadingPage?.CurrentPage as SplashScreenPage;

                // Inicijalizuj vreme poslednje kontrole
                await InitializeLastNotificationCheckTime();

                bool isLoggedIn = false;
                try
                {
                    isLoggedIn = authService.IsLogged();
                }
                catch (Exception exAuth)
                {
                    Debug.WriteLine($"[InitializeAppAsync][AuthCheck][Error] {exAuth}");
                }
                Debug.WriteLine($"[InitializeAppAsync] IsLoggedIn = {isLoggedIn}");
                if (isLoggedIn)
                {
                    await CheckForNotifications();
                }

                // Uklanjanje granica iz Entry kontrola
                try
                {
                    RemoveBorderEntry.RemoveBorders();
                    Debug.WriteLine("[InitializeAppAsync] Borders removed from Entry controls");
                }
                catch (Exception exBorders)
                {
                    Debug.WriteLine($"[InitializeAppAsync][RemoveBorders][Error] {exBorders}");
                }

                // Inicijalizacija SQL baze
                try
                {
                    await GCloudPhone.SQL.InitializeAsync();
                    Debug.WriteLine("[InitializeAppAsync] SQL.InitializeAsync completed");
                }
                catch (Exception exSql)
                {
                    Debug.WriteLine($"[InitializeAppAsync][SQL.InitializeAsync][Error] {exSql}");
                }

                // Sinhronizacija podataka ako postoji internet
                if (Connectivity.NetworkAccess.HasFlag(NetworkAccess.Internet))
                {
                    Debug.WriteLine("[InitializeAppAsync] Internet access available");

                    try
                    {
                        var dataImporter = new DataImporter();
                        Debug.WriteLine("[InitializeAppAsync] Calling DataImporter.CheckForImport()");
                        await dataImporter.CheckForImport();
                        Debug.WriteLine("[InitializeAppAsync] DataImporter.CheckForImport completed");
                    }
                    catch (Exception exImport)
                    {
                        Debug.WriteLine($"[InitializeAppAsync][DataImporter.CheckForImport][Error] {exImport}");
                    }

                    try
                    {
                        var dataImport = new StartUpDataImport();
                        Debug.WriteLine("[InitializeAppAsync] Calling StartUpDataImport.ImportData()");
                        await dataImport.ImportData(authService);
                        Debug.WriteLine("[InitializeAppAsync] StartUpDataImport.ImportData completed");
                    }
                    catch (Exception exStartupImport)
                    {
                        Debug.WriteLine($"[InitializeAppAsync][StartUpDataImport.ImportData][Error] {exStartupImport}");
                    }
                }
                else
                {
                    Debug.WriteLine("[InitializeAppAsync] No internet access");
                }

                // Učitaj parametre iz baze
                List<Parameters> parameters = new List<Parameters>();
                try
                {
                    parameters = await SQL.GetParametersAsync();
                    Debug.WriteLine($"[InitializeAppAsync] Loaded {parameters.Count} parameters");
                    ParameterLoader.LoadParameters(parameters);
                }
                catch (Exception exParams)
                {
                    Debug.WriteLine($"[InitializeAppAsync][GetParametersAsync][Error] {exParams}");
                }

                // Inicijalizacija SignalR klijenta
                try
                {
                    // Pre nego što prosledimo URI u SignalRClient, proveravamo ga TryCreate
                    string rawHubUrl = "signalrtestprotic.willessen.online/chathub";
                    Debug.WriteLine($"[InitializeAppAsync] RAW hub URL: {rawHubUrl}");
                    if (Uri.TryCreate(rawHubUrl, UriKind.Absolute, out Uri hubUri) ||
                        Uri.TryCreate($"https://{rawHubUrl}", UriKind.Absolute, out hubUri))
                    {
                        Debug.WriteLine($"[InitializeAppAsync] Parsed hub URI: {hubUri}");
                        SignalR = new SignalRClient("MobilePhone1", "Kassa1234", hubUri.ToString());
                        Debug.WriteLine("[InitializeAppAsync] SignalRClient instantiated");

                        SignalR.MessageReceived += SignalR.OnMessageReceived;
                        SignalR.OnlineUsersReceived += SignalR.OnOnlineUsersReceived;
                        SignalR.UserOnline += SignalR.OnUserOnline;
                        SignalR.UserOffline += SignalR.OnUserOffline;
                        Debug.WriteLine("[InitializeAppAsync] SignalRClient event handlers attached");

                        var sendOk = await SignalR.SendMessageToUser("Kassa12", OrderTools.SerializeInfoMessage("Phone111 online"));
                        Debug.WriteLine($"[InitializeAppAsync] SignalR.SendMessageToUser returned {sendOk}");
                    }
                    else
                    {
                        Debug.WriteLine($"[InitializeAppAsync] INVALID hub URL: {rawHubUrl}");
                    }
                }
                catch (Exception exSignalR)
                {
                    Debug.WriteLine($"[InitializeAppAsync][SignalR][Error] {exSignalR}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[InitializeAppAsync][Error] {ex}");
                try
                {
                    await MainPage.DisplayAlert("Greška", "Došlo je do greške pri inicijalizaciji aplikacije.", "OK");
                }
                catch
                {
                    Debug.WriteLine("[InitializeAppAsync] DisplayAlert nije mogao biti prikazan.");
                }
            }
            finally
            {
                Debug.WriteLine("[InitializeAppAsync] End");
                // Nakon što smo pokušali inicijalizaciju, prebacujemo se na stvarnu MainPage
                try
                {
                    MainPage = new NavigationPage(new Views.MainPage());
                    Debug.WriteLine("[InitializeAppAsync] MainPage postavljen na Views.MainPage");
                }
                catch (Exception exMainPage)
                {
                    Debug.WriteLine($"[InitializeAppAsync][MainPage][Error] {exMainPage}");
                }
            }
        }

        private async Task CheckForNotifications()
        {
            Debug.WriteLine("[CheckForNotifications] Start");
            try
            {
                var ur = new UserRepository(DbBootstraper.Connection);
                var user = ur.GetCurrentUser();
                Debug.WriteLine($"[CheckForNotifications] CurrentUser: {user?.UserId}");

                if (string.IsNullOrEmpty(user?.UserId))
                {
                    Debug.WriteLine("[CheckForNotifications] UserId je prazan ili null, izlazim.");
                    return;
                }

                string formattedTimestamp = _lastNotificationCheck.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
                Debug.WriteLine($"[CheckForNotifications] Sending timestamp: {formattedTimestamp}");

                var notifications = await _authService.CheckForUserNotifications(user.UserId, _lastNotificationCheck);
                Debug.WriteLine($"[CheckForNotifications] Fetched {notifications.Count} notifications");

                DateTime mostRecentTimestamp = _lastNotificationCheck;
                foreach (var notification in notifications)
                {
                    Debug.WriteLine($"[CheckForNotifications] Notification: {notification.title} at {notification.ReceivedDateTime:o}");
                    if (notification.ReceivedDateTime > mostRecentTimestamp)
                        mostRecentTimestamp = notification.ReceivedDateTime;

                    await SaveNotification(notification);

                    MainThread.BeginInvokeOnMainThread(() =>
                        WeakReferenceMessenger.Default.Send(new PushNotificationReceived(notification.title))
                    );
                }
                _lastNotificationCheck = mostRecentTimestamp;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckForNotifications][Error] {ex}");
            }
            Debug.WriteLine("[CheckForNotifications] End");
        }

        private async Task SaveNotification(PushNotifications notification)
        {
            Debug.WriteLine("[SaveNotification] Start");
            try
            {
                var existing = await _notificationService.GetNotificationByTitleAndBodyAsync(notification.title, notification.body);
                if (existing == null)
                {
                    await _notificationService.SaveNotificationAsync(notification);
                    Debug.WriteLine($"[SaveNotification] Saved notification: {notification.title}");
                }
                else
                {
                    Debug.WriteLine($"[SaveNotification] Notification već postoji: {notification.title}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SaveNotification][Error] {ex}");
            }
            Debug.WriteLine("[SaveNotification] End");
        }

        private async Task InitializeLastNotificationCheckTime()
        {
            Debug.WriteLine("[InitializeLastNotificationCheckTime] Start");
            try
            {
                var list = await _notificationService.GetNotificationsAsync();
                Debug.WriteLine($"[InitializeLastNotificationCheckTime] Found {list.Count} saved notifications");
                _lastNotificationCheck = list.Count > 0
                    ? list[0].ReceivedDateTime.ToUniversalTime()
                    : DateTime.Now.AddDays(-1);
                Debug.WriteLine($"[InitializeLastNotificationCheckTime] _lastNotificationCheck = {_lastNotificationCheck:o}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[InitializeLastNotificationCheckTime][Error] {ex}");
                _lastNotificationCheck = DateTime.Now.AddDays(-20);
            }
            Debug.WriteLine("[InitializeLastNotificationCheckTime] End");
        }

        public static byte[] DownloadImage(string imageUrl)
        {
            Debug.WriteLine($"[DownloadImage] Start for URL: {imageUrl}");
            using var httpClient = new HttpClient();
            try
            {
                var response = httpClient.GetAsync(imageUrl).Result;
                response.EnsureSuccessStatusCode();

                var data = response.Content.ReadAsByteArrayAsync().Result;
                Debug.WriteLine($"[DownloadImage] Success: {imageUrl}, bytes={data.Length}");
                return data;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DownloadImage][Error] {imageUrl} -> {ex}");
                return null;
            }
            finally
            {
                Debug.WriteLine("[DownloadImage] End");
            }
        }
    }
}
