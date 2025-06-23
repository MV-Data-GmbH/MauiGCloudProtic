using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GCloud.Reporting.CompanyCoupon02
{
    public class CompanyCoupon02ViewModel : AbstractReportViewModel
    {
        public string couponname { get; set; }
        public string ShortDescription { get; set; }
        public decimal Umsatz { get; set; }
        public string Datum { get; set; }
        public string Zeit { get; set; }
    }
}