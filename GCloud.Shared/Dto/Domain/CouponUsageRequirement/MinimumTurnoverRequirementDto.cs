namespace GCloud.Shared.Dto.Domain.CouponUsageRequirement
{
    /// <summary>
    /// Gibt an, dass dieser Gutschein nur verwendet werden kann, wenn die Zwischensumme an der Kasse den gegebenen Wert überschreitet (>=)
    /// </summary>
    public class MinimumTurnoverRequirementDto : AbstractUsageRequirementDto
    {
        public decimal MinimumTurnover { get; set; }
        public bool DiscountsDecreaseTurnover { get; set; }

        public override bool Apply(AbstractUsageRequirementVisitor visitor, CouponDto couponDto)
        {
            return visitor.Visit(this, couponDto);
        }
    }
}