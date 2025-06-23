

namespace GCloud.Shared.Dto.Domain.CouponUsageAction
{
    public abstract class AbstractUsageActionVisitor
    {
        public abstract bool Visit(ArticleDiscountUsageActionDto usageAction, CouponDto couponDto);
        public abstract bool Visit(InvoiceDiscountUsageActionDto usageAction, CouponDto couponDto);
        public abstract bool Visit(OrderArticleUsageActionDto usageAction, CouponDto couponDto);
    }
}
