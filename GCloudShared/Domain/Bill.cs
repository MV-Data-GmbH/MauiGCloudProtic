using GCloud.Shared.Dto;
using Newtonsoft.Json;
using SQLite;

namespace GCloudShared.Domain
{
    public class Bill:BasePersistable
    {
        public Guid ServerId { get; set; }
        public string Company { get; set; }
        public decimal? Amount { get; set; }
        public string InvoiceXml { get; set; }

        private Invoice invoice = null;
        [Ignore]
        public Invoice Invoice
        {
            get
            {
                if (invoice == null)
                    if (string.IsNullOrWhiteSpace(InvoiceXml) == false)
                        invoice = JsonConvert.DeserializeObject<Invoice>(InvoiceXml);

                return invoice;
            }

            set
            {
                invoice = value;

                InvoiceXml = JsonConvert.SerializeObject(invoice);
            }
        }
    }
}
