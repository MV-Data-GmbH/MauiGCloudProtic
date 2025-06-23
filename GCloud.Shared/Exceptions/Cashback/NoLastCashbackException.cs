

namespace GCloud.Shared.Exceptions.Cashback
{
    public class NoLastCashbackException : BaseGustavException
    {
        public string UserId { get; set; }
        public Guid StoreId { get; set; }

        public NoLastCashbackException(string userId, Guid storeId) : base(ExceptionStatusCode.NoLastCashback, "Es wurde kein vorhergehender Cashback Eintrag gefunden.")
        {
            UserId = userId;
            StoreId = storeId;
        }
    }
}
