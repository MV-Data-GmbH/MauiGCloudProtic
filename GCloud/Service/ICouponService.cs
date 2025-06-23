using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;

namespace GCloud.Service
{
    public interface ICouponService : IAbstractService<Coupon>
    {
        IQueryable<Coupon> FindByInterestedUser(string userId);
        IQueryable<Coupon> FindByInterestedUserAndCompany(string userId, Guid storeGuid);
        IQueryable<Coupon> FindByStoreId(Guid storeGuid);
        IQueryable<Coupon> FindByManagerId(string managerId);
        IQueryable<Coupon> FindByNameLike(string searchText);
        decimal GetSpecialProductValue(string productId);
    }
}