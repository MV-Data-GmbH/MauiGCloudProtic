
using GCloud.Shared.Exceptions.User;

namespace GCloud.Shared.Exceptions.Store
{
    public class StoreNotFoundException : BaseStoreException
    {
        public StoreNotFoundException(Guid storeId) : base(ExceptionStatusCode.StoreNotFound, $"Die Filiale wurde nicht gefunden.", storeId)
        {
        }
    }
}
