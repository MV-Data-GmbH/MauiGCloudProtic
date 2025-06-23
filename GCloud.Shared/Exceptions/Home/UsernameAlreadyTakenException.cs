using System.Net;


namespace GCloud.Shared.Exceptions.Home
{
    public class UsernameAlreadyTakenException : BaseGustavException
    {
        public UsernameAlreadyTakenException(string username) : base(ExceptionStatusCode.UsernameAlreadyTaken, HttpStatusCode.Conflict, $"Benutzername {username} bereits vergeben.")
        {
        }
    }
}
