

using System.Globalization;

namespace GCloudShared.WebShopDto
{
    public class OrderItemKuechenDisplay:BaseEntity
    {
        public int Quantity { get; set; }
        public int OrderId { get; set; }
        public decimal ProductID { get; set; }
        public int GeneratedToBestellungsImport { get; set; }
        public decimal Price { get; set; }
        public int BundleData { get; set; }

        public int Sku { get; set; }
        public string ProductName { get; set; }
        public virtual OrderKuechenDisplay Order { get; set; }
        public string FormattedPrice
        {
            get
            {
                CultureInfo euroCulture = new CultureInfo("de-DE");
                return Price.ToString("C2", euroCulture);
            }
        }
    }
}
