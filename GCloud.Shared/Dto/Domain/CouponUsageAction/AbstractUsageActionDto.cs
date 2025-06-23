

namespace GCloud.Shared.Dto.Domain.CouponUsageAction
{
    public abstract class AbstractUsageActionDto
    {
        public Guid Id { get; set; }
        public int SortOrder { get; set; }
        public Guid CouponId { get; set; }

        public abstract bool Apply(AbstractUsageActionVisitor visitor, CouponDto couponDto);
    }
}