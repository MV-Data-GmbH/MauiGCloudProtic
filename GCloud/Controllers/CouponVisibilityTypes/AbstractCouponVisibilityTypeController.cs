using System.Web.Mvc;
using GCloud.Models.Domain;

namespace GCloud.Controllers.CouponVisibilityTypes
{
    public abstract class AbstractCouponVisibilityTypeController<T> : Controller where T : AbstractCouponVisibility, new()
    {
        [HttpPost]
        public ActionResult Create([Bind(Include = "Id,Name,ShortDescription,MaxRedeems,Value,CouponType,CreatedUserId,AssignedStoreId,ImageData,CouponScope,ArticleNumber,Visibilities")] Coupon coupon)
        {
            var modal = new T()
            {
                Coupon = coupon
            };

            return PartialView("~/Views/CouponVisibility/CouponVisibilityTypes/" + typeof(T).Name + ".cshtml", modal);
        }
    }
}