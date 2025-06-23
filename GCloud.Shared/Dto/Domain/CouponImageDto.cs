

namespace GCloud.Shared.Dto.Domain
{
    public class CouponImageDto
    {

        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string OrigFileName { get; set; }
        public DateTime CreationDateTime { get; set; }

        public CouponDto Coupon { get; set; }
        public string CreatorId { get; set; }
        public UserDto Creator { get; set; }
    }
}