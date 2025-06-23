//using Firebase.CloudMessaging;
using Foundation;
using GCloudPhone.Views.Shop;
using UIKit;
using UserNotifications;
using CommunityToolkit.Mvvm.Messaging;
using GCloudPhone.Platforms.iOS;
using GCloudPhone.Models;
using GCloudPhone.Services;
using GCloudPhone.Views.Shop.Checkout;
using GCloudPhone.Views.Points;


namespace GCloudPhone
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate /*IMessagingDelegate*/
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
       

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            /*Firebase.Core.App.Configure()*/;
            WeakReferenceMessenger.Default.Register<PushNotificationReceived>(this, (r, m) =>
            {
                string msg = m.Value;
                
            });
            UNUserNotificationCenter.Current.Delegate = new UserNotificationCenterDelegate();
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                var authOption = UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound;

                UNUserNotificationCenter.Current.RequestAuthorization(authOption, (granted, error) =>
                {

                });

                UNUserNotificationCenter.Current.Delegate = new UserNotificationCenterDelegate();

                //Messaging.SharedInstance.AutoInitEnabled = true;
                //Messaging.SharedInstance.Delegate = this;
            }
            UIApplication.SharedApplication.RegisterForRemoteNotifications();

            return base.FinishedLaunching(application, launchOptions);
        }

        [Export("messaging:didReceiveRegistrationToken:")]
        //public void DidReceiveRegistrationToken(Messaging message, string regToken)
        //{
        //    if (Preferences.ContainsKey("DeviceToken"))
        //    {
        //        Preferences.Remove("DeviceToken");
        //    }
        //    Preferences.Set("DeviceToken", regToken);
        //}
      

        public override bool ContinueUserActivity(UIApplication application, NSUserActivity userActivity, UIApplicationRestorationHandler completionHandler)
        {
            if (userActivity != null)
            {
                string URL = userActivity.WebPageUrl?.ToString();
                string apitoken = URL.Substring(URL.LastIndexOf('/') + 1);
              
                if (URL.Contains("SuccessfulPayment"))
                {
                    // Notify the app about successful payment
                    App._paymentCompletionSource?.TrySetResult(true);

                    if (App.Current.MainPage is NavigationPage navigationPage)
                    {
                        if (navigationPage.CurrentPage is DeliveryCheckout deliveryPage)
                        {
                            MainThread.BeginInvokeOnMainThread(async () =>
                            {
                                await deliveryPage.ProcessOrderExternally();
                            });
                        }
                        else if (navigationPage.CurrentPage is PickupCheckout pickupPage)
                        {
                            MainThread.BeginInvokeOnMainThread(async () =>
                            {
                                await pickupPage.ProcessOrderExternally();
                            });
                        }
                        else if (navigationPage.CurrentPage is BuyPoints buyPointsPage)
                        {
                            MainThread.BeginInvokeOnMainThread(async () =>
                            {
                                await buyPointsPage.ProcessPaymentExternally();
                            });
                        }
                    }
                }
                else if (URL.Contains("FailedPayment"))
                {
                    // Notify the app about failed payment
                    App._paymentCompletionSource?.TrySetResult(false);
                }

            }
            return true;
        }

       

       
    }
}
