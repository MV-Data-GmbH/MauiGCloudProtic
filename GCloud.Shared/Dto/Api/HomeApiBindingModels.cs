
using GCloud.Shared.Dto.Domain;

namespace GCloud.Shared.Dto.Api
{
    public class LoginRequestModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponseModel
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string AuthToken { get; set; }
        public Guid MobilePhoneGuid { get; set; }
        public string InvitationCode { get; set; }
    }

    public class LoadInitialDataResponseModel
    {
        public List<StoreDto> Stores { get; set; }
        public List<CouponDto> Coupons { get; set; }
    }
}