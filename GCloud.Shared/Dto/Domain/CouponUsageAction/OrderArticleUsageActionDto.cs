

namespace GCloud.Shared.Dto.Domain.CouponUsageAction
{
    public class OrderArticleUsageActionDto : AbstractUsageActionDto
    {
        /// <summary>
        /// A List of Products that you need to use
        /// </summary>
        public List<OrderArticleUsageActionItemDto> ToOrderProducts { get; set; }
        public Coupling Coupling { get; set; }

        public override bool Apply(AbstractUsageActionVisitor visitor, CouponDto couponDto)
        {
            return visitor.Visit(this, couponDto);
        }
    }

    public class OrderArticleUsageActionItemDto
    {
        public string ProductNumber { get; set; }
        public int Amount { get; set; }
    }
}