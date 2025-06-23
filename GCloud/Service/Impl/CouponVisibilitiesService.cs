using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;
using GCloud.Repository;
using Newtonsoft.Json;

namespace GCloud.Service.Impl
{
    public class CouponVisibilitiesService : AbstractService<AbstractCouponVisibility>, ICouponVisibilitiesService
    {
        public CouponVisibilitiesService(IAbstractRepository<AbstractCouponVisibility> repository) : base(repository)
        {
        }

        public Coupon Deserialize(Coupon coupon, string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return coupon;
            }
            dynamic couponsObject = JsonConvert.DeserializeObject(json);
            foreach (var jsonToken in couponsObject)
            {
                string typeStr = jsonToken.key.ToString();
                string valueStr = jsonToken.value.ToString();
                var type = Type.GetType(typeStr);
                var abstractCouponVisibility = (AbstractCouponVisibility)JsonConvert.DeserializeObject(valueStr.ToString(), type);
                abstractCouponVisibility.Coupon = coupon;
                
                if (jsonToken.delete.ToString() == bool.TrueString)
                {
                    coupon.Visibilities.Remove(coupon.Visibilities.FirstOrDefault(x => x.Id == (Guid)jsonToken.value.Id));
                }
                else
                {
                    coupon.Visibilities.Add(abstractCouponVisibility);
                }
            }

            return coupon;
        }
    }
}