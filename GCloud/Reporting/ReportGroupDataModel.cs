using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GCloud.Reporting
{
    public class ReportGroupDataModel
    {
        public string GroupName { get; set; }
        public List<ReportNameDataModel> GroupReports { get; set; } = new List<ReportNameDataModel>();
    }

    public class ReportNameDataModel
    {
        public string ReportName { get; set; }
    }
}