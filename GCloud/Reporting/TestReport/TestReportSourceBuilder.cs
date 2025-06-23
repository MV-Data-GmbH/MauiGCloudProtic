using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;
using GCloud.Repository;
using Microsoft.Reporting.WebForms;

namespace GCloud.Reporting.TestReport
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.VisualBasic;

    public class TestReportSourceBuilder : AbstractDataSourceBuilder<TestReportViewModel>
    {
        public TestReportSourceBuilder() : base(true)
        {
        }

        public TestReportSourceBuilder(IProcedureRepository repository, ReportParameterViewModel parameterViewModel, Store store) : base(repository, parameterViewModel, store, true)
        {
            IgnoreCompanyParam = false;
        }

        public override ReportGroup GetReportGroup()
        {
            return ReportGroup.Testbericht;
        }

        public override string GetReportName()
        {
            return "Test-Bericht";
        }

        public override string GetReportFolderName()
        {
            return "TestReport";
        }

        public override string GetReportFileName()
        {
            return "TestReport";
        }

        public override string GetStoredProcedureName()
        {
            return "BER_TestReport";
        }

        public override string GetSchema()
        {
            return "dbo";
        }

        public override List<string> GetProcedureParameterNames()
        {
            return new List<string>() { "@startDate", "@endDate" };
        }

        public override void HandleSubreportProcessing(object sender, SubreportProcessingEventArgs e)
        {
        }
    }

}