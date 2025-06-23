using System;
using System.Collections.Generic;
using GCloud.Models.Domain;
using GCloud.Repository;
using Microsoft.Reporting.WebForms;

namespace GCloud.Reporting.EBillReport
{
    public class EBillReportSourceBuilder:AbstractDataSourceBuilder<EBillReportViewModel>
    {
        public EBillReportSourceBuilder() : base(false)
        {
        }

        public EBillReportSourceBuilder(IProcedureRepository repository, ReportParameterViewModel parameterViewModel, Store store, bool isVisible) : base(repository, parameterViewModel, store, isVisible)
        {
            IgnoreCompanyParam = true;
        }

        public override ReportGroup GetReportGroup()
        {
            return ReportGroup.Testbericht;
        }

        public override string GetReportName()
        {
            return "EBill";
        }

        public override string GetReportFolderName()
        {
            return "EBillReport";
        }

        public override string GetReportFileName()
        {
            return "EBillReport";
        }

        public override string GetStoredProcedureName()
        {
            return "BER_EBillReport";
        }

        public override string GetSchema()
        {
            return "dbo";
        }

        public override void HandleSubreportProcessing(object sender, SubreportProcessingEventArgs e)
        {
            throw new NotImplementedException();
        }

        public override List<string> GetProcedureParameterNames()
        {
            return new List<string>() { "@billId"};
        }
    }
}