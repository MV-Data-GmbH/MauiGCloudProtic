#nullable enable

using GCloudPhone.Models;
using GCloudShared.Service.Dto;

namespace GCloudShared.Interface
{
    public interface IAuthService
    {
        public Task<object> LoginService(LoginRequestModel model);
        public Task<HttpResponseMessage> Logout(String deviceId);
        public Task<object> RegisterService(RegisterRequestModel model);
        public Task<bool?> IsUsernameAvailable(string username);
        public Task<bool?> IsEmailAvailable(string email);
        public Task<object?> IsInvitationCodeAvailable(string invationCode);
        public Task<string?> InvitationCodeSenderId(string invitationCode);
        public Task<object> GetSpecialProductsByCompanyName(string companyName);
        public Task<object> GetQRCode();
        public Task<List<string>> GetAllUsers();
        public Task<object> ChangePassword(ChangePasswordRequestModel changePasswordRequestModel);
        Task<string> GetTotalPointsByUserID(string userId);
        public Task<object> ResetPassword(string usernameOrEmail);
        bool IsLogged();
        Task<object> ResendActivationEmail(string username);
        Task<object> CheckIfUserCanBuySpecialProduct(string productId, string userId);
        Task<int> DeleteUser(string email);
        Task<List<PushNotifications>> CheckForUserNotifications(string userId, DateTime dateTime);
    }
}
