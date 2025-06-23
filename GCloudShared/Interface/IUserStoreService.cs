
namespace GCloudShared.Interface
{
    public interface IUserStoreService
    {
        public Task<object> GetUserStores();
        public Task<object> AddToWatchList(string guid);
        public Task<object> DeleteFromWatchlist(string guid);
        public Task<object> GetManagerStores(); 
    }
}
