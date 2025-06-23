

namespace GCloudShared.Service.Dto
{
    public class RegisterRequestModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public DateTime Birthday { get; set; }

        public string InvitationCode { get; set; }

        public string InvitationCodeSender { get; set; }
    }
}
