

namespace GCloud.Shared.Exceptions.User
{
    public abstract class BaseCashbackException : BaseGustavException
    {
        public Guid? CashbackId { get; set; }

        protected BaseCashbackException(ExceptionStatusCode errorCode, string humanReadableMessage, Guid? cashbackId) : base(errorCode, humanReadableMessage)
        {
            CashbackId = cashbackId;
        }
    }
}
