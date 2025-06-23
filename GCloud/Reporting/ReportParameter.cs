using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Reporting.WebForms;

namespace GCloud.Reporting
{
    public sealed class ReportParameter
    {
        private string ParameterName { get; set; }

        public static ReportParameter CompanyName = new ReportParameter("CompanyName");
        public static ReportParameter CompanyAddress = new ReportParameter("CompanyAddress");
        public static ReportParameter DateFrom = new ReportParameter("DateFrom");
        public static ReportParameter DateTo = new ReportParameter("DateTo");

        public ReportParameter(string parameterName)
        {
            this.ParameterName = parameterName;
        }

        public Microsoft.Reporting.WebForms.ReportParameter WithValue(string parameterValue)
        {
            return new Microsoft.Reporting.WebForms.ReportParameter(ParameterName, parameterValue);
        }
    }

}