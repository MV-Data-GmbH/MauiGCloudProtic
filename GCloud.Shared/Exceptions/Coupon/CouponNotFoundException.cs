
using GCloud.Shared.Exceptions.User;

namespace GCloud.Shared.Exceptions.Coupon
{
    public class CouponNotFoundException : BaseCouponException
    {
        public CouponNotFoundException(Guid? couponId) : base(ExceptionStatusCode.CouponNotFound, $"Der Gutschein konnte nicht gefunden werden.", couponId)
        {
        }
    }
}
