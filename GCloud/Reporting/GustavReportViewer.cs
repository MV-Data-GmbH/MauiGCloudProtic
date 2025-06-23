using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Reporting.WebForms;

namespace GCloud.Reporting
{
    public class GustavReportViewer : ReportViewer
    {
        public IDataSourceBuilder DataSourceBuilder { get; set; }
    }
}