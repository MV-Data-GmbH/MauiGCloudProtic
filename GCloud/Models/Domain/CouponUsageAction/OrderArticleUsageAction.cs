using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GCloud.Models.Domain.CouponUsageAction
{
    public class OrderArticleUsageAction : AbstractUsageAction
    {
        /// <summary>
        /// A List of Products that you need to use
        /// </summary>
        public virtual List<OrderArticleUsageActionItem> ToOrderProducts { get; set; }

        public Coupling Coupling { get; set; }
    }

    public class OrderArticleUsageActionItem
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string ProductNumber { get; set; }
        public int Amount { get; set; } = 1;
    }
}