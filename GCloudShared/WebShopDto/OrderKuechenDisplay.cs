
namespace GCloudShared.WebShopDto
{
    public class OrderKuechenDisplay:BaseEntity
    {
        public string OrderNumber { get; set; }
        public DateTime Date { get; set; }
        public DateTime? PaidOn { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }

        public string PaymentMethod { get; set; }

        public int CustomerID { get; set; }
        public string DeliveryTyp { get; set; }
        public DateTime DeliveryDateUtc { get; set; }
        public int StoreId { get; set; }
        public string OrderComment { get; set; }
        public decimal ShippingPrice { get; set; }
        public ICollection<OrderItemKuechenDisplay> orderItemKuechenDisplays { get; set; } = new HashSet<OrderItemKuechenDisplay>();

        public string Email { get; set; }
    }
}
