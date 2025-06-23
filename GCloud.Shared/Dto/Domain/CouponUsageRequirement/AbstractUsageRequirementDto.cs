

namespace GCloud.Shared.Dto.Domain.CouponUsageRequirement
{
    public abstract class AbstractUsageRequirementDto
    {
        public Guid Id { get; set; }
        public int SortOrder { get; set; }
        public Guid CouponId { get; set; }

        public abstract bool Apply(AbstractUsageRequirementVisitor visitor, CouponDto couponDto);
    }
}