using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using GCloud.Controllers.ViewModels;
using GCloud.Controllers.ViewModels.Coupon;
using GCloud.Extensions;
using GCloud.Helper;
using GCloud.Models;
using GCloud.Models.Domain;
using GCloud.Repository;
using GCloud.Service;
using Microsoft.AspNet.Identity;
using RefactorThis.GraphDiff;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GCloud.Controllers
{
    public class CouponsController : Controller
    {
        private readonly ICouponService _couponService;
        private readonly IStoreService _storeService;
        private readonly IUserService _userService;
        private readonly ICouponImageService _couponImageService;
        private readonly ICouponVisibilitiesRepository _couponVisibilitiesRepository;
        private readonly ICouponVisibilitiesService _couponVisibilitiesService;

        public CouponsController(
            ICouponService couponService, 
            IStoreService storeService, 
            IUserService userService, 
            ICouponImageService couponImageService, 
            ICouponVisibilitiesRepository couponVisibilitiesRepository,
            ICouponVisibilitiesService couponVisibilitiesService)
        {
            _couponService = couponService;
            _storeService = storeService;
            _userService = userService;
            _couponImageService = couponImageService;
            _couponVisibilitiesRepository = couponVisibilitiesRepository;
            _couponVisibilitiesService = couponVisibilitiesService;
        }

        [Authorize(Roles = "Managers")]
        // GET: Coupons
        public  ActionResult Index()
        {
            var coupons = _couponService.FindByManagerId(User.Identity.GetUserId()).Include(c => c.AssignedStores).Include(c => c.CreatedUser).OrderByName();
            var stores = _storeService.FindByUserId(User.Identity.GetUserId());
            ViewBag.stores = new SelectList(stores, "Id", "Name");
            return View(coupons.ToList());
        }

        [Authorize(Roles = "Administrators")]
        // GET: Coupons
        public ActionResult IndexAdmin()
        {
            var stores = _storeService.FindByUserId(User.Identity.GetUserId());

            var coupons = _couponService.FindAll().Include(c => c.AssignedStores).Include(c => c.CreatedUser).OrderByName();
            ViewBag.stores = new SelectList(stores, "Id", "Name");
            return View(coupons.ToList());
        }

        // GET: Coupons/Details/5
        public ActionResult Details(Guid id)
        {
            Coupon coupon = _couponService.FindById(id);
            if (coupon == null)
            {
                return HttpNotFound();
            }
            return View(coupon);
        }

        // GET: Coupons/Create
        [Authorize(Roles = "Managers")]
        public ActionResult Create()
        {
            var currentUserId = User.Identity.GetUserId();
            var model = new CouponCreateViewModel();
            var stores = _storeService.FindByUserId(currentUserId);
            model.AssignedStores = stores.Select(x => new CheckBoxListItem
            {
                Id = x.Id,
                Display = x.Name,
                IsChecked = false
            }).ToList();

            return View(model);
        }

        // POST: Coupons/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Managers")]
        public ActionResult Create([Bind(Include = "Id,Name,ShortDescription,MaxRedeems,Value,CouponType,CouponScope,AssignedStores,ImageData,ArticleNumber,Enabled")] CouponCreateViewModel couponModel, [Bind(Include = "couponVisibilities")] string couponVisibilities)
        {
            if (ModelState.IsValid)
            {
                var coupon = Mapper.Map<Coupon>(couponModel);
                var currentUserId = User.Identity.GetUserId();
                var storeIds = couponModel.AssignedStores.Where(x => x.IsChecked).Select(x => x.Id).ToList();
                coupon.CreatedUserId = currentUserId;
                _couponVisibilitiesService.Deserialize(coupon, couponVisibilities);

                var couponImage = SaveImage(coupon);
                if (couponImage != null)
                {
                    coupon.CouponImages = new List<CouponImage> { couponImage };
                }

                coupon.AssignedStores = _storeService.FindBy(x => storeIds.Contains(x.Id)).ToList();
                _couponService.Add(coupon);

                return RedirectToAction("Index");
            }

            return View(couponModel);
        }

        // GET: Coupons/Edit/5
        [Authorize(Roles = "Managers")]
        public ActionResult Edit(Guid id)
        {
            var currentUser = User.Identity.GetUserId();
            var coupon = _couponService.FindById(id);
            if (coupon == null)
            {
                return HttpNotFound();
            }

            var model = Mapper.Map<CouponEditViewModel>(coupon);
            ViewBag.VisibilityTypes = typeof(AbstractCouponVisibility).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(AbstractCouponVisibility)) && !type.IsAbstract);
            ViewBag.couponId = coupon.Id;
            model.AssignedStores = _storeService.FindByUserId(currentUser).ToList().Select(x => new CheckBoxListItem()
            {
                Display = x.Name,
                IsChecked = coupon.AssignedStores.Any(s => s.Id == x.Id),
                Id = x.Id
            }).ToList();
            
            ViewBag.VisibilityTypes = typeof(AbstractCouponVisibility).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(AbstractCouponVisibility)) && !type.IsAbstract);
            ViewBag.couponId = coupon.Id;

            return View(model);
        }

        // POST: Coupons/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Managers")]
        public ActionResult Edit([Bind(Include = "Id,Name,ShortDescription,MaxRedeems,Value,CouponType,CouponScope,CreatedUserId,AssignedStores,ImageData,ArticleNumber,Enabled")] CouponEditViewModel model, [Bind(Include = "couponVisibilities")] string couponVisibilities)
        {
            if (ModelState.IsValid)
            {
                var coupon = Mapper.Map<Coupon>(model);
                var storeIds = coupon.AssignedStores.Select(x => x.Id).ToList();
                var couponFromDb = _couponService.FindById(coupon.Id);
                coupon.Visibilities = couponFromDb.Visibilities.ToList();

                _couponVisibilitiesService.Deserialize(coupon, couponVisibilities);

                if (!string.IsNullOrWhiteSpace(coupon?.ImageData))
                {
                    var couponImage = SaveImage(coupon);
                    if (couponImage != null)
                    {
                        // Bestehende Bilder löschen, oder zumindest den Flag auf Deleted setzten
                        var currentCouponImages = _couponImageService.FindByCouponId(coupon.Id).ToList();
                        currentCouponImages.ForEach(x=> _couponImageService.Delete(x));

                        _couponImageService.Add(couponImage);
                        couponImage.CouponId = model.Id;
                        _couponImageService.Update(couponImage);
                    }
                }
                else
                {
                    var currentCouponImages = _couponImageService.FindByCouponId(coupon.Id).ToList();
                    currentCouponImages.ForEach(x => _couponImageService.Delete(x));
                }

                coupon.AssignedStores = _storeService.FindByUserId(User.Identity.GetUserId()).Where(x => storeIds.Contains(x.Id)).ToList();
                _couponService.Update(coupon, with => with.AssociatedCollection(p => p.AssignedStores).OwnedCollection(p => p.Visibilities).OwnedCollection(p => p.UsageActions, with2 => with2.AssociatedEntity(p2 => p2.Coupon)));
                return RedirectToAction("Index");
            }
            ViewBag.AssignedStoreId = new SelectList(_storeService.FindByUserId(User.Identity.GetUserId()), "Id", "Name", model.AssignedStores.FirstOrDefault());
            ViewBag.couponVisibilities = couponVisibilities;

            return View(model);
        }

        // GET: Coupons/Delete/5
        [Authorize(Roles = "Managers")]
        public ActionResult Delete(Guid id)
        {
            Coupon coupon = _couponService.FindById(id);
            //_couponService.Delete(coupon);

            if (coupon == null)
            {
                return HttpNotFound();
            }
            return View(coupon);
        }

        // POST: Coupons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Managers")]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Coupon coupon = _couponService.FindById(id);
            _couponService.Delete(coupon);
            return RedirectToAction("Index");
        }

        public ActionResult SelectCoupons(Store store)
        {
            ViewBag.store = store;
            return PartialView("_SelectCoupons", _couponService.FindByManagerId(User.Identity.GetUserId()).Include(c => c.AssignedStores).Include(c => c.CreatedUser).OrderByName().ToList());
        }

        private CouponImage SaveImage(Coupon coupon, string origFilename = "")
        {
            if (string.IsNullOrWhiteSpace(coupon?.ImageData)) return null;
            var newFileName = Guid.NewGuid().ToString();
            var filePath = Path.Combine(Server.MapPath("~/UploadedFiles"), newFileName);
            while (System.IO.File.Exists(filePath))
            {
                newFileName = Guid.NewGuid().ToString();
                filePath = Path.Combine(Server.MapPath("~/UploadedFiles"), newFileName);
            }

            var image = GetImageFromString(coupon?.ImageData);
            image.Save(filePath);

            return new CouponImage
            {
                FileName = newFileName,
                OrigFileName = origFilename,
                CreatorId = User.Identity.GetUserId()
            };
        }
        
        [HttpGet]
        public ActionResult LoadCouponImage(Guid id)
        {
            var couponImage = _couponImageService.FindByCouponId(id).OrderByDescending(x => x.CreationDateTime).FirstOrDefault();
            if (couponImage != null)
            {
                var filePath = Path.Combine(Server.MapPath("~/UploadedFiles"), couponImage.FileName);

                if (!System.IO.File.Exists(filePath))
                {
                    filePath = Path.Combine(Server.MapPath("~/Content"), "No-Image-Available.png");
                }
                return File(filePath, "image/jpeg");
            }
            else
            {
                var filePath = Path.Combine(Server.MapPath("~/Content"), "No-Image-Available.png");
                return File(filePath, "image/jpeg");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Managers,Administrators")]
        public ActionResult SearchCoupons(string searchText)
        {
            var coupons = _couponService.FindByNameLike(searchText);

            if (!User.IsInRole("Administrators"))
            {
                coupons = coupons.WhereManagerOwnsCoupon(User.Identity.GetUserId());
            }
                
            var stores = _storeService.FindByUserId(User.Identity.GetUserId());
            ViewBag.stores = new SelectList(stores, "Id", "Name");

            return View("Index", coupons.ToList());
        }
        [HttpGet]
        [Authorize(Roles = "Managers,Administrators")]
        public ActionResult SearchCouponsApi(string query)
        {
            var userId = User.Identity.GetUserId();
            var coupons = _couponService.FindByNameLike(query);

            if (!User.IsInRole("Administrators"))
            {
                coupons = coupons.Where(x => x.CreatedUserId == userId);
            }
            var suggestions = coupons.OrderBy(x => x.Name).Take(5).Select(x => new CouponSearchResultSuggestionModel
            {
                Data = x.Id.ToString(),
                Value = x.Name
            });
            var result = new CouponSearchResultModel()
            {
                Query = query,
                Suggestions = suggestions.ToList()
            };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public PartialViewResult CouponsByStoreId(Guid? storeId)
        {
            if (storeId.HasValue)
            {
                return PartialView("_IndexTable", _couponService.FindByStoreId(storeId.Value).OrderByName().ToList());
            }
            else
            {
                return PartialView("_IndexTable", _couponService.FindByManagerId(User.Identity.GetUserId()).OrderByName().ToList());
            }
        }

        [Authorize(Roles = "Managers,Administrators")]

        public ActionResult ToggleEnabled(Guid couponId)
        {
            var coupon = _couponService.FindById(couponId);

            if (coupon != null)
            {
                coupon.Enabled = !coupon.Enabled;
                _couponService.Update(coupon);
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }

        [NonAction]
        public string GetStringFromImage(Image image)
        {
            if (image != null)
            {
                var ic = new ImageConverter();
                var buffer = (byte[])ic.ConvertTo(image, typeof(byte[]));
                return Convert.ToBase64String(
                    buffer,
                    Base64FormattingOptions.InsertLineBreaks);
            }
            else
                return null;
        }
        [NonAction]
        public static Image GetImageFromString(string base64String)
        {
            if (base64String.Contains("data:image/png;base64,"))
            {
                base64String = base64String.Replace("data:image/png;base64,", "");
            }
            if (base64String.Contains("data:image/jpeg;base64,"))
            {
                base64String = base64String.Replace("data:image/jpeg;base64,", "");
            }
            var buffer = Convert.FromBase64String(base64String);

            if (buffer != null)
            {
                ImageConverter ic = new ImageConverter();
                return ic.ConvertFrom(buffer) as Image;
            }
            else
                return null;
        }
    }
}
