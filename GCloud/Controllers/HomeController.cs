using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using GCloud.Controllers.ViewModels.Home;
using GCloud.Models;
using GCloud.Models.Domain;
using GCloud.Service;
using GCloud.Shared.Exceptions.User;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;

namespace GCloud.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationUserManager _userManager;
        private IUserService _userService;

        public HomeController(IUserService userService)
        {
            _userService = userService;
        }

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("Administrators"))
            {
                return RedirectToAction("Index", "Users");
            }

            if (User.Identity.IsAuthenticated && User.IsInRole("Managers"))
            {
                return RedirectToAction("Index", "Coupons");
            }

            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("CustomerIndex", "Users");
            }

            return RedirectToAction("Login");
        }

        [System.Web.Mvc.HttpGet]
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("Administrators"))
            {
                return RedirectToAction("Index", "Users");
            }

            if (User.Identity.IsAuthenticated && User.IsInRole("Managers"))
            {
                return RedirectToAction("Index", "Coupons");
            }

            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("CustomerIndex", "Users");
            }

            return View("Login");
        }

        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> Login(LoginBindingModel model)
        {
            if (string.IsNullOrEmpty(model.Username))
            {
                ViewBag.ErrorMessage = "Es wurde kein Benutzername übergeben!";
                return View(model);
            }

            if (string.IsNullOrEmpty(model.Password))
            {
                ViewBag.ErrorMessage = "Es wurde kein Passwort übergeben!";
                return View(model);
            }

            var user = _userService.FindbyUsername(model.Username);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"Benutzer \"{model.Username}\" wurde nicht gefunden!";
                return View(model);
            }

            if (!user.IsActive)
            {
                ViewBag.ErrorMessage = $"Benutzer \"{model.Username} \" wurde deaktiviert!";
                return View(model);
            }

            if ((user.CreatedById == null || !UserManager.IsInRole(user.CreatedById, "Administrators")) &&
                !user.EmailConfirmed)
            {
                ViewBag.EmailConfirmed = false;
                return View(model);
            }

            if (UserManager.CheckPassword(user, model.Password))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email)
                };

                var roles = await UserManager.GetRolesAsync(user.Id);

                claims.AddRange(roles.Select(x => new Claim(ClaimTypes.Role, x.ToString())));

                var id = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);

                Authentication.SignIn(id);
            }
            else
            {
                ViewBag.ErrorMessage = $"Das Passwort für den Benutzer \"{model.Username}\" ist falsch!";
                return View();
            }

            if (UserManager.IsInRole(user.Id, "Administrators"))
            {
                return RedirectToAction("Index", "Users");
            }
            else
            {
                return RedirectToAction("Index", "Coupons");
            }
        }

        [System.Web.Mvc.Authorize]
        public ActionResult LogOff()
        {
            Authentication.SignOut();
            return RedirectToAction("Index");
        }

        [System.Web.Mvc.Authorize]
        public ActionResult ResetPassword()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [System.Web.Mvc.HttpPost]
        [System.Web.Mvc.Authorize]
        public ActionResult ResetPassword(HomeResetPasswordUserModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUsername = User.Identity.GetUserName();
                var currentUser = UserManager.Find(currentUsername, model.OldPassword);
                if (currentUser == null)
                {
                    ModelState.AddModelError("OldPassword", "Das eingegebene Passwort ist falsch.");
                    return View(model);
                }

                var token = UserManager.GeneratePasswordResetToken(currentUser.Id);
                UserManager.ResetPassword(currentUser.Id, token, model.PasswordNew);

                return RedirectToAction("Index");
            }

            return View(model);
        }


        [System.Web.Mvc.AllowAnonymous]
        [System.Web.Mvc.HttpGet]
        [System.Web.Mvc.Route("ResetPasswordConfirm")]
        public ActionResult ResetPasswordConfirm(Guid userId, string code)
        {
            return View(new ResetPasswordViewModel() {UserId = userId, Code = code});
        }

        [System.Web.Mvc.AllowAnonymous]
        [System.Web.Mvc.HttpPost]
        [System.Web.Mvc.Route("ResetPasswordConfirm")]
        public async Task<ActionResult> ResetPasswordConfirm([FromBody()] ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_userService.FindById(model.UserId.ToString()) != null)
                {
                    var result = await UserManager.ResetPasswordAsync(model.UserId.ToString(), model.Code, model.Password);
                    
                    if (result.Succeeded)
                    {
                        await ResetPasswordInWebShop(model.UserId.ToString(), model.Password);
                        return RedirectToAction("ResetPasswordSuccess");
                    }
                }
                return View(model);
            }
            return View(model);
        }


        private async Task ResetPasswordInWebShop(string userId, string password) {
            var baseAddress = new Uri("https://mattersburg.schnitzelundmehr.at/");

            //  LoginApiResponseResult loginApiResponseResult = null;

            var user = await UserManager.FindByIdAsync(userId);

            using (var client = new HttpClient())
            {
                var model = new RecoveryPasswordToWebShopModel()
                {
                    Email = user.Email,
                    NewPassword = password,
                    Result=null

                };

                var content = new StringContent(JsonConvert.SerializeObject(model),
                Encoding.UTF8, "application/json");


                var response = await client.PostAsync(baseAddress + "/Customer/PasswordRecoveryFromGCloud", content);

            }
        }

        [System.Web.Mvc.HttpGet]
        [System.Web.Mvc.Route("ResetPasswordSuccess")]
        public ActionResult ResetPasswordSuccess()
        {
            return View();
        }

        [System.Web.Mvc.HttpGet]
        public ActionResult Exception()
        {
            throw new UserNotFoundException("userId");
        }

        [System.Web.Mvc.HttpGet]
        [System.Web.Mvc.Route("Home/Impressum")]
        [System.Web.Mvc.Route("Impressum")]
        public ActionResult Impressum()
        {
            return View();
        }

        [System.Web.Mvc.HttpGet]
        [System.Web.Mvc.Route("Home/AGB")]
        [System.Web.Mvc.Route("AGB")]
        public ActionResult Agb()
        {
            return View();
        }

        [System.Web.Mvc.HttpGet]
        [System.Web.Mvc.Route("Home/Datenschutzhinweise")]
        [System.Web.Mvc.Route("Datenschutzhinweise")]
        public ActionResult Datenschutzhinweise()
        {
            return View();
        }

        [System.Web.Mvc.HttpGet]
        [System.Web.Mvc.Route("Home/Marketing")]
        [System.Web.Mvc.Route("Marketing")]
        public ActionResult Marketing()
        {
            return View();
        }

        public ActionResult Install()
        {
            return View();
        }
    }
}