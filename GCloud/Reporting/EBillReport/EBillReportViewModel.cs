using GCloud.Shared.Dto;
using Newtonsoft.Json;

namespace GCloud.Reporting.EBillReport
{
    public class EBillReportViewModel
    {
        public string InvoiceXml { get; set; }

        private Invoice invoice = null;

        [JsonIgnore]
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

        public string StoreName => Invoice?.Biller.Address.Name;
        public string CompanyName => Invoice?.Biller.ComanyName;
        public string StoreAddress => $"{Invoice?.Biller.Address.Street}, {Invoice?.Biller.Address.ZIP} {Invoice?.Biller.Address.Town}";
        public string CompanyAtu => Invoice?.Biller.VATIdentificationNumber;
        public int TerminalId => (int) Invoice?.Biller.InvoiceRecipientsBillerID;
        public string InvoiceNo => Invoice?.InvoiceNumber;
        public string Date => Invoice?.InvoiceDate.ToString("dd-MM-yyyy");
        public string Time => Invoice?.InvoiceDate.ToString("t");
    }
}