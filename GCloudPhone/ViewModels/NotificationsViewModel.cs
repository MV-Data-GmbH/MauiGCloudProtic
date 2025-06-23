using GCloudPhone.Models;
using GCloudPhone.Services;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCloudPhone.ViewModels
{
    public class NotificationsViewModel : BindableObject
    {
        private readonly NotificationDatabaseService _notificationService = new NotificationDatabaseService();

        public ObservableCollection<PushNotifications> Notifications { get; set; } = new();

        public async Task LoadNotificationsAsync()
        {
            var notifications = await _notificationService.GetNotificationsAsync();
            Notifications.Clear();
            foreach (var notification in notifications)
            {
                Notifications.Add(notification);
            }
        }

        public async Task MarkNotificationAsRead(PushNotifications notification)
        {
            if (!notification.IsRead)
            {
                await _notificationService.MarkNotificationAsRead(notification.Id);
                notification.IsRead = true;
            }
        }
    }
}
