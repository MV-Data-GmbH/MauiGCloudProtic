

namespace GCloud.Shared.Exceptions.User
{
    public abstract class BaseStoreException : BaseGustavException
    {
        public Guid? StoreId { get; set; }

        protected BaseStoreException(ExceptionStatusCode errorCode, string humanReadableMessage, Guid? storeId) : base(errorCode, humanReadableMessage)
        {
            StoreId = storeId;
        }
    }
}
