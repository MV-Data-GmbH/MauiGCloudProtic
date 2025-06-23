
using GCloud.Shared.Dto.Domain.CouponUsageAction;
using GCloud.Shared.Dto.Domain.CouponUsageRequirement;

namespace GCloud.Shared.Dto.Domain
{
    public class CouponDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public int? MaxRedeems { get; set; }
        public int? RedeemsLeft { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public decimal Value { get; set; }
        public CouponTypeDto CouponType { get; set; }
        public CouponScopeDto CouponScope { get; set; }
        public int? ArticleNumber { get; set; }
        public bool IsValid { get; set; }
        public List<Guid> AssignedStores { get; set; }
        public List<AbstractUsageActionDto> UsageActions { get; set; }
        public List<AbstractUsageRequirementDto> UsageRequirements { get; set; }
        public string IconBase64 { get; set; }
        public string CouponPoints { get; set; }
        public Color TextColor { get; set; }
        public string PointsText { get; set; }


        public ImageSource ImageSource { get; set; }
        public double PictureWidth { get; set; }

        public Color BorderColor { get; set; }

    }
}