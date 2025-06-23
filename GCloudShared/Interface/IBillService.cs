
namespace GCloudShared.Interface
{
    public interface IBillService
    {
        public Task<object> Get();
        Task<object> GetById(Guid id);
        Task<object> Csv(Guid? anonymousUserId);
    }
}
