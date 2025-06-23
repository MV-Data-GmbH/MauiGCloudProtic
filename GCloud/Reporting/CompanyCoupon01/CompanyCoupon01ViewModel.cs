using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GCloud.Reporting.CompanyCoupon01
{
    public class CompanyCoupon01ViewModel : AbstractReportViewModel
    {
        public string couponname { get; set; }
        public string ShortDescription { get; set; }
        public decimal Umsatz { get; set; }
    }
}