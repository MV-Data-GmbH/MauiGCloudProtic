using Android.App;
using Android.Content;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Firebase.Messaging;
using GCloudPhone.Models;
using GCloudPhone.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Android.Manifest;

namespace GCloudPhone.Platforms.Android.Service
{
    [Service(Exported = true)]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class FirebaseService : FirebaseMessagingService
    {
        private readonly NotificationDatabaseService _notificationDatabaseService = new NotificationDatabaseService();

        public FirebaseService()
        {
            
        }
        public override void OnNewToken(string token)
        {
            base.OnNewToken(token);
            if (Preferences.ContainsKey("DeviceToken"))
            {
                Preferences.Remove("DeviceToken");
            }
            Preferences.Set("DeviceToken", token);
        }

        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);

            var notification = message.GetNotification();

            if (notification != null) {
                var newNotification = new PushNotifications
                {
                    title = notification.Title,
                    body = notification.Body,
                    ReceivedDateTime = DateTime.Now,
                    IsRead = false
                };
                _notificationDatabaseService.SaveNotificationAsync(newNotification).Wait();
            }

            SendNotification(notification.Body, notification.Title, message.Data);
        }


        private void SendNotification(string messageBody, string title, IDictionary<string, string> data)
        {

            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            intent.AddFlags(ActivityFlags.SingleTop);


            foreach (var key in data.Keys)
            {
                string value = data[key];
                intent.PutExtra(key, value);
            }

            var pendingIntent = PendingIntent.GetActivity(this,
                MainActivity.NotificationID, intent, PendingIntentFlags.OneShot | PendingIntentFlags.Immutable);

            var notificationBuilder = new NotificationCompat.Builder(this, MainActivity.Channel_ID)
                .SetContentTitle(title)
                .SetSmallIcon(Resource.Drawable.appicon)
                .SetContentText(messageBody)
                .SetChannelId(MainActivity.Channel_ID)
                .SetContentIntent(pendingIntent)
                .SetAutoCancel(true)
                .SetPriority((int)NotificationPriority.Max);

            var notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(MainActivity.NotificationID, notificationBuilder.Build());
        }
    }
}
