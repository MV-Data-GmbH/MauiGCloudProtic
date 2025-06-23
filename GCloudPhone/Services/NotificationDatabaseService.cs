using GCloudPhone.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCloudPhone.Services
{
    public class NotificationDatabaseService
    {
        private readonly SQLiteAsyncConnection _database;

        public NotificationDatabaseService() 
        {
            var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "notifications.db3");
            _database = new SQLiteAsyncConnection(databasePath);
            _database.CreateTableAsync<PushNotifications>().Wait();
        }
        public Task<int> SaveNotificationAsync(PushNotifications notification)
        {
            return _database.InsertAsync(notification);

        }

        public Task<List<PushNotifications>> GetNotificationsAsync()
        {
            return _database.Table<PushNotifications>().OrderByDescending(n => n.ReceivedDateTime).ToListAsync();
        }

        public Task<List<PushNotifications>> GetUnreadNotificationsAsync()
        {
            return _database.Table<PushNotifications>().Where(n => !n.IsRead).OrderByDescending(n => n.ReceivedDateTime).ToListAsync();
        }

        public async Task MarkNotificationAsRead(int notificationId)
        {
            var notification = await _database.Table<PushNotifications>()
                                              .Where(n => n.Id == notificationId)
                                              .FirstOrDefaultAsync();

            if (notification != null)
            {
                notification.IsRead = true;
                await _database.UpdateAsync(notification);
            }
        }



        public Task<int> DeleteAllNotificationsAsync()
        {
            return _database.DeleteAllAsync<PushNotifications>();
        }

        public async Task DeleteNotificationAsync(int id)
        {
            var notification = await _database.Table<PushNotifications>()
                                              .Where(n => n.Id == id)
                                              .FirstOrDefaultAsync();

            if (notification != null)
            {
                await _database.DeleteAsync(notification); // ✅ This should work
            }
        }


        public async Task<int> GetUnreadNotificationCountAsync()
        {
            return await _database.Table<PushNotifications>().CountAsync(n => !n.IsRead);
        }

        public async Task<PushNotifications> GetNotificationByTitleAndBodyAsync(string title, string body)
        {
            return await _database.Table<PushNotifications>()
                                  .Where(n => n.title == title && n.body == body)
                                  .FirstOrDefaultAsync();
        }
    }
}
