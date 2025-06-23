using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GCloud.Models.Domain.CouponUsageRequirement
{
    /// <summary>
    /// Gibt an, dass dieser Gutschein nur verwendet werden kann, wenn die Zwischensumme an der Kasse den gegebenen Wert überschreitet (>=)
    /// </summary>
    public class MinimumTurnoverRequirement : AbstractUsageRequirement
    {
        public decimal MinimumTurnover { get; set; }
        public bool DiscountsDecreaseTurnover { get; set; }
    }
}