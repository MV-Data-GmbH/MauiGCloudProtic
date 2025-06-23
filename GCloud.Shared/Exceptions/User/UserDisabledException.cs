using System.Net;


namespace GCloud.Shared.Exceptions.User
{
    public class UserDisabledException : BaseUserException
    {
        public UserDisabledException(string userId) : base(ExceptionStatusCode.UserDisabled, HttpStatusCode.PreconditionFailed, $"Der Benutzer wurde deaktiviert.", userId)
        {
        }
    }
}
