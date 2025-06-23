using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using GCloud.Models;

namespace GCloud.Models.Domain
{
    public class Redeem : ISoftDeletable
    {
        [Key]
        [Column(Order = 1)]
        public Guid CouponId { get; set; }
        public virtual Coupon Coupon { get; set; }
        [Key]
        [Column(Order = 2)]
        public string UserId { get; set; }
        public virtual User User { get; set; }
        [Key]
        [Column(Order = 3)]
        public DateTime RedeemDateTime { get; set; }
        public Guid RedeemedStoreId { get; set; }
        public virtual Store RedeemedStore { get; set; }
        public decimal CashValue { get; set; }
        public decimal CouponValue { get; set; }
        public bool IsDeleted { get; set; }
    }
}