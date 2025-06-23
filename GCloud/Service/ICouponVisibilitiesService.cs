using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;

namespace GCloud.Service
{
    public interface ICouponVisibilitiesService : IAbstractService<AbstractCouponVisibility>
    {
        Coupon Deserialize(Coupon coupon, string json);
    }
}