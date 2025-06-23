using CommunityToolkit.Mvvm.Messaging.Messages;


namespace GCloudPhone.Models
{
    public class PushNotificationReceived : ValueChangedMessage<string>
    {
        public PushNotificationReceived(string message) : base(message) { }
    }
}
