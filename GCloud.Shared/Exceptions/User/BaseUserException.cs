
using System.Net;


namespace GCloud.Shared.Exceptions.User
{
    public abstract class BaseUserException : BaseGustavException
    {
        public string UserId { get; set; }

        protected BaseUserException(ExceptionStatusCode errorCode, string humanReadableMessage, string userId) : base(errorCode, humanReadableMessage)
        {
            UserId = userId;
        }

        protected BaseUserException(ExceptionStatusCode errorCode, HttpStatusCode httpStatusCode, string humanReadableMessage, string userId) : base(errorCode, httpStatusCode, humanReadableMessage)
        {
            UserId = userId;
        }
    }
}
