using System.Net;

namespace GCloud.Shared.Exceptions.Home
{
    public class SendMailException : BaseGustavException
    {
        public SendMailException() : base(ExceptionStatusCode.SendMailException, HttpStatusCode.BadRequest, $"Beim versenden des Bestätigungs-Links ist ein allgemeiner Fehler aufgetreten.")
        {
        }
    }
}
