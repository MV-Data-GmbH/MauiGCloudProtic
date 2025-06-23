using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using GCloud.Controllers.ViewModels.Company;
using GCloud.Extensions;
using GCloud.Helper;
using GCloud.Models.Domain;
using GCloud.Service;

namespace GCloud.Controllers
{
    [Authorize(Roles = "Administrators")]
    public class CompaniesController : Controller
    {
        private readonly ICompanyService _companyService;
        private readonly IUserService _userService;

        public CompaniesController(ICompanyService companyService, IUserService userService)
        {
            _companyService = companyService;
            _userService = userService;
        }

        // GET: Companies
        public ActionResult Index()
        {
            var companies = _companyService.FindAll().Include(c => c.User);
            return View(companies.ToList());
        }

        // GET: Companies/Details/5
        public ActionResult Details(Guid id)
        {
            Company company = _companyService.FindById(id);
            if (company == null)
            {
                return HttpNotFound();
            }
            return View(company);
        }

        // GET: Companies/Create
        public ActionResult Create()
        {
            ViewBag.UserId = new SelectList(_userService.FindAll().ToDictionary(x => x.Id, x => $"{x.UserName} - {x.FirstName} {x.LastName} ({x.Email})"), "key", "value");
            return View();
        }

        // POST: Companies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CompanyCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var company = Mapper.Map<Company>(model);
                _companyService.Add(company);
                SaveLogo(model.LogoData, company.Id);
                return RedirectToAction("Index");
            }

            ViewBag.UserId = new SelectList(_userService.FindAll().ToDictionary(x => x.Id, x => $"{x.UserName} - {x.FirstName} {x.LastName} ({x.Email})"), "key", "value");
            return View(model);
        }

        // GET: Companies/Edit/5
        public ActionResult Edit(Guid id)
        {
            Company company = _companyService.FindById(id);
            if (company == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = new SelectList(_userService.FindAll().ToDictionary(x => x.Id, x => $"{x.UserName} - {x.FirstName} {x.LastName} ({x.Email})"), "key", "value");
            return View(Mapper.Map<CompanyEditViewModel>(company));
        }

        // POST: Companies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CompanyEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var company = Mapper.Map<Company>(model);
                //SaveLogo(model.LogoData, model.Id);
                _companyService.Update(company);
                return RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(_userService.FindAll().ToDictionary(x => x.Id, x => $"{x.UserName} - {x.FirstName} {x.LastName} ({x.Email})"), "key", "value");
            return View(model);
        }

        // GET: Companies/Delete/5
        public ActionResult Delete(Guid id)
        {
            Company company = _companyService.FindById(id);
            if (company == null)
            {
                return HttpNotFound();
            }
            return View(company);
        }

        // POST: Companies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Company company = _companyService.FindById(id);
            _companyService.Delete(company);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult LoadLogoImage(Guid? id)
        {
            string filePath;

            if (id.HasValue)
            {
                var directory = Server.MapPath("~/UploadedFiles/CompanyLogos");
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                filePath = Path.Combine(directory, id.ToString());
                if (!System.IO.File.Exists(filePath))
                {
                    filePath = Path.Combine(Server.MapPath("~/Content"), "No-Image-Available.png");
                }
            }
            else
            {
                filePath = Path.Combine(Server.MapPath("~/Content"), "No-Image-Available.png");
            }

            return File(filePath, "image/jpeg");
        }

        private void SaveLogo(string imageData, Guid guid)
        {
            var newFileName = guid.ToString();
            var directory = Server.MapPath("~/UploadedFiles/CompanyLogos");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            var filePath = Path.Combine(directory, newFileName);

            while (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            using (var image = CouponsController.GetImageFromString(imageData))
            {
                using (var resizedImage = ImageUtils.ResizeImageToFixedHeight(image, 150))
                {
                    resizedImage.Save(filePath);
                }
            }
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
