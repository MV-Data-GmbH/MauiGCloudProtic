
namespace GCloudShared.Domain
{
    public class User:BasePersistable
    {
        public string UserId { get; set; }
        public string RoleName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string AuthToken { get; set; }
        public UserLoginMethod UserLoginMethod { get; set; }

        public string invitationCode { get; set; }  
        public string Password { get; set; }


        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
