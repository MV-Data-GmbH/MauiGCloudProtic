using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GCloud.Models.Domain
{
    public enum CouponScope
    {
        [Display(Name = "Rechnung")]
        Invoice = 0,
        [Display(Name = "Artikel")]
        Article = 1
    }
}