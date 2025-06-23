using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GCloud.Models.Domain
{
    public class PointsFromKassa
    {
        [Key]
        public int ID { get; set; }
        //[Display(Name = "UserId")]

        [MaxLength(128)]
        [Required]
        public string UserId { get; set; }

        //public string CouponId { get; set; }

        //public string StoreId { get; set; }

        public int PointsToDeducted { get; set; }

        //public decimal PriceToPaid { get; set; }
    }
}