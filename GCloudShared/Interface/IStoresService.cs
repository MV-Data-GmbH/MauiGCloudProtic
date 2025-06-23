using GCloud.Shared.Dto.Api;

namespace GCloudShared.Interface
{
    public interface IStoresService
    {
        public Task<object> GetStores();
        public Task<Stream> GetStoreImage(string guid);
        public Task<HttpResponseMessage> UpdateStore(StoreManagerEditModel storeManagerEditModel);
    }
}
