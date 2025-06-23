using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;
using GCloud.Service;

namespace GCloud.Service
{
    public interface ICouponImageService : IAbstractService<CouponImage>
    {
        IQueryable<CouponImage> FindByCouponId(Guid id);
    }
}