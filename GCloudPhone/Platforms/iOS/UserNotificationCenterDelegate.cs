using CommunityToolkit.Mvvm.Messaging;
using CoreData;
using Foundation;
using GCloudPhone.Models;
using GCloudPhone.Services;
using System.Text.Json;
using UIKit;
using UserNotifications;

namespace GCloudPhone.Platforms.iOS
{
    public class UserNotificationCenterDelegate : UNUserNotificationCenterDelegate
    {
        private readonly NotificationDatabaseService _notificationService = new NotificationDatabaseService();

        // Save notification as UNREAD when received
        public override async void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
        {
            var userInfo = notification.Request.Content.UserInfo;

            string title = notification.Request.Content.Title;
            string body = notification.Request.Content.Body;

            var newNotification = new PushNotifications
            {
                title = title,
                body = body,
                ReceivedDateTime = DateTime.Now,
                IsRead = false
            };

            await _notificationService.SaveNotificationAsync(newNotification);

            // Show the notification banner
            completionHandler(UNNotificationPresentationOptions.Banner);
        }


        // Mark notification as READ when clicked
        [Export("userNotificationCenter:didReceiveNotificationResponse:withCompletionHanger:")]
        public override async void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
        {
            var userInfo = response.Notification.Request.Content.UserInfo;

            string title = response.Notification.Request.Content.Title;
            string body = response.Notification.Request.Content.Body;

            string navigationID = userInfo.ContainsKey(new NSString("NavigationID"))
                ? userInfo["NavigationID"].ToString()
                : string.Empty;

            // Find the notification in the database by title & body (since ID is now auto-incremented)
            var notification = await _notificationService.GetNotificationByTitleAndBodyAsync(title, body);

            if (notification != null)
            {
                await _notificationService.MarkNotificationAsRead(notification.Id);
            }

            // Refresh UI
            WeakReferenceMessenger.Default.Send(new PushNotificationReceived(body));

            Preferences.Set("NavigationID", navigationID);
            completionHandler();
        }



        [Export("application:didReceiveRemoteNotification:fetchCompletionHandler:")]
        public void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        {
            try
            {
                // Extract the notification data
                var aps = userInfo["aps"] as NSDictionary;
                var alert = aps["alert"] as NSDictionary;

                string title = alert?["title"]?.ToString() ?? "(no title)";
                string body = alert?["body"]?.ToString() ?? "(no body)";

                // Save the notification to the database
                var newNotification = new PushNotifications
                {
                    title = title,
                    body = body,
                    ReceivedDateTime = DateTime.Now,
                    IsRead = false
                };

                // Save to DB
                _notificationService.SaveNotificationAsync(newNotification).Wait();

                SaveNotification(userInfo);

                // Notify the system that new data was fetched
                completionHandler(UIBackgroundFetchResult.NewData);
            }
            catch (Exception ex)
            {
                // Log the error (you can use a logging framework like Serilog or just print to console)
                Console.WriteLine($"Error processing background notification: {ex.Message}");
                completionHandler(UIBackgroundFetchResult.Failed);
            }
        }
        private void SaveNotification(NSDictionary userInfo)
        {
            var defaults = NSUserDefaults.StandardUserDefaults;
            var notifications = defaults.StringForKey("saved_notifications") ?? "[]";
            var notificationList = JsonSerializer.Deserialize<List<string>>(notifications);

            // Convert userInfo to a string and save it
            notificationList.Add(userInfo.ToString());
            defaults.SetString(JsonSerializer.Serialize(notificationList), "saved_notifications");
            defaults.Synchronize();
        }
    }
}
