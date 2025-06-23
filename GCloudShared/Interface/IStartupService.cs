
namespace GCloudShared.Interface
{
    public interface IStartupService
    {
        Task<object> LoadInitialData(bool includeImage, bool includeBanner, bool includeCompanyLogo);
        Task<object> GetBackGroundImages(string alreadyDownloaded);
    }
}
