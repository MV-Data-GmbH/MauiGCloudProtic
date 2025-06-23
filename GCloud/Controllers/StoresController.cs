using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using FcmSharp;
using FcmSharp.Requests;
using FcmSharp.Settings;
using GCloud.Controllers.ModelBinders;
using GCloud.Controllers.ViewModels.Coupon;
using GCloud.Controllers.ViewModels.Store;
using GCloud.Extensions;
using GCloud.Helper;
using GCloud.Models;
using GCloud.Models.Domain;
using GCloud.Repository;
using GCloud.Service;
using GCloud.Shared.Exceptions.Store;
using Geocoding;
using Geocoding.Google;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using RefactorThis.GraphDiff;
using Notification = GCloud.Models.Domain.Notification;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GCloud.Controllers
{
    public class StoresController : Controller
    {
        private readonly IStoreService _storeService;
        private readonly IEBillCategoryRepository _eBillCategoryRepository;
        private readonly ICompanyService _companyService;
        private readonly ICountryService _countryService;
        private readonly ICouponService _couponService;
        private readonly ICouponImageService _couponImageService;
        private readonly ITagService _tagService;
        private readonly FcmClientSettings _fcmClientSettings;
        private readonly INotificationRepository _notificationRepository;
        private readonly IGeocoder _geocoder;

        public StoresController(IStoreService storeService, IEBillCategoryRepository eBillCategoryRepository,
            ICompanyService companyService, ICountryService countryService, ICouponService couponService,
            ICouponImageService couponImageService, FcmClientSettings fcmClientSettings, ITagService tagService,
            INotificationRepository notificationRepository)
        {
            _storeService = storeService;
            _eBillCategoryRepository = eBillCategoryRepository;
            _companyService = companyService;
            _countryService = countryService;
            _couponService = couponService;
            _couponImageService = couponImageService;
            _tagService = tagService;
            _fcmClientSettings = fcmClientSettings;
            _notificationRepository = notificationRepository;

            _geocoder = new GoogleGeocoder() {ApiKey = "AIzaSyDAgfRxfgawXgLa_4dCtGa_YZzKEs_hgo4"};
        }

        // GET: Stores
        [Authorize(Roles = "Administrators,Managers")]
        public ActionResult Index()
        {
            if (User.IsInRole("Administrators"))
            {
                var stores = _storeService.FindAll().Include(x => x.Company).OrderBy(x => x.Company.Name)
                    .ThenBy(x => x.Name);
                return View(stores.ToList());
            }
            else
            {
                var stores = _storeService.FindByUserId(User.Identity.GetUserId());
                return View(stores.ToList());
            }
        }

        // GET: Stores/Details/5
        public ActionResult Details(Guid id)
        {
            Store store = _storeService.FindById(id);
            if (store == null)
            {
                return HttpNotFound();
            }

            return View(store);
        }

        // GET: Stores/Create
        public ActionResult Create()
        {
            ViewBag.CompanyId = new SelectList(_companyService.FindAll(), "Id", "Name");
            var countries = _countryService.FindAll();
            ViewBag.Countries = new SelectList(countries, "Id", "Name",
                countries.FirstOrDefault(x => x.Name == "Austria")?.Id.ToString());

            ViewData["eBillkategories"] = new SelectList(_eBillCategoryRepository.FindAll().ToList(), "Name", "Name");
            return View();
        }

        // POST: Stores/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "Name,CompanyId,City,CountryId,Street,HouseNr,Plz,SelectedTags,ImageData,EBillCategory,EuroPerPoints,PointsPerEuro")]
            StoreCreateViewModel model)
        {
            if (model.EBillCategory == null)
            {
                ModelState.AddModelError("EBillCategory", "Es muss eine E-Bill-Kategorie festgelegt werden");
            }

            if (ModelState.IsValid)
            {
                var store = Mapper.Map<Store>(model);

                var eBillCategory = _eBillCategoryRepository.FindBy(x => x.Name == model.EBillCategory)
                    .FirstOrDefault();
                if (eBillCategory == null)
                {
                    eBillCategory = new EBillCategory()
                    {
                        Name = model.EBillCategory
                    };
                    _eBillCategoryRepository.Add(eBillCategory);
                    _eBillCategoryRepository.Save();
                }

                store.EBillCategory = eBillCategory;

                var country = _countryService.FindById(store.CountryId);

                //var result = _geocoder
                //    .GeocodeAsync($"{store.Street} {store.HouseNr} {store.Plz} {store.City} {country.Name}").Result
                //    .First();
                //store.Latitude = result.Coordinates.Latitude;
                //store.Longitude = result.Coordinates.Longitude;

                store.Tags = _tagService.FindTagsByName(model.SelectedTags);
              
                _storeService.Add(store);
                //var c = model.ImageData;
                //var d = store.Id;
                SaveImage(model.ImageData, store.Id);
                return RedirectToAction("Index");
            }

            ViewBag.CompanyId = new SelectList(_companyService.FindAll(), "Id", "Name");
            var countries = _countryService.FindAll();
            ViewBag.Countries = new SelectList(countries, "Id", "Name",
                countries.FirstOrDefault(x => x.Name == "Austria")?.Id.ToString());
            ViewData["eBillkategories"] = new SelectList(_eBillCategoryRepository.FindAll().ToList(), "Name", "Name");

            return View(model);
        }

        // GET: Stores/Edit/5
        public ActionResult Edit(Guid id)
        {
            Store store = _storeService.FindById(id);
            if (store == null)
            {
                return HttpNotFound();
            }

            ViewBag.CompanyId = new SelectList(_companyService.FindAll(), "Id", "Name", store.CompanyId);
            var countries = _countryService.FindAll();
            ViewBag.Countries = new SelectList(countries, "Id", "Name",
                countries.FirstOrDefault(x => x.Name == "Austria")?.Id.ToString());
            ViewBag.Tags = new SelectList(store.Tags, "Name", "Name");

            ViewData["eBillkategories"] = new SelectList(_eBillCategoryRepository.FindAll().ToList(), "Name", "Name");

            return View(Mapper.Map<StoreEditViewModel>(store));
        }

        // POST: Stores/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include =
                "Id,Name,CreationDateTime,CompanyId,CountryId,City,Street,HouseNr,Plz,ApiToken,SelectCoupons,SelectedTags,ImageData,EBillCategory,EuroPerPoints,PointsPerEuro")]
            StoreEditViewModel model)
        {
            if (model.EBillCategory == null)
            {
                ModelState.AddModelError("EBillCategory", "Es muss eine E-Bill-Kategorie festgelegt werden");
            }

            if (ModelState.IsValid)
            {
                var storeToSave = Mapper.Map<Store>(model);
                if (model?.SelectCoupons != null)
                {
                    var couponIds = model.SelectCoupons.Select(x => new Guid(x)).ToList();
                    var coupons = _couponService.FindAll().Where(x => couponIds.Contains(x.Id)).ToList();
                    storeToSave.Coupons = coupons;
                }

                var eBillCategory = _eBillCategoryRepository.FindBy(x => x.Name == model.EBillCategory)
                    .FirstOrDefault();
                if (eBillCategory == null)
                {
                    eBillCategory = new EBillCategory()
                    {
                        Name = model.EBillCategory
                    };
                    _eBillCategoryRepository.Add(eBillCategory);
                    _eBillCategoryRepository.Save();
                }

                var storeFromDb = _storeService.FindById(storeToSave.Id);
                storeFromDb.EBillCategory = eBillCategory;
                _storeService.Update(storeFromDb);

                var country = _countryService.FindById(storeToSave.CountryId);
                //var result = _geocoder
                //    .GeocodeAsync(
                //        $"{storeToSave.Street} {storeToSave.HouseNr} {storeToSave.Plz} {storeToSave.City} {country.Name}")
                //    .Result;
                //storeToSave.Latitude = result.First().Coordinates.Latitude;
                //storeToSave.Longitude = result.First().Coordinates.Longitude;

                storeToSave.Tags = _tagService.FindTagsByName(model?.SelectedTags);
                _storeService.Update(storeToSave,
                    with => with.AssociatedCollection(p => p.Coupons).AssociatedCollection(p => p.Tags));
                if (!String.IsNullOrWhiteSpace(model.ImageData))
                {
                    SaveImage(model.ImageData, model.Id);
                }

                return RedirectToAction("Index");
            }

            var store = _storeService.FindById(model.Id);
            ViewBag.CompanyId = new SelectList(_companyService.FindAll(), "Id", "Name", store.CompanyId);
            var countries = _countryService.FindAll();
            ViewBag.Countries = new SelectList(countries, "Id", "Name",
                countries.FirstOrDefault(x => x.Name == "Austria")?.Id.ToString());
            ViewBag.Tags = new SelectList(store.Tags, "Name", "Name");
            ViewData["eBillkategories"] = new SelectList(_eBillCategoryRepository.FindAll().ToList(), "Name", "Name");

            return View(model);
        }

        // GET: Stores/Delete/5
        public ActionResult Delete(Guid id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Store store = _storeService.FindById(id);
            if (store == null)
            {
                return HttpNotFound();
            }

            return View(store);
        }

        // POST: Stores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Store store = _storeService.FindById(id);
            //store.Id = id;
            //_storeService.Delete(store);
            _storeService.DeleteStore(id);
            return RedirectToAction("Index");

        }

        public async Task<ActionResult> NotifyFollowingUsers(Guid id)
        {
            using (var fcmClient = new FcmClient(_fcmClientSettings))
            {
                var store = _storeService.FindById(id);
                await fcmClient.SendAsync(new FcmMessage()
                {
                    Message = new Message()
                    {
                        Topic = id.ToString(),
                        //Notification = new FcmSharp.Requests.Notification()
                        //{
                        //    Title = "asdf",
                        //    Body = "asdf"
                        //},
                        AndroidConfig = new AndroidConfig()
                        {
                            Data = new System.Collections.Generic.Dictionary<string, string>
                            {
                                { "title", "Notification Test" },
                                { "body", "Das ist ein Test" },
                                { "storeGuid", store.Id.ToString() },
                                { "storeName", store.Name },
                                { "cashbackEnabled", (store?.Company?.IsCashbackEnabled ?? false).ToString() }
                            }
                        },
                        ApnsConfig = new ApnsConfig()
                        {
                            Payload = new ApnsConfigPayload()
                            {
                                Aps = new Aps()
                                {
                                    Alert = new ApsAlert()
                                    {
                                        Title = "Notification Test",
                                        Body = "Das ist ein Test"
                                    },
                                    Sound = "default",
                                    Badge = 1
                                },
                                CustomData = new System.Collections.Generic.Dictionary<string, object>
                                {
                                    { "storeGuid", store.Id.ToString() },
                                }
                            }
                        }
                    }
                });
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult EditNotifications(Guid id)
        {
            var store = _storeService.FindById(id);

            if (store == null)
            {
                throw new StoreNotFoundException(id);
            }

            var model = Mapper.Map<NotificationEditViewModel>(store);

            return View("Notifications/Index", model);
        }

        [HttpPost]
        public ActionResult EditNotifications([ModelBinder(typeof(NotificationModelBinder))]
            NotificationEditViewModel model)
        {
            var store = _storeService.FindById(model.StoreId);

            if (store == null)
            {
                throw new StoreNotFoundException(model.StoreId);
            }

            // I need to perform the DTO mapping without automapper, because of the complex mapping behind it it's easier that way
            var selectedDays = model.DaySelection
                .Where(day => day.Abend || day.Mittag || day.Nachmittag || day.Vormittag).ToDictionary(x => x.DayOfWeek,
                    x =>
                    {
                        var list = new List<TimeSpan?>
                        {
                            x.Vormittag ? new TimeSpan(8, 0, 0) : (TimeSpan?) null,
                            x.Mittag ? new TimeSpan(10, 0, 0) : (TimeSpan?) null,
                            x.Nachmittag ? new TimeSpan(14, 0, 0) : (TimeSpan?) null,
                            x.Abend ? new TimeSpan(18, 0, 0) : (TimeSpan?) null
                        }.Where(time => time.HasValue);
                        return list;
                    });

            var notifications = selectedDays.SelectMany(kvp => selectedDays[kvp.Key].Cast<TimeSpan>().Select(time =>
                new Notification
                {
                    DayOfWeek = kvp.Key,
                    NotifyTime = time,
                    Message = model.Message
                })).ToList();

            _notificationRepository.Delete(store.Notifications);
            store.Notifications = notifications;
            _storeService.Update(store);

            return View("Notifications/Index", model);
        }

        [HttpGet]
        public ActionResult LoadStoreImage(Guid? id)
        {
            string filePath;

            if (id.HasValue)
            {
                var directory = Server.MapPath("~/UploadedFiles/Stores");
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                filePath = Path.Combine(directory, id.ToString());
                if (!System.IO.File.Exists(filePath))
                {
                    filePath = Path.Combine(Server.MapPath("~/Content"), "nav_header_background.jpg");
                }
            }
            else
            {
                filePath = Path.Combine(Server.MapPath("~/Content"), "nav_header_background.jpg");
            }

            return File(filePath, "image/jpeg");
        }

        private void SaveImage(string imageData, Guid guid)
        {
            var newFileName = guid.ToString();
            var directory = Server.MapPath("~/UploadedFiles/Stores");
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
                using (var resizedImage = ImageUtils.ResizeImageToFixedHeight(image, 500))
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