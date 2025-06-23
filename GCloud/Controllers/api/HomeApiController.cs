using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.Http.Results;
using System.Web.Mvc;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GCloud.App_Start;
using GCloud.Controllers.ViewModels.Home;
using GCloud.Extensions;
using GCloud.Helper;
using GCloud.Models.Domain;
using GCloud.Service;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using GCloud.Shared.Dto.Api;
using GCloud.Shared.Dto.Domain;
using GCloud.Shared.Exceptions.Home;
using GCloud.Shared.Exceptions.User;
using Microsoft.Ajax.Utilities;
using WebGrease.Css.Extensions;
using ModelStateDictionary = System.Web.Http.ModelBinding.ModelStateDictionary;
using GCloud.Service.Impl;
using System.Configuration;
using System.Data.SqlClient;

namespace GCloud.Controllers.api
{
    [System.Web.Http.RoutePrefix("api/HomeApi")]
    public class HomeApiController : ApiController
    {
        private readonly IMobilePhoneService _mobilePhoneService;
        private readonly IUserService _userService;
        private readonly IStoreService _storesService;
        private readonly ICouponService _couponService;
        private readonly ISpecialProductService _specialProductService;
        private readonly IEmailService _emailService;
        private ApplicationUserManager _userManager;
        private readonly ICompanyService _companyService;

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        public HomeApiController(IMobilePhoneService mobilePhoneService, IUserService userService,
            ICouponService couponService, IStoreService storeService, IEmailService emailService, ICompanyService companyService, ISpecialProductService specialProductService)
        {
            _mobilePhoneService = mobilePhoneService;
            _userService = userService;
            _storesService = storeService;
            _emailService = emailService;
            _companyService = companyService;
            _couponService = couponService;
            _specialProductService = specialProductService;

        }

        private IAuthenticationManager Authentication => Request.GetOwinContext().Authentication;

        [System.Web.Http.HttpPost]
        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.Route("Login")]
        public async Task<LoginResponseModel> Login(LoginBindingModel model)
        {
            var user = _userService.FindbyUsername(model.Username) ?? _userService.FindBy(x => x.Email == model.Username).FirstOrDefault();

            if (user == null)
            {
                throw new CredentialsWrongException();
            }

            if (!user.IsActive)
            {
                throw new UserDisabledException(user.Id);
            }

            if ((user.CreatedById == null || !UserManager.IsInRole(user.CreatedById, "Administrators")) &&
                !user.EmailConfirmed)
            {
                throw new EmailNotConfirmedException(user.Id);
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
                // Set authentication cookie options
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true, // Make the cookie persistent
                    ExpiresUtc = DateTimeOffset.UtcNow.AddYears(1) // Extend by 1 year
                };

                Authentication.SignIn(authProperties, id);
                //Authentication.SignIn(id);
            }
            else
            {
                throw new CredentialsWrongException();
            }

            var userRoles = UserManager.GetRolesAsync(user.Id);

            MobilePhone usersMobilePhone = null;

            if (!string.IsNullOrWhiteSpace(model.FirebaseInstanceId))
            {
                usersMobilePhone = _mobilePhoneService.FindBy(m => m.FirebaseInstanceId == model.FirebaseInstanceId).FirstOrDefault();
            }

            if (usersMobilePhone == null)
            {
                if (model.DeviceId.HasValue)
                {
                    usersMobilePhone = _mobilePhoneService.FindById(model.DeviceId.Value) ?? _mobilePhoneService.CreateNewDevice(user.Id, model.FirebaseInstanceId);
                }
                else
                {
                    usersMobilePhone = _mobilePhoneService.CreateNewDevice(user.Id, model.FirebaseInstanceId);
                }
            }
            else
            {
                usersMobilePhone.UserId = user.Id;
                _mobilePhoneService.Update(usersMobilePhone);
            }

