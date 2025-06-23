using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GCloud.Models.Domain;
using GCloud.Service;

namespace GCloud.Controllers
{
    public class RedeemsController : Controller
    {
        private IRedeemService _redeemService;

        public RedeemsController(IRedeemService redeemService)
        {
            _redeemService = redeemService;
        }

        // GET: Redeems
        public ActionResult Index()
        {
            var redeems = _redeemService.FindAll().Include(r => r.User).Include(r => r.Coupon);
            return View(redeems.ToList());
        }

        // GET: Redeems/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var redeem = _redeemService.FindById(id);
            if (redeem == null)
            {
                return HttpNotFound();
            }
            return View(redeem);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }
    }
}
