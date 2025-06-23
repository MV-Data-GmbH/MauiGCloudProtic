

namespace GCloud.Shared.Dto.Domain.CouponUsageRequirement
{
    public abstract class AbstractUsageRequirementVisitor
    {
        public abstract bool Visit(ProductRequiredUsageRequirementDto usageRequirement, CouponDto couponDto);

        public abstract bool Visit(MinimumTurnoverRequirementDto minimumTurnoverRequirementDto, CouponDto couponDto);
    }
}