            return new LoginResponseModel
            {
                UserId = user.Id,
                Username = user.UserName,
                Role = (await userRoles).FirstOrDefault(),
                MobilePhoneGuid = usersMobilePhone.Id,
                Email = user.Email,
                InvitationCode = user.InvitationCode,
                AuthToken = HttpContext.Current.Response.Cookies[".AspNet.ApplicationCookie"]?.Value
            };
        }

        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("Register")]
        public async Task<IHttpActionResult> Register(RegisterBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                BadRequest(ModelState);
            }

            if (_userService.FindbyUsername(model.Username) != null)
            {
                throw new UsernameAlreadyTakenException(model.Username);
            }


            var user = new User()
            {
                UserName = model.Username,
                Email = model.Email,
                IsActive = true,
                Birthday = model.Birthday,
                FirstName = model.FirstName,
                LastName = model.LastName,
                InvitationCode = model.InvitationCode,
                InvitationCodeSender = model.InvitationCodeSender,
                EmailConfirmed = true,
                //TotalPoints = model.InvitationCodeSender == null ? model.InvitationCodeSender : "1"
                TotalPoints = _companyService.GetRegistrationPoints().ToString()
            };

            IdentityResult result = await UserManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                throw new RegistrationException();
            }

            result = await UserManager.AddToRoleAsync(user.Id, "Customers");

            await UsersController.SendMail(UserManager, UsersController.GetBaseUrl(HttpContext.Current), user);

            if (!result.Succeeded)
            {
                throw new RegistrationException();
            }

            return Ok();
        }


        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("ResetPassword")]
        public async Task<bool> ResetPassword(string usernameOrEmail)
        {
            //User user = _emailService.IsEmailValid(usernameOrEmail)
            //    ? _userService.FindBy(x => x.Email == usernameOrEmail).FirstOrDefault()
            //    : _userService.FindbyUsername(usernameOrEmail);
            //if (user == null) return false;

            //await _emailService.SendForgotPasswordEmail(user.GetId(), UserManager);

            //return true;


            // Check if the input is a valid email address
            bool isEmail = _emailService.IsEmailValid(usernameOrEmail);

            bool userFound = false;
            string userId = null;

            string connectionString = ConfigurationManager.ConnectionStrings["GCloudContext"].ConnectionString;

            string query = isEmail ?
                "SELECT Id FROM AspNetUsers WHERE Email = @UsernameOrEmail" :
                "SELECT Id FROM AspNetUsers WHERE UserName = @UsernameOrEmail";

            //"SELECT Id FROM AspNetUsers WHERE Email = @UsernameOrEmail or UserName = @UsernameOrEmail";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(); // Open connection
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UsernameOrEmail", usernameOrEmail);

                    // Execute the command to retrieve user ID
                    object result = await command.ExecuteScalarAsync();

                    if (result != null && result != DBNull.Value)
                    {
                        userFound = true;
                        userId = result.ToString();
                    }
                }
            }

            if (!userFound)
            {
                return false; // User not found, return false
            }

            //konvertuje string u Guid
            Guid userIdGuid;
            if (!Guid.TryParse(userId, out userIdGuid))
            {
                return false;
            }

            // Call email service to send forgot password email
            await _emailService.SendForgotPasswordEmail(userIdGuid, UserManager);

            return true;
        }

        [System.Web.Http.HttpGet]
        public void LogOff(Guid? deviceId)
        {
            Authentication.SignOut();
            //if (deviceId != null)
            //{
            //    _mobilePhoneService.RemoveDevice(userId, deviceId.Value);
            //}
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.AllowAnonymous]
        public async Task<bool> ResendActivationEmail(string username, string email = null, string password = null)
        {
            var user = _userService.FindbyUsername(username);
            var userFromUserManager = UserManager.FindById(user.Id);
            if (!user.EmailConfirmed)
            {
                if (email != null && _emailService.IsEmailValid(email) && IsEmailAvailable(email))
                {
                    if (!UserManager.CheckPassword(user, password))
                    {
                        throw new CredentialsWrongException();
                    }
                    userFromUserManager.Email = email;
                    UserManager.Update(userFromUserManager);
                }
                await UsersController.SendMail(UserManager, UsersController.GetBaseUrl(HttpContext.Current), userFromUserManager);
                return true;
            }

            return false;
        }

        [System.Web.Http.Route("Available/{username}")]
        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpGet]
        public bool IsUserNameAvailable(string username)
        {
            if (username.IsNullOrWhiteSpace())
            {
                return false;
            }

            var user = UserManager.FindByName(username.Trim());
            return user == null;
        }

        [System.Web.Http.Route("IsEmailAvailable")]
        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpGet]
        public bool IsEmailAvailable(string email)
        {
            //if (email.IsNullOrWhiteSpace())
            //{
            //    return false;
            //}

            //var user = UserManager.FindByEmail(email.Trim());
            //return user == null; //ako je user null return true, email je dostupan

            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            bool isAvailable = false;

            string connectionString = ConfigurationManager.ConnectionStrings["GCloudContext"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM AspNetUsers WHERE Email = @Email";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email.Trim());

                    int count = (int)command.ExecuteScalar();
                    isAvailable = count == 0;
                }
            }

            return isAvailable;
        }

        protected override InvalidModelStateResult BadRequest(ModelStateDictionary modelState)
        {
            var responseMessage = string.Join("\n",
                modelState.Values.SelectMany(m => m.Errors).Select(e => e.ErrorMessage).ToList());
            throw new HttpResponseException(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(responseMessage, Encoding.UTF8)
            });
        }

        [System.Web.Http.Route("IsInvitationCodeAvailable")]
        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpGet]
        public bool IsInvitationCodeAvailable(string invitationCode)
        {
            //if (invitationCode.IsNullOrWhiteSpace())
            //{
            //    return false;
            //}

            //var user = _userService.FindBy(x => x.InvitationCode == invitationCode).FirstOrDefault();
            //return user == null; //ako je user null,true , invitation code ne postoji vec u bazi i dostupan je za novog korisnika

            if (invitationCode.IsNullOrWhiteSpace())
            {
                return false;
            }

            string connectionString = ConfigurationManager.ConnectionStrings["GCloudContext"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM AspNetUsers WHERE InvitationCode = @InvitationCode";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@InvitationCode", invitationCode);

                    int count = (int)command.ExecuteScalar();

                    // If count is greater than 0, it means the invitation code exists in the database.
                    // Otherwise, it doesn't exist.
                    return count == 0;
                }
            }
        }

        [System.Web.Http.Route("GetAllUsers")]
        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpGet]
        public List<string> GetAllUsersRepository()
        {
            //var result = _mobilePhoneService.GetMobilePhones().ToList();
            //return result;
            string ConnectionString = ConfigurationManager.ConnectionStrings["GCloudContext"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                List<string> result = new List<string>();
                using (SqlCommand cmd = new SqlCommand("Select FirebaseInstanceId from MobilePhones", connection))
                {
                    connection.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        //if reader.HasRows
                        while (reader.Read())
                        {
                            result.Add(Convert.ToString(reader["FirebaseInstanceId"]));
                        }
                        return result;
                    } 
                }
            }
        }

        [System.Web.Http.Route("GetUserDeviceIds")]
        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpGet]
        public List<string> GetUserDeviceIds(string userEmail)
        {
            string ConnectionString = ConfigurationManager.ConnectionStrings["GCloudContext"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                List<string> result = new List<string>();
                using (SqlCommand cmd = new SqlCommand("SELECT FirebaseInstanceId FROM MobilePhones WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE Email = @UserEmail)", connection))
                {
                    cmd.Parameters.AddWithValue("@UserEmail", userEmail);
                    connection.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(Convert.ToString(reader["FirebaseInstanceId"]));
                        }
                        return result;
                    }
                }
            }
        }

        [System.Web.Http.Route("GetTotalPointsByUserID")]
        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpGet]
        public string GetTotalPointsByUserID(string userId)
        {
            //if (userId.IsNullOrWhiteSpace())
            //{
            //    return "invalid result";
            //}

            //var user = _userService.FindBy(x => x.Id == userId).FirstOrDefault();
            //return user.TotalPoints;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return "invalid result";
            }

            string totalPoints = "0"; // Default value if user is not found or total points is null

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["GCloudContext"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT TotalPoints FROM AspNetUsers WHERE Id = @UserId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);

                        object result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            totalPoints = result.ToString();
                        }
                    }
                }
            }
            catch (Exception)
            {
                return "invalid result";
            }

            return totalPoints;
        }

        [System.Web.Http.Route("InvitationCodeSenderId")]
        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpGet]
        public string InvitationCodeSenderId(string invitationCode)
        {
            //if (invitationCode.IsNullOrWhiteSpace())
            //{
            //    return null;
            //}

            //var user = _userService.FindBy(x => x.InvitationCode == invitationCode).FirstOrDefault();
            //return user.Id;

            if (string.IsNullOrWhiteSpace(invitationCode))
            {
                return null;
            }

            string userId = null;

            string connectionString = ConfigurationManager.ConnectionStrings["GCloudContext"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT Id FROM AspNetUsers WHERE InvitationCode = @InvitationCode";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@InvitationCode", invitationCode);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            userId = reader["Id"].ToString();
                        }
                    }
                }
            }

            return userId;
        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.Authorize]
        public HttpResponseMessage ChangePassword(ChangePasswordBindingModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = _userService.FindById(User.Identity.GetUserId());

                var isOldPasswordValid = UserManager.CheckPassword(currentUser, model.OldPassword);

                if (isOldPasswordValid)
                {
                    var token = UserManager.GeneratePasswordResetToken(currentUser.Id);
                    UserManager.ResetPassword(currentUser.Id, token, model.NewPassword);
                    return Request.CreateResponse<object>(HttpStatusCode.OK, null);
                }

                throw new OldPasswordInvalidException();
            }

            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

           
        }




        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("LoadInitialData")]
        public LoadInitialDataResponseModel LoadInitialData(bool includeImage = false, bool includeBanner = false,
            bool includeCompanyLogo = false)
        {
            var userId = User?.Identity?.GetUserId();
            var stores = _storesService.FindAll().Include(s => s.Coupons).ToList().Select(x => Mapper.Map<StoreDto>(x,
                opts =>
                {
                    opts.Items.Add(AutomapperConfig.UserId, userId);
                    opts.Items.Add(AutomapperConfig.IncludeBanner, includeBanner);
                    opts.Items.Add(AutomapperConfig.IncludeCompanyLogo, includeCompanyLogo);
                })).ToList();
            var coupons = _storesService.FindAll().SelectMany(s => s.Coupons).ToList().DistinctBy(c => c.Id).Select(s =>
                Mapper.Map<CouponDto>(s, opts =>
                {
                    opts.Items.Add(AutomapperConfig.IncludeImage, includeImage);
                    opts.Items.Add(AutomapperConfig.UserId, userId);
                })).ToList();
            //var coupons = stores.SelectMany(s => s.Coupons).ToList();

            var responseModel = new LoadInitialDataResponseModel
            {
                Stores = stores,
                Coupons = coupons
            };

            return responseModel;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("GetBackGroundImages")]
        public IList<ImageViewModel> GetBackGroundImages(string alreadyDownloaded)
        {
            var result = new List<ImageViewModel>();
            DirectoryInfo
                d = new DirectoryInfo(
                    HostingEnvironment.MapPath("~/UploadedFiles/DashboardBackgrounds/")); //Assuming Test is your Folder
            FileInfo[] files = d.GetFiles(); //Getting Text files

            if (alreadyDownloaded != null)
            {
                result = alreadyDownloaded.Split(',').Select(x => new ImageViewModel()
                {
                    Name = x,
                    StateEnum = ImageViewModelState.Deleted
                }).ToList();
            }

            foreach (FileInfo file in files)
            {
                var item = result.FirstOrDefault(x => x.Name == file.Name);
                if (item != null)
                {
                    item.StateEnum = ImageViewModelState.UpToDate;
                    continue;
                }

                using (Image image = Image.FromFile(file.FullName))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        image.Save(m, image.RawFormat);
                        byte[] imageBytes = m.ToArray();

                        result.Add(new ImageViewModel
                        {
                            Name = file.Name,
                            Image = imageBytes,
                            StateEnum = ImageViewModelState.New
                        });
                    }
                }
            }

            return result;
        }

        [System.Web.Http.Authorize]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("AssignAnonymousUserToCurrentUser")]
        public void AssignAnonymousUserToCurrentUser(Guid anonymousUserId)
        {
            _userService.AssignAnonymousUserToUser(anonymousUserId, User.Identity.GetUserId());
        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetUserEmail")]
        public string GetUserEmail(string username, string password)
        {
            var user = _userService.FindbyUsername(username);

            if (user == null)
            {
                throw new CredentialsWrongException();
            }

            if (!user.IsActive)
            {
                throw new UserDisabledException(user.Id);
            }

            if (UserManager.CheckPassword(user, password))
            {
                return user.Email;
            }
            throw new CredentialsWrongException();
        }


        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("InsertPoints")]
        public void InsertPoints(PointsFromKassa totalPoints)
        {
            //User user = _emailService.IsEmailValid(usernameOrEmail)
            //    ? _userService.FindBy(x => x.Email == usernameOrEmail).FirstOrDefault()
            //    : _userService.FindbyUsername(usernameOrEmail);
            //if (user == null) return false;

            //await _emailService.SendForgotPasswordEmail(user.GetId(), UserManager);

            //return true;

            GCloudContext context = new GCloudContext();
            context.PointsFromKassas.Add(totalPoints);
            context.SaveChanges();


        }

        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("DeleteUser")]
        public void DeleteUser(string email)
        {
            //GCloudContext context = new GCloudContext();
            //var user = UserManager.FindByEmail(email.Trim());

            //var id = user.Id;

            //var user2 = context.Users.Find(id);
            //context.Users.Remove(user2);
            //context.SaveChanges();

            //var user = _userService.FindbyEmail(email);
            //user.IsActive = false;
            //_userService.Update(user);


            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentNullException(nameof(email));
            }

            string connectionString = ConfigurationManager.ConnectionStrings["GCloudContext"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "UPDATE AspNetUsers SET IsActive = @IsActive WHERE Email = @Email";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IsActive", false);
                    command.Parameters.AddWithValue("@Email", email);

                    command.ExecuteNonQuery();
                }
            }
        }

        //Api za citanje TotalPoints i PointsPerEuro!
        //[System.Web.Http.AllowAnonymous]
        //[System.Web.Http.HttpGet]
        //[System.Web.Http.Route("GetPoints")]
        //public List<int> GetPoints(string userId, string storeApiToken)
        //{
        //    var user = _userService.FindBy(x => x.Id == userId).FirstOrDefault();
        //    int points = Convert.ToInt32(user.TotalPoints);

        //    var store = _storesService.FindBy(x => x.ApiToken == storeApiToken).FirstOrDefault();
        //    int pointsPerEuro = store.PointsPerEuro;

        //    List<int> list = new List<int>();
        //    list.Add(points);
        //    list.Add(pointsPerEuro);
        //    return list;


        //    ////GCloudContext context = new GCloudContext();
        //    ////context.PointsFromKassas.Add(totalPoints);
        //    ////context.SaveChanges();


        //}

        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("GetPoints")]
        public PointsDto GetPoints(string userId, string storeApiToken)
        {
            var user = _userService.FindBy(x => x.Id == userId).FirstOrDefault();
            string points = user.TotalPoints;

            var store = _storesService.FindBy(x => x.ApiToken == storeApiToken).FirstOrDefault();
            string pointsPerEuro = store.PointsPerEuro.ToString();

            string euroPerPoints = store.EuroPerPoints.ToString();

            PointsDto pointsAndPointsPerEuro = new PointsDto();
            pointsAndPointsPerEuro.totalPoints = points;
            pointsAndPointsPerEuro.pointsPerEuro = pointsPerEuro;
            pointsAndPointsPerEuro.euroPerPoints = euroPerPoints;

            //List<int> list = new List<int>();
            //list.Add(points);
            //list.Add(pointsPerEuro);
            return pointsAndPointsPerEuro;


            ////GCloudContext context = new GCloudContext();
            ////context.PointsFromKassas.Add(totalPoints);
            ////context.SaveChanges();


        }


        //[System.Web.Http.Authorize]
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("GetSpecialProductsByCompanyName")]
        public List<SpecialProductsDto> GetSpecialProductsByCompanyName(string companyName)
        {
            var managerId = _companyService.GetManagerIdByCompanyName(companyName);


            var listOfSpecialProducts = Mapper.Map<List<SpecialProductsDto>>(_specialProductService.FindByUserId(managerId));


            return listOfSpecialProducts;

        }

        //[System.Web.Http.Authorize]
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("CheckIfUserCanBuySpecialProduct")]
        public IHttpActionResult CheckIfUserCanBuySpecialProduct(string productId, string userId)
        {
            //SpecialProductAvaliability specialProductAvailability = new SpecialProductAvaliability();
            //specialProductAvailability.IsAvailable = false;

            //if (productId != null && userId != null)
            //{
            //    //decimal valueOfSpecialProduct = _specialProductService.GetSpecialProductValue(productId);

            //    decimal valueOfSpecialProduct = _couponService.GetSpecialProductValue(productId);
            //    var user = _userService.FindBy(x => x.Id == userId).FirstOrDefault();
            //    var valueOfUserPoints = Convert.ToDecimal(user.TotalPoints);

            //    if (valueOfUserPoints < valueOfSpecialProduct)
            //    {
            //        var points = (valueOfUserPoints - valueOfSpecialProduct).ToString();
            //        return Ok(points);
            //    }
            //    else
            //    {
            //        //dovoljan broj bodova usera za oduzimanje
            //        var points = valueOfSpecialProduct.ToString();
            //        specialProductAvailability.IsAvailable = true;
            //        return Ok(points);
            //    }

            //}
            //else
            //{
            //    //specialProductAvailability.IsAvailable = false;
            //    //invalid input
            //    return Content(HttpStatusCode.BadRequest, "User or product doesn't exist!");
            //}

            SpecialProductAvaliability specialProductAvailability = new SpecialProductAvaliability();
            specialProductAvailability.IsAvailable = false;

            AvailablePoints availablePoints = new AvailablePoints();

            if (productId != null && userId != null)
            {
                //decimal valueOfSpecialProduct = _specialProductService.GetSpecialProductValue(productId);

                decimal valueOfSpecialProduct = _couponService.GetSpecialProductValue(productId);
                var user = _userService.FindBy(x => x.Id == userId).FirstOrDefault();
                var valueOfUserPoints = Convert.ToDecimal(user.TotalPoints);

                if (valueOfUserPoints < valueOfSpecialProduct)
                {
                    availablePoints.Points = Convert.ToInt32(valueOfUserPoints - valueOfSpecialProduct);
                    return Ok(availablePoints);
                }
                else
                {
                    //dovoljan broj bodova usera za oduzimanje
                    availablePoints.Points = Convert.ToInt32(valueOfSpecialProduct);
                    specialProductAvailability.IsAvailable = true;
                    return Ok(availablePoints);
                }

            }
            else
            {
                //specialProductAvailability.IsAvailable = false;
                //invalid input
                return Content(HttpStatusCode.BadRequest, "User or product doesn't exist!");
            }
        }

        //[System.Web.Http.Authorize]
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("SubstractSpecialProductValue")]
        public IHttpActionResult SubstractSpecialProductValue(string productId, string userId)
        {
            if (productId != null && userId != null)
            {
                //decimal valueOfSpecialProduct = _specialProductService.GetSpecialProductValue(productId);
                decimal valueOfSpecialProduct = _couponService.GetSpecialProductValue(productId);
                var user = _userService.FindBy(x => x.Id == userId).FirstOrDefault();
                var valueOfUserPoints = Convert.ToDecimal(user.TotalPoints);

                if (valueOfUserPoints < valueOfSpecialProduct)
                {
                    //sifra za nedovoljan broj bodova usera
                    return Content(HttpStatusCode.NotFound, "User doesn't have enough points!");
                }
                else
                {
                    var totalPointsNew = (Convert.ToInt32(valueOfUserPoints - valueOfSpecialProduct)).ToString();
                    user.TotalPoints = totalPointsNew;
                    _userService.Update(user);
                    var userPointsDto = Mapper.Map<UserPointsDto>(_userService.FindBy(x => x.Id == userId).FirstOrDefault());
                    return Ok(userPointsDto);
                }
            }
            else
            {
                //invalid input
                return Content(HttpStatusCode.BadRequest, "User or product doesn't exist!");
            }
        }


        //Naredne tri metode su dodate za potrebe nove funkcionalnosti prenosa broja bodova iz mySchnitzel Punktepass 
        //aplikacije u aplikaciju mySchnitzel. Isti user ima razlicite id-jeve u ove dve aplikacije
        //GCLoudContextOld je potrebno dodati na serveru u web.config fajl
        //[System.Web.Http.Authorize]
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("ReturnTotalPointsFromOldApp")]
        public IHttpActionResult ReturnTotalPointsFromOldApp(string oldUserId)
        {

            string ConnectionString = ConfigurationManager.ConnectionStrings["GCloudContextOld"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                string Command = "Select TotalPoints from AspNetUsers where Id= @UserId";
                SqlCommand cmd = new SqlCommand(Command, connection);
                cmd.Parameters.AddWithValue("@UserId", oldUserId);
                connection.Open();

                string TotalPoints = (string)cmd.ExecuteScalar();
                return Ok(TotalPoints);
            }
        }



        //[System.Web.Http.Authorize]
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("ReturnTotalPointsFromNewApp")]
        public IHttpActionResult ReturnTotalPointsFromNewApp(string newUserId)
        {

            string ConnectionString = ConfigurationManager.ConnectionStrings["GCloudContext"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {

                string Command = "Select TotalPoints from AspNetUsers where Id= @UserId";
                SqlCommand cmd = new SqlCommand(Command, connection);
                cmd.Parameters.AddWithValue("@UserId", newUserId);
                connection.Open();

                string TotalPoints = (string)cmd.ExecuteScalar();
                return Ok(TotalPoints);
            }

        }



        //public void Put(int id, [FromBody] Employee employee)
        //{
        //    using (EmployeeDBEntities entities = new EmployeeDBEntities())
        //    {
        //        var entity = entities.Employees.FirstOrDefault(e => e.ID == id);

        //        entity.FirstName = employee.FirstName;
        //        entity.LastName = employee.LastName;
        //        entity.Gender = employee.Gender;
        //        entity.Salary = employee.Salary;

        //        entities.SaveChanges();
        //    }
        //}


        //http://localhost/GCloud/api/HomeApi/UpdateTotalPointsFromOldApp?oldUserId=147d462f-1b80-4261-bb5e-16b4f181d843&newUserId=1fb31906-25c2-41eb-992f-4a33f0b47aa1
        //[System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("UpdateTotalPointsFromOldApp")]
        public IHttpActionResult UpdateTotalPointsFromOldApp(string oldUserId, string newUserId)
        {
            string OldTotalPoints = string.Empty;
            string ConnectionString = ConfigurationManager.ConnectionStrings["GCloudContextOld"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                string Command = "Select TotalPoints from AspNetUsers where Id= @UserId";
                SqlCommand cmd = new SqlCommand(Command, connection);
                cmd.Parameters.AddWithValue("@UserId", oldUserId);
                connection.Open();

                //TotalPoints that user has in mySchnitzelPunktePass app
                OldTotalPoints = (string)cmd.ExecuteScalar();
            }

            var user = _userService.FindBy(x => x.Id == newUserId).FirstOrDefault();

            if (user == null)
            {
                //return NotFound();
                return Content(HttpStatusCode.NotFound, "User not found");
            }

            var c = Convert.ToInt32(user.TotalPoints);
            var d = c + Convert.ToInt32(OldTotalPoints);
            user.TotalPoints = d.ToString();
            _userService.Update(user);

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                string Command = "update AspNetUsers set TotalPoints= 0 where Id= @UserId";
                SqlCommand cmd = new SqlCommand(Command, connection);
                cmd.Parameters.AddWithValue("@UserId", oldUserId);
                connection.Open();

                cmd.ExecuteNonQuery();
            }

            return Ok();
        }

        //[System.Web.Http.AllowAnonymous]
        //[System.Web.Http.HttpPost]
        //[System.Web.Http.Route("InsertPoints")]
        //public void InsertPoints(PointsFromKassa totalPoints)
        //{
        //    //User user = _emailService.IsEmailValid(usernameOrEmail)
        //    //    ? _userService.FindBy(x => x.Email == usernameOrEmail).FirstOrDefault()
        //    //    : _userService.FindbyUsername(usernameOrEmail);
        //    //if (user == null) return false;

        //    //await _emailService.SendForgotPasswordEmail(user.GetId(), UserManager);

        //    //return true;

        //    GCloudContext context = new GCloudContext();
        //    context.PointsFromKassas.Add(totalPoints);
        //    context.SaveChanges();


        //}



    }
}