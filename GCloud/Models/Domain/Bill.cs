using GCloud.Shared.Dto;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GCloud.Models.Domain
{
    public class Bill : ISoftDeletable, IIdentifyable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public Guid GetId() => Id;
        
        [Display(Name = "User")]
        public string UserId { get; set; }
        public virtual User User { get; set; }
        
        [Display(Name = "Imported At")]
        public DateTime ImportedAt { get; set; }

        [Display(Name = "Invoice Number")]
        public string InvoiceNumber { get; set; }

        [Display(Name = "Invoice Date")]
        public DateTime InvoiceDate { get; set; }

        public string Company { get; set; }
        public decimal Amount { get; set; }

        public virtual AnonymousUser AnonymousUser { get; set; }
        public Guid? AnonymousUserId { get; set; }

        public virtual CashRegister CashRegister { get; set; }
        public Guid? CashRegisterId { get; set; }

        private Invoice invoice = null;
        [NotMapped]
        public Invoice Invoice
        {
            get
            {
                if (invoice == null)
                    if (string.IsNullOrWhiteSpace(InvoiceXml) == false)
                        invoice = JsonConvert.DeserializeObject<Invoice>(InvoiceXml);
                //  We stay with JSON. Is more readable

                return invoice;
            }

            set
            {
                invoice = value;

                //  We stay with JSON. Is more readable
                InvoiceXml = JsonConvert.SerializeObject(invoice);
            }
        }
        public string InvoiceXml { get; set; }


        public bool IsDeleted { get; set; }

        public string StoreApiToken { get; set; }
    }
}