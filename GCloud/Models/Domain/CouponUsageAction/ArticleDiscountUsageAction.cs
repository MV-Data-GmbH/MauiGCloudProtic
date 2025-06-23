using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GCloud.Models.Domain.CouponUsageAction
{
    public class ArticleDiscountUsageAction : AbstractUsageAction
    {
        /// <summary>
        /// This is the target article that gets a discount
        /// </summary>
        public int TargetArticle { get; set; }

        /// <summary>
        /// Gibt die Höhe des Rabattes an
        /// </summary>
        [Column("ArticleDiscount")]
        public decimal? Discount { get; set; }
        /// <summary>
        /// Entscheidet ob es sich um einen Prozent-, oder Wertgutschein handelt
        /// </summary>
        [Column("ArticleDiscountType")]
        public CouponType? DiscountType { get; set; }
    }
}