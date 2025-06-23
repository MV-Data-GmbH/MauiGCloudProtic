
using GCloud.Shared.Dto.Domain.CouponUsageAction;

namespace GCloud.Shared.Dto.Domain.CouponUsageRequirement
{
    public class ProductRequiredUsageRequirementDto : AbstractUsageRequirementDto
    {
        public ISet<string> RequiredProducts { get; set; }
        public Coupling PredicateCoupling { get; set; } = Coupling.Or;

        public override bool Apply(AbstractUsageRequirementVisitor visitor, CouponDto couponDto)
        {
            return visitor.Visit(this, couponDto);
        }
    }
}