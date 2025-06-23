

namespace GCloud.Shared.Exceptions.User
{
    public abstract class BaseCouponException : BaseGustavException
    {
        public Guid? CouponId { get; set; }

        protected BaseCouponException(ExceptionStatusCode errorCode, string humanReadableMessage, Guid? couponId) : base(errorCode, humanReadableMessage)
        {
            CouponId = couponId;
        }
    }
}
