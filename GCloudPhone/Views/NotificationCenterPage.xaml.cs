using GCloudPhone.Models;
using GCloudPhone.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using Microsoft.Maui.Dispatching;
using System.Windows.Input;

namespace GCloudPhone.Views
{
    public partial class NotificationCenterPage : ContentPage
    {
        private readonly NotificationDatabaseService _notificationService = new NotificationDatabaseService();
        public ObservableCollection<PushNotifications> Notifications { get; set; } = new();
        public ICommand DeleteNotificationCommand { get; }

        public NotificationCenterPage()
        {
            InitializeComponent();
            BindingContext = this;
            DeleteNotificationCommand = new Command<int>(async (notificationId) => await DeleteNotification(notificationId));
            LoadNotifications();
        }

     
        private async Task DeleteNotification(int notificationId)
        {
            var notificationToDelete = Notifications.FirstOrDefault(n => n.Id == notificationId);
            if (notificationToDelete != null)
            {
                // Remove from SQLite database
                await _notificationService.DeleteNotificationAsync(notificationId);

                // Remove from ObservableCollection
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Notifications.Remove(notificationToDelete);
                    NoNotificationsLabel.IsVisible = Notifications.Count == 0;
                });

                Debug.WriteLine($"Deleted notification ID: {notificationId}");
            }
        }
        private async void LoadNotifications()
        {
            var notifications = new List<PushNotifications>();

            notifications = await _notificationService.GetNotificationsAsync();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Notifications.Clear();
                foreach (var notification in notifications.OrderByDescending(n => n.ReceivedDateTime))
                {
                    Notifications.Add(notification);
                }
                NoNotificationsLabel.IsVisible = Notifications.Count == 0;
            });
        }

        private async void OnNotificationTapped(object sender, TappedEventArgs e)
        {
            if (e.Parameter is int notificationId)
            {
                Debug.WriteLine($"Tapped notification ID: {notificationId}");

                var notification = Notifications.FirstOrDefault(n => n.Id == notificationId);
                if (notification != null && !notification.IsRead)
                {  
                    await _notificationService.MarkNotificationAsRead(notification.Id);

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        notification.IsRead = true;

                        // Notify UI about changes (CollectionView will refresh automatically)
                        var index = Notifications.IndexOf(notification);
                        if (index >= 0)
                        {
                            Notifications[index] = new PushNotifications
                            {
                                Id = notification.Id,
                                title = notification.title,
                                body = notification.body,
                                ReceivedDateTime = notification.ReceivedDateTime,
                                IsRead = true // Update to read
                            };
                        }
                    });
                }
            }
        }

    }
}
