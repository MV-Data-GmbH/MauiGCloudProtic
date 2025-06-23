using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using GCloud.Models.Domain;
using GCloud.Service;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;

namespace GCloud.Controllers
{
    public class CouponVisibilityController : Controller
    {
        private readonly ICouponService _couponService;
        private readonly ICouponVisibilitiesService _couponVisibilitiesService;
        private readonly IStoreService _storeService;

        public CouponVisibilityController(ICouponService couponService, ICouponVisibilitiesService couponVisibilitiesService, IStoreService storeService)
        {
            _couponService = couponService;
            _couponVisibilitiesService = couponVisibilitiesService;
            _storeService = storeService;
        }

        public ActionResult Index(Guid couponId, string toBeAdded = null)
        {
            var coupon = couponId == Guid.Empty ? new Coupon() : _couponService.FindById(couponId);

            if (!string.IsNullOrWhiteSpace(toBeAdded))
            {
                _couponVisibilitiesService.Deserialize(coupon, toBeAdded);
            }
            return PartialView(coupon);
        }

        public ActionResult Create(Guid couponId, string toBeAdded = null)
        {
            var coupon = couponId == Guid.Empty? new Coupon() : _couponService.FindById(couponId);
            ViewBag.couponId = coupon.Id;
            ViewBag.VisibilityTypes = typeof(AbstractCouponVisibility).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(AbstractCouponVisibility)) && !type.IsAbstract).Select(x => new CouponVisibilityNamesViewModel() {FullName = x.FullName, Name = x.Name, HumanReadableName = x.GetMethod("GetHumanReadableName")?.Invoke(Activator.CreateInstance(x),null) as string}).ToList();
            if (!string.IsNullOrWhiteSpace(toBeAdded))
            {
                _couponVisibilitiesService.Deserialize(coupon, toBeAdded);
            }
            return PartialView(coupon);
        }

        public ActionResult Delete(Guid couponId, Guid couponvaliditGuid)
        {
            var coupon = _couponService.FindById(couponId);
            if (coupon.Visibilities.Any(x => x.Id == couponvaliditGuid))
            {
                coupon.Visibilities.Remove(coupon.Visibilities.FirstOrDefault(x => x.Id == couponvaliditGuid));
                _couponService.Update(coupon);
            }
            return RedirectToAction("Edit", "Coupons", new { id = couponId });
        }
    }

    public class CouponVisibilityNamesViewModel
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public string HumanReadableName { get; set; }
    }
}