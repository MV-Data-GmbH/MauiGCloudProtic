using GCloudShared.Domain;
using SQLite;

namespace GCloudShared.Repository
{
    public class SubscriptionRepository: AbstractRepository<Subscription>
    {
        public SubscriptionRepository(SQLiteConnection connection) : base(connection)
        {
        }

        public bool HasSubscription(Guid storeId)
        {
            return _connection.Table<Subscription>().Any(s => s.StoreId == storeId);
        }

        public bool HasSubscription(string storeId)
        {
            if (Guid.TryParse(storeId, out var storeGuid))
            {
                return HasSubscription(storeGuid);
            }

            return false;
        }

        public void DeleteById(Guid storeId)
        {
            _connection.Table<Subscription>().Delete(s => s.StoreId == storeId);
        }

        public void DeleteById(string storeId)
        {
            if (Guid.TryParse(storeId, out var storeGuid))
            {
                DeleteById(storeGuid);
            }
        }
    }
}
