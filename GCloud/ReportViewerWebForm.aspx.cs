using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GCloud.Reporting;

namespace GCloud
{
    public class ReportViewerWebForm : ReportViewerForMvc.ReportViewerWebForm
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!Page.IsPostBack)
            {
                if (ReportViewer1 is GustavReportViewer gustavReportViewer)
                {
                    var datasourceBuilder = gustavReportViewer.DataSourceBuilder;
                    gustavReportViewer.LocalReport.SubreportProcessing += datasourceBuilder.HandleSubreportProcessing;
                }
            }
        }
    }
}