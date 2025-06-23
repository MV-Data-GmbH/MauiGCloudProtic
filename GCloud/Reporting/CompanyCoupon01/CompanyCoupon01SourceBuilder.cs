using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;
using GCloud.Repository;
using Microsoft.Reporting.WebForms;

namespace GCloud.Reporting.CompanyCoupon01
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

    public class CompanyCoupon01SourceBuilder : AbstractDataSourceBuilder<CompanyCoupon01ViewModel>
    {
        public CompanyCoupon01SourceBuilder() : base(true)
        {
        }

        public CompanyCoupon01SourceBuilder(IProcedureRepository repository, ReportParameterViewModel parameterViewModel, Store store) : base(repository, parameterViewModel, store, true)
        {
            IgnoreCompanyParam = false;
        }

        public override ReportGroup GetReportGroup()
        {
            return ReportGroup.Coupons;
        }

        public override string GetReportName()
        {
            return "Eingelöste Gutscheine";
        }

        public override string GetReportFolderName()
        {
            return "CompanyCoupon01";
        }

        public override string GetReportFileName()
        {
            return "CompanyCoupon01";
        }

        public override string GetStoredProcedureName()
        {
            return "COUPONCOMPANY01";
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