using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GCloud.Models.Domain.CouponUsageRequirement
{
    public abstract class AbstractUsageRequirement
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public int SortOrder { get; set; }
        public Coupon Coupon { get; set; }
        public Guid CouponId { get; set; }
    }
}