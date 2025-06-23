using SQLite;

namespace GCloudPhone.Models
{
    public class PushNotifications: NotificationMessageBody
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string NavigationID { get; set; }
        public DateTime ReceivedDateTime { get; set; }
        public bool IsRead { get; set; }
        public Color FrameBorderColor => IsRead ? Colors.Gray : Colors.DarkRed;
    }
}
