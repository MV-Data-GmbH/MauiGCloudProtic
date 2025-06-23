using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GCloud;
using GCloud.Controllers.ViewModels.User;
using GCloud.Extensions;
using GCloud.Models.Domain;
using GCloud.Service;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace GCloud.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private ApplicationRoleManager roleManager;
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        public ApplicationRoleManager RoleManager
        {
            get => this.roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            private set => this.roleManager = value;
        }

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: Users
        [Authorize(Roles = "Administrators")]
        public ActionResult Index()
        {
            var adminRoleId = RoleManager.FindByName("Administrators")?.Id;
            var users = _userService.FindAll().Include(u => u.CreatedBy).Include(u => u.Roles).Where(x => x.Roles.All(r => r.RoleId != adminRoleId)).OrderBy(x => x.UserName);
            return View(users.ProjectTo<UserIndexViewModel>());
        }

        // GET: Users/Details/5
        [Authorize(Roles = "Administrators")]
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = _userService.FindById(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Create
        [Authorize(Roles = "Administrators")]
        public ActionResult Create()
        {
            var availableRoles = RoleManager.Roles.Where(x => x.Name != "Administrators");

            ViewBag.availableRoles = new SelectList(availableRoles, "Id", "Name", availableRoles.FirstOrDefault());
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators")]
        public ActionResult Create(UserCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var passwordPlain = model.Password;
                var user = Mapper.Map<User>(model);

                var currentUserId = User.Identity.GetUserId();
                user.CreatedById = currentUserId;

                var result = UserManager.Create(user, passwordPlain);
                if (result.Succeeded)
                {
                    var role = RoleManager.FindById(model.RoleId);
                    UserManager.AddToRoles(user.Id, role.Name);
                    return RedirectToAction("Index");
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, String.Join("\n", result.Errors));
                }
            }

            var availableRoles = RoleManager.Roles.Where(x => x.Name != "Administrators");

            ViewBag.availableRoles = new SelectList(availableRoles, "Id", "Name", availableRoles.FirstOrDefault());

            return View(model);
        }

        // GET: Users/Edit/5
        [Authorize(Roles = "Administrators")]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = _userService.FindById(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var model = Mapper.Map<UserEditViewModel>(user);

            var availableRoles = RoleManager.Roles.Where(x => x.Name != "Administrators");
            ViewBag.AvailableRoles = new SelectList(availableRoles, "Id", "Name", model.RoleId);
            return View(model);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators")]
        public ActionResult Edit(UserEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = Mapper.Map<User>(model);
                var userRoles = UserManager.GetRoles(user.Id);
                UserManager.RemoveFromRoles(user.Id, userRoles.ToArray());

                var role = RoleManager.FindById(model.RoleId);
                UserManager.AddToRoles(user.Id, role.Name);

                _userService.Update(user);
                return RedirectToAction("Index");
            }

            var availableRoles = RoleManager.Roles.Where(x => x.Name != "Administrators");
            ViewBag.AvailableRoles = new SelectList(availableRoles, "Id", "Name", model.RoleId);
            return View(model);
        }

        // GET: Users/Delete/5
        [Authorize(Roles = "Administrators")]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = _userService.FindById(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators")]
        public ActionResult DeleteConfirmed(string id)
        {
            User user = _userService.FindById(id);

            if (user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            UserManager.RemoveFromRoles(user.Id, user.Roles.Select(x => x.RoleId).ToArray());

            _userService.Delete(user);
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrators")]
        public ActionResult ToggleActive(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = _userService.FindById(userId);

            if (user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            user.IsActive = !user.IsActive;

            _userService.Update(user);
            return RedirectToAction("Index");
        }

        public ActionResult CustomerIndex()
        {
            var currentUserEntity = _userService.FindById(User.Identity.GetUserId());

            if (currentUserEntity == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            return View(currentUserEntity);
        }

        [Authorize(Roles = "Administrators")]
        [HttpGet]
        public ActionResult ResetCustomerPassword()
        {
            return PartialView("_ResetPassword", new ResetUserPasswordViewModel());
        }

        [Authorize(Roles = "Administrators")]
        [HttpPost]
        public ActionResult ResetCustomerPassword(ResetUserPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                UserManager.RemovePassword(model.UserId);
                var result = UserManager.AddPassword(model.UserId, model.NewPassword);
                if (result.Succeeded)
                {
                    return Content("Passwort erfolgreich geändert!");
                }
                else
                {
                    Response.StatusCode = 500;
                    return Content(string.Join(Environment.NewLine, result.Errors));
                }
            }

            Response.StatusCode = 500;
            return PartialView("_ResetPassword", model);
        }

        #region ConfirmEmail
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            if (result.Succeeded)
            {
                //User user1 = _userService.FindBy(x => x.Id == userId).FirstOrDefault();
                //user1.EmailConfirmed = true;
                var user = _userService.FindBy(x => x.Id == userId).FirstOrDefault();
                user.EmailConfirmed = true;
                return View("ConfirmEmail");
            }
            //var user =  _userService.FindBy(x => x.Id == userId).FirstOrDefault();
            //user.EmailConfirmed = true;
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ConfirmEmailText(string userId, string code)
        {
            dynamic model = new System.Dynamic.ExpandoObject();
            model.userId = userId;
            model.code = code;
            return View(model);
        }
        [AllowAnonymous]
        public ActionResult ConfirmEmailSend()
        {
            return View();
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> ResendActivationEmail(string username)
        {
            var user = _userService.FindbyUsername(username);
            if (!user.EmailConfirmed)
            {
                await SendMail(UserManager, GetBaseUrl(System.Web.HttpContext.Current), user);
                return RedirectToAction("ConfirmEmailSend", "Users");
            }

            return RedirectToAction("Login", "Home");
        }

        public static string GetBaseUrl(HttpContext currentContext)
        { 
            //bug: Not working due to ReverseProxy Setup. Returns the wrong baseUrl (http://127.0.0.1:8098)

            //var req = currentContext.Request;
            //string baseUrl = $"{req.Url.Scheme}://{req.Url.Authority}/";
            //if (!string.IsNullOrWhiteSpace(req.ApplicationPath) && !req.ApplicationPath.Equals("/"))
            //{
            //    baseUrl += req.ApplicationPath;
            //}
            //return baseUrl;

            return GCloud.Shared.BaseUrlContainer.BaseUri.ToString();
        }
        [NonAction]
        public static async Task SendMail(ApplicationUserManager UserManager, String baseUrl, User user)
        {
            var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
            var page = baseUrl + "Users/ConfirmEmailText?userId=" + HttpUtility.UrlEncode(user.Id) + "&code=" + HttpUtility.UrlEncode(code);

            using (var client = new HttpClient())
            using (var response = await client.GetAsync(page))
            using (var content = response.Content)
            {
                var result = await content.ReadAsStringAsync();

                if (result != null && result.Length >= 50)
                {
                    await UserManager.SendEmailAsync(user.Id, "Account Aktivierung", result);
                }
            }

            ////za lokal:
            //var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
            ////za produkciju:
            ////var page = baseUrl + "Users/ConfirmEmailText?userId=" + HttpUtility.UrlEncode(user.Id) + "&code=" + HttpUtility.UrlEncode(code);
            ////za lokal:
            //var page = baseUrl.Replace(":80", "") + "GCloud/" + "Users/ConfirmEmailText?userId=" + HttpUtility.UrlEncode(user.Id) + "&code=" + HttpUtility.UrlEncode(code);

            //using (var client = new HttpClient())
            //using (var response = await client.GetAsync(page))
            //using (var content = response.Content)
            //{
            //    var result = await content.ReadAsStringAsync();

            //    if (result != null && result.Length >= 50)
            //    {
            //        await UserManager.SendEmailAsync(user.Id, "Account Aktivierung", result);
            //    }
            //}
        }

        #endregion
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }
    }
}
