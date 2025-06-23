
namespace GCloudShared.Interface
{
    public interface IUserCouponService
    {
        Task<object> GetUserCoupons(bool skipUserValidation = false);
        Task<object> GetUserCoupon(string guid);
        Task<object> GetCouponQrCode(string guid);
        Task<object> GetCouponImage(string guid);
        Task<object> GetUserCouponsByStore(string guid, bool skipUserValidation = false, bool includeImage = true);
        Task<object> GetManagerCoupons();
    }
}
