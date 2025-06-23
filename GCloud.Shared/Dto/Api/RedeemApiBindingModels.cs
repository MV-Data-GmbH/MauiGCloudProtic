

namespace GCloud.Shared.Dto.Api
{
    public class RedeemRequestModel
    {
        public Guid CouponId { get; set; }
        public Guid UserGuid { get; set; }
        public DateTime? RedeemDateTime { get; set; }
        public decimal CouponValue { get; set; }
        public decimal CashValue { get; set; }
        public string StoreApiToken { get; set; }
    }
}