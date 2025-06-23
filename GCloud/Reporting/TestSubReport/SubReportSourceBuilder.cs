using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;
using GCloud.Repository;
using Microsoft.Reporting.WebForms;

namespace GCloud.Reporting.TestSubReport
{
    public class SubReportSourceBuilder : AbstractDataSourceBuilder<SubReportViewModel>
    {
        public SubReportSourceBuilder() : base(false)
        {
            IgnoreCompanyParam = true;
        }

        public SubReportSourceBuilder(IProcedureRepository repository, ReportParameterViewModel parameterViewModel, Store store, int givenValue) : base(repository, parameterViewModel, store, false)
        {
            ParameterViewModel = parameterViewModel.Clone();
            ParameterViewModel.Add("@customParameter", givenValue);
            IgnoreCompanyParam = true;
        }

        public override ReportGroup GetReportGroup()
        {
            return ReportGroup.Testbericht;
        }

        public override string GetReportName()
        {
            return "TestSubreport";
        }

        public override string GetReportFolderName()
        {
            return "TestSubReport";
        }

        public override string GetReportFileName()
        {
            return "Subreport";
        }

        public override string GetStoredProcedureName()
        {
            return "BER_TestSubReport";
        }

        public override void HandleSubreportProcessing(object sender, SubreportProcessingEventArgs e)
        {
        }

        public override List<string> GetProcedureParameterNames()
        {
            return new List<string>{"@customParameter"};
        }
    }
}