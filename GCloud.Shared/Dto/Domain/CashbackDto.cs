

namespace GCloud.Shared.Dto.Domain
{
    public class CashbackDto
    {
        public Guid Id { get; set; }
        public decimal TurnoverOld { get; set; }
        public decimal TurnoverChange { get; set; }
        public decimal TurnoverNew { get; set; }
        public DateTime CreditDateTime { get; set; }
        public string UserId { get; set; }
        public Guid StoreId { get; set; }
        public decimal CreditOld { get; set; }
        public decimal CreditChange { get; set; }
        public decimal CreditNew { get; set; }
    }
}