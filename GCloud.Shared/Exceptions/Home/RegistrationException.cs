
using System.Net;


namespace GCloud.Shared.Exceptions.Home
{
    public class RegistrationException : BaseGustavException
    {
        public RegistrationException() : base(ExceptionStatusCode.GeneralRegistrationException, HttpStatusCode.BadRequest, $"Bei der registrierung ist ein allgemeiner Fehler aufgetreten.")
        {
        }
    }
}
