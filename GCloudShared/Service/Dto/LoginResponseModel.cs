

namespace GCloudShared.Service.Dto
{
    public class LoginResponseModel
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string AuthToken { get; set; }
        public Guid MobilePhoneGuid { get; set; }

        public string InvitationCode { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

    }
}
