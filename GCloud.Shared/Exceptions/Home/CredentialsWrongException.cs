using System.Net;


namespace GCloud.Shared.Exceptions.Home
{
    public class CredentialsWrongException : BaseGustavException
    {
        public CredentialsWrongException() : base(ExceptionStatusCode.CredentialsInvalid, HttpStatusCode.NotFound, $"Die eingegebenen Zugangsdaten sind falsch.")
        {
        }
    }
}
