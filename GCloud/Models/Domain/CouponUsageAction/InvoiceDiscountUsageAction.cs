using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GCloud.Models.Domain.CouponUsageAction
{
    public class InvoiceDiscountUsageAction : AbstractUsageAction
    {
        /// <summary>
        /// Gibt die Höhe des Rabattes an
        /// </summary>
        [Column("InvoiceDiscount")]
        public decimal? Discount { get; set; }
        /// <summary>
        /// Entscheidet ob es sich um einen Prozent-, oder Wertgutschein handelt
        /// </summary>
        [Column("InvoiceDiscountType")]
        public CouponType? DiscountType { get; set; }
    }
}