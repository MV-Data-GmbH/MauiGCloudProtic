using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Reporting.WebForms;

namespace GCloud.Reporting
{
    public interface IDataSourceBuilder
    {
        bool IsVisible { get; set; }
        ReportGroup GetReportGroup();
        string GetReportName();
        string GetReportFolderName();
        string GetReportFileName();
        string GetStoredProcedureName();
        string GetSchema();
        List<Microsoft.Reporting.WebForms.ReportParameter> GetReportParameters();
        IEnumerable LoadData();
        void HandleSubreportProcessing(object sender, SubreportProcessingEventArgs e);

    }
}