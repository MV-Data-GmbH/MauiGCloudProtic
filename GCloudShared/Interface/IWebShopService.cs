using GCloudShared.Service.Dto;

namespace GCloudShared.Interface
{
    public interface IWebShopService
    {
        Task<object> Register(RegisterToWebShopModel model);
        Task<object> CheckIfUserIsAlreadyRegistredInWebShop(RegisterToWebShopModel model);
        Task<string> ResetPasswordInWebShopFromGcloud(RecoveryPasswordToWebShopModel model);
        Task<string> SetWelcomeEmailToWebShopFromGcloud(RecoveryPasswordToWebShopModel model);
        Task<object> GetLastOrder();
        Task<object> GetOrdersInLastHour();
    }
}
