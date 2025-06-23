using AutoMapper;
using GCloud.Controllers.ViewModels;
using GCloud.Controllers.ViewModels.SpecialProduct;
using GCloud.Models.Domain;
using GCloud.Repository;
using GCloud.Service;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace GCloud.Controllers
{
    public class SpecialProductController : Controller
    {
        private readonly ISpecialProductService _specialProductService;
        private readonly ICouponService _couponService;
        private readonly IStoreService _storeService;
        private readonly IUserService _userService;
        private readonly ICouponImageService _couponImageService;
        private readonly ICouponVisibilitiesRepository _couponVisibilitiesRepository;
        private readonly ICouponVisibilitiesService _couponVisibilitiesService;

        public SpecialProductController(
            ISpecialProductService specialProductService,
            ICouponService couponService,
            IStoreService storeService,
            IUserService userService,
            ICouponImageService couponImageService,
            ICouponVisibilitiesRepository couponVisibilitiesRepository,
            ICouponVisibilitiesService couponVisibilitiesService)
        {
            _specialProductService = specialProductService;
            _couponService = couponService;
            _storeService = storeService;
            _userService = userService;
            _couponImageService = couponImageService;
            _couponVisibilitiesRepository = couponVisibilitiesRepository;
            _couponVisibilitiesService = couponVisibilitiesService;
        }


        // GET: SpecialProduct
        [Authorize(Roles = "Managers")]
        public ActionResult Index()
        {     
                var specialProducts = _specialProductService.FindByUserId(User.Identity.GetUserId());
                return View(specialProducts.ToList());
        }

        // GET: SpecialProduct/Create/
        [Authorize(Roles = "Managers")]
        public ActionResult Create()
        {
            var currentUserId = User.Identity.GetUserId();
            var model = new SpecialProductCreateViewModel();
            var stores = _storeService.FindByUserId(currentUserId);
            model.AssignedStores = stores.Select(x => new CheckBoxListItem
            {
                Id = x.Id,
                Display = x.Name,
                IsChecked = false
            }).ToList();

            return View(model);
        }

        // POST: SpecialProduct/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Managers")]
        public ActionResult Create([Bind(Include = "Id,Name,ShortDescription,Value,AssignedStores,Enabled")] SpecialProductCreateViewModel specialProductModel)
        {
            if (ModelState.IsValid)
            {
                var specialProduct = Mapper.Map<SpecialProduct>(specialProductModel);
                var currentUserId = User.Identity.GetUserId();
                var storeIds = specialProductModel.AssignedStores.Where(x => x.IsChecked).Select(x => x.Id).ToList();
                specialProduct.CreatedUserId = currentUserId;
               

              

                specialProduct.AssignedStores = _storeService.FindBy(x => storeIds.Contains(x.Id)).ToList();
                _specialProductService.Add(specialProduct);

                return RedirectToAction("Index");
            }

            return View(specialProductModel);
        }

        // GET: SpecialProduct/Edit/5
        public ActionResult Edit(Guid id)
        {
            SpecialProduct specialProduct = _specialProductService.FindById(id);
            if (specialProduct == null)
            {
                return HttpNotFound();
            }

            return View(Mapper.Map<SpecialProductEditViewModel>(specialProduct));
        }


        // POST: SpecialProduct/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,ShortDescription,Value")] SpecialProductEditViewModel model)
        {

            if (ModelState.IsValid)
            {
                model.CreatedUserId = User.Identity.GetUserId();
                var specialProductToSave = Mapper.Map<SpecialProduct>(model);
                

                _specialProductService.Update(specialProductToSave);

                return RedirectToAction("Index");
            }

            return View(model);
        }



        // GET: SpecialProduct/Delete/5
        public ActionResult Delete(Guid id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SpecialProduct specialProduct = _specialProductService.FindById(id);
            if (specialProduct == null)
            {
                return HttpNotFound();
            }

            return View(specialProduct);
        }

        // POST: SpecialProduct/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            SpecialProduct specialProduct = _specialProductService.FindById(id);
            _specialProductService.Delete(specialProduct);
            return RedirectToAction("Index");
        }


    }
}