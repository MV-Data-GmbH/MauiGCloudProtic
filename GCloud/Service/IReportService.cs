using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GCloud.Models;
using GCloud.Models.Charts;

namespace GCloud.Service
{
    public interface IReportService
    {
        IList<ColumnDataModel> GetCouponUsages(Guid storeId, Guid companyId, DateTime dateFrom, DateTime dateTo);
        IList<ColumnDataModel> GetCouponUserUsages(Guid companyId, DateTime dateFrom, DateTime dateTo);
        IList<ColumnDataModel> GetCouponUsagePerTime(Guid companyId, DateTime dateFrom, DateTime dateTo);
    }
}