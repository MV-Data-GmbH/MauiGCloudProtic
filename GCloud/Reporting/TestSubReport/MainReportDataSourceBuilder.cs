using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;
using GCloud.Repository;
using Microsoft.Reporting.WebForms;

namespace GCloud.Reporting.TestSubReport
{
    public class MainReportDataSourceBuilder : AbstractDataSourceBuilder<MainReportViewModel>
    {
        public MainReportDataSourceBuilder() : base(true)
        {
        }

        public MainReportDataSourceBuilder(IProcedureRepository repository, ReportParameterViewModel parameterViewModel, Store store) : base(repository, parameterViewModel, store, true)
        {
        }

        public override ReportGroup GetReportGroup()
        {
            return ReportGroup.Testbericht;
        }

        public override string GetReportName()
        {
            return "Test-Subreport";
        }

        public override string GetReportFolderName()
        {
            return "TestSubReport";
        }

        public override string GetReportFileName()
        {
            return "MainReport";
        }

        public override string GetStoredProcedureName()
        {
            return "BER_TestReport";
        }

        public override void HandleSubreportProcessing(object sender, SubreportProcessingEventArgs e)
        {
            var givenParameter = e.Parameters.FirstOrDefault(param => param.Name == "GivenValue")?.Values?.FirstOrDefault();
            if (int.TryParse(givenParameter, out var parsedGivenValue))
            {
                var subReportSourceBuilder = new SubReportSourceBuilder(_repository, ParameterViewModel, _store, parsedGivenValue);
                var subReportData = subReportSourceBuilder.LoadData();
                e.DataSources.Add(new ReportDataSource("Data", subReportData));
            }
        }

        public override List<string> GetProcedureParameterNames()
        {
            return new List<string>{"@startDate", "@endDate"};
        }
    }
}