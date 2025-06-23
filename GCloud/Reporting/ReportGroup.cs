using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GCloud.Reporting
{
    public sealed class ReportGroup
    {
        public static ReportGroup Coupons = new ReportGroup("Gutscheine", true);
        public static ReportGroup Customers = new ReportGroup("Kunden", true);
        public static ReportGroup StoresAndCompanies = new ReportGroup("Filialen", true);
        public static ReportGroup Testbericht = new ReportGroup("Testberichte", true);

        private string GroupName { get; set; }
        private bool IsVisible { get; set; }

        private ReportGroup(string groupName, bool isVisible)
        {
            this.GroupName = groupName;
            this.IsVisible = isVisible;
        }

        public string GetGroupName()
        {
            return GroupName;
        }

        public bool GetIsVisible()
        {
            return IsVisible;
        }

        public override bool Equals(object obj)
        {
            if (obj is ReportGroup reportGroup)
                return reportGroup.GroupName == GroupName;
            return false;
        }
    }

}