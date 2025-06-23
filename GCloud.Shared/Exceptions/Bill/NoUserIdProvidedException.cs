

namespace GCloud.Shared.Exceptions.Bill
{
    public class NoUserIdProvidedException : BaseGustavException
    {
        public NoUserIdProvidedException() : base(ExceptionStatusCode.NoUserIdProvided, "No valid UserId provided")
        {

        }
    }
}
