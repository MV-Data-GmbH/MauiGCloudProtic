using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web;
using GCloud.Models;
using GCloud.Models.Charts;
using GCloud.Models.Domain;
using Microsoft.Owin.Security.Provider;

namespace GCloud.Service.Impl
{
    public class ReportService : IReportService
    {
        private readonly IRedeemService _redeemService;
        private readonly ICouponService _couponService;
        public ReportService(IRedeemService redeemService, ICouponService couponService)
        {
            _redeemService = redeemService;
            _couponService = couponService;
        }

        public IList<ColumnDataModel> GetCouponUsages(Guid storeId, Guid companyId, DateTime dateFrom, DateTime dateTo)
        {
            IQueryable<Redeem> data;
            if (storeId == Guid.Empty)
            {
                data = _redeemService.FindBy(x => x.Coupon.AssignedStores.Any(y => y.CompanyId == companyId));
            }
            else
            {
                data = _redeemService.FindBy(x => x.Coupon.AssignedStores.Any(y => y.Id == storeId));
            }

            var result = data.Where(x => x.RedeemDateTime > dateFrom && x.RedeemDateTime < dateTo).GroupBy(x => x.Coupon.Name).Select(x => new ColumnDataModel()
            {
                Name = x.Key,
                Value = x.Count()
            });
            return result.ToList();
        }

        public IList<ColumnDataModel> GetCouponUserUsages(Guid companyId, DateTime dateFrom, DateTime dateTo)
        {
            var data = _redeemService.FindBy(x => x.Coupon.CreatedUser.Companies.Any(y => y.Id == companyId))
                .Where(x=> x.RedeemDateTime > dateFrom && x.RedeemDateTime < dateTo)
                .GroupBy(x => new { x.User.Id, Name = x.User.FirstName + " " + x.User.LastName }).Select(x => new ColumnDataModel()
                {
                    Name = x.Key.Name,
                    Value = x.Count()
                }).OrderByDescending(x => x.Value).ToList();
            return data;
        }

        public IList<ColumnDataModel> GetCouponUsagePerTime(Guid companyId, DateTime dateFrom, DateTime dateTo)
        {
            var data = _redeemService.FindBy(x => x.Coupon.CreatedUser.Companies.Any(y => y.Id == companyId))
                .Where(x => x.RedeemDateTime > dateFrom && x.RedeemDateTime < dateTo)
                .GroupBy(x => SqlFunctions.DatePart("Hour", x.RedeemDateTime)).Select(x => new ColumnDataModel()
                {
                    Name = x.Key.Value.ToString(),
                    Value = x.Count()
                }).OrderBy(x => x.Name).ToList();
            return data;
        }
    }
}