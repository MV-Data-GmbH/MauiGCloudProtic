
namespace GCloudShared.Interface
{
    public interface ICashbackService
    {
        public Task<object> GetCashbacksForStore(string storeGuid);
    }
}
