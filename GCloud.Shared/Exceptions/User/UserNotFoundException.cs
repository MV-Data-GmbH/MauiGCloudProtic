

namespace GCloud.Shared.Exceptions.User
{
    public class UserNotFoundException : BaseUserException
    {
        public UserNotFoundException(string userId) : base(ExceptionStatusCode.UserNotFound, $"Benutzer wurde nicht gefunden.", userId)
        {
        }
    }
}
