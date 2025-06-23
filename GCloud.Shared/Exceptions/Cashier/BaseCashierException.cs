

namespace GCloud.Shared.Exceptions.Cashier
{
    public abstract class BaseCashierException : BaseGustavException
    {
        public Guid CashRegisterId { get; set; }

        protected BaseCashierException(Guid cashRegisterId, ExceptionStatusCode errorCode, string humanReadableMessage) : base(errorCode, humanReadableMessage)
        {
            CashRegisterId = cashRegisterId;
        }
    }
}
