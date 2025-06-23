using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GCloud.Reporting.CustomerCoupon01
{
    public class CustomerCoupon01ViewModel : AbstractReportViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime Birthday { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public decimal Umsatz { get; set; }
        public string Datum { get; set; }
        public string Zeit { get; set; }
        public string CouponName { get; set; }
        public string ShortDescription { get; set; }
    }
}