using System.Net;


namespace GCloud.Shared.Exceptions.User
{
    public class EmailNotConfirmedException : BaseUserException
    {
        public EmailNotConfirmedException(string userId) : base(ExceptionStatusCode.EmailNotConfirmed, HttpStatusCode.NotAcceptable, $"E-Mail noch nicht bestätigt.", userId)
        {
        }
    }
}
