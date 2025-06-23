

namespace GCloudShared.Service.Dto
{
    public class LoginRequestModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string DeviceId { get; set; }
        public string FirebaseInstanceId { get; set; }
    }
}
