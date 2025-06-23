

namespace GCloud.Shared.Exceptions.Home
{
    public class OldPasswordInvalidException : BaseGustavException
    {
        public OldPasswordInvalidException() : base(ExceptionStatusCode.OldPasswordInvalid, $"Das alte Password ist falsch.")
        {
        }
    }
}
