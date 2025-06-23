#pragma warning disable CS8632

using GCloud.Shared.Dto.Domain;
using GCloud.Shared.Exceptions;
using GCloudShared.Interface;
using GCloudShared.Service.Dto;
using GCloudPhone;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.Web;
using GCloudShared.Repository;
using GCloudShared.Shared;
using GCloudPhone.Models;

namespace GCloudShared.Service
{
    public class AuthService:IAuthService
    {  

        public async Task<object> LoginService(LoginRequestModel model)
        {
            try
            {
                CookieContainer cookies = new CookieContainer();
                HttpClientHandler handler = new HttpClientHandler();
                handler.CookieContainer = cookies;
                using (var client = new HttpClient(handler))
                {
                   
                    client.BaseAddress = new Uri(UrlConnection.LoginUrl);
                    var request = new HttpRequestMessage();
                    request.Method = HttpMethod.Post;
                    string data = JsonConvert.SerializeObject(model);
                    request.Content = new StringContent(data, Encoding.UTF8, "application/json");

                    var response = await client.SendAsync(request);
                    var content = await response.Content.ReadAsStringAsync();
                    
                    var result = JsonConvert.DeserializeObject<LoginResponseModel>(content);
                    if (result.Username == null)
                    {
                        
                        var res = JsonConvert.DeserializeObject<ExceptionHandlerResult>(content);
                        return res;
                    }
                
                    IEnumerable<Cookie> responseCookies = cookies.GetCookies(new Uri(UrlConnection.LoginUrl)).Cast<Cookie>();

                    foreach (Cookie cookie in responseCookies)
                    {
                        DateTime expires = cookie.Expires;
                        if (cookie.Name == ".AspNet.ApplicationCookie")
                        {
                            ParametersRepository.SetAuthTokenToParameterTable(cookie.Value, cookie.Expires);
                            break;
                        }
                    }
                    ParametersRepository.SetDeviceIdToParameterTable(result.MobilePhoneGuid.ToString());
                    return result;


                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<object> RegisterService(RegisterRequestModel model)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(UrlConnection.RegisterUrl);
                    var request = new HttpRequestMessage();
                    request.Method = HttpMethod.Post;
                    string data = JsonConvert.SerializeObject(model);
                    request.Content = new StringContent(data, Encoding.UTF8, "application/json");
                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        return "Sie haben sich erfolgreich registriert";
                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode ==HttpStatusCode.BadRequest)
                        {
                            var rlist = content as string;
                            var list = rlist.Split('\n');
                            return list;
                        }
                        else
                        {
                            var res = JsonConvert.DeserializeObject<ExceptionHandlerResult>(content);
                            if (res != null && res.Message != null)
                            {
                                return res;
                            }
                            return null;
                        }
                        
                       
                    }


                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool?> IsUsernameAvailable(string username)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage();
                    request.Method = HttpMethod.Get;
                    //string data = JsonConvert.SerializeObject(model);
                    request.Content = new StringContent("image/png");
                    //request.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    var response = await client.GetAsync(UrlConnection.IsUsernameAvailableUrl + username);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var res = JsonConvert.DeserializeObject<bool>(content);
                        return res;
                    }
                    return null;



                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool?> IsEmailAvailable(string email)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage();

                    var builder = new UriBuilder(UrlConnection.IsEmailAvailableUrl);

                    var query = HttpUtility.ParseQueryString(builder.Query);
                    query["email"] = email;

                    builder.Query = query.ToString();

                    string url = builder.ToString();
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var res = JsonConvert.DeserializeObject<bool>(content);
                        return res;
                    }

                    return null; ;


                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                return null;
            }
        }

        public async Task<object?> IsInvitationCodeAvailable(string invationCode)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage();

                    var builder = new UriBuilder(UrlConnection.IsInvitationCodeAvailableUrl);

                    var query = HttpUtility.ParseQueryString(builder.Query);
                    query["invitationCode"] = invationCode;

                    builder.Query = query.ToString();

                    string url = builder.ToString();
                    var response = await client.GetAsync(url);
                    if(response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var res = JsonConvert.DeserializeObject<bool>(content);
                        return res;
                    }
                    else
                    {
                        if(response.StatusCode==System.Net.HttpStatusCode.InternalServerError)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            var res = JsonConvert.DeserializeObject<ExceptionHandlerResult>(content);
                            return res;
                        }
                        return null;
                    }
                    
                   


                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<string> InvitationCodeSenderId(string invitationCode)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage();

                    var builder = new UriBuilder(UrlConnection.InvitationCodeSenderIdUrl);

                    var query = HttpUtility.ParseQueryString(builder.Query);
                    query["invitationCode"] = invitationCode;

                    builder.Query = query.ToString();

                    string url = builder.ToString();
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var res = JsonConvert.DeserializeObject<string>(content);
                        return res;
                    }
                    else
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            var res = JsonConvert.DeserializeObject<ExceptionHandlerResult>(content);
                            return res.Message;
                        }
                    }

                    return null;


                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<object> GetSpecialProductsByCompanyName(string companyName)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage();

                    var builder = new UriBuilder(UrlConnection.GetSpecialProductsByCompanyNameUrl);

                    var query = HttpUtility.ParseQueryString(builder.Query);
                    query["companyName"] = companyName;

                    builder.Query = query.ToString();

                    string url = builder.ToString();
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var res = JsonConvert.DeserializeObject<List<SpecialProductsDto>>(content);
                        return res;
                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var res = JsonConvert.DeserializeObject<ExceptionHandlerResult>(content);
                        return res;
                    }


                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<object> GetQRCode()
        {
            try
            {
                var cookieContainer = new CookieContainer();
                using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
                using (var client = new HttpClient(handler))
                {

                    var request = new HttpRequestMessage();
                    request.Method = HttpMethod.Get;
                    //string data = JsonConvert.SerializeObject(model);
                    request.Content = new StringContent("image/png");
                    //request.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    cookieContainer.Add(new Uri(UrlConnection.GetQRCodeUrl), new Cookie(".AspNet.ApplicationCookie", ParametersRepository.GetAuthTokenFromParameterTable()));
                    var response = await client.GetAsync(UrlConnection.GetQRCodeUrl);
                    var con = await response.Content.ReadAsStringAsync();
                    if (con.Contains("<!DOCTYPE html>"))
                    {
                        HttpResponseMessage hp = new HttpResponseMessage();
                        hp.StatusCode = HttpStatusCode.Forbidden;
                        return hp;
                    }
                    var content = await response.Content.ReadAsStreamAsync();
                    return content;


                }
            }
            catch (Exception ex)
            {
                var s = ex.Message;
                return null;
            }
        }

        public async Task<List<string>> GetAllUsers()
        {
            try
            {
                CookieContainer cookies = new CookieContainer();
                HttpClientHandler handler = new HttpClientHandler();
                handler.CookieContainer = cookies;

                using (var client = new HttpClient(handler))
                {
                    cookies.Add(new Uri(UrlConnection.GetAllUsers), new Cookie(".AspNet.ApplicationCookie", ParametersRepository.GetAuthTokenFromParameterTable()));
                    var response = await client.GetAsync(UrlConnection.GetAllUsers);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        var userList = JsonConvert.DeserializeObject<List<string>>(content);
                        return userList;
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}");
                        return null;
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }



        public async Task<object> ChangePassword(ChangePasswordRequestModel changePasswordRequestModel)
        {
            try
            {
                CookieContainer cookieContainer = new CookieContainer();
                HttpClientHandler handler = new HttpClientHandler();
                handler.CookieContainer = cookieContainer;
                using (var client = new HttpClient(handler))
                {

                    client.BaseAddress = new Uri(UrlConnection.ChangePasswordUrl);
                    var request = new HttpRequestMessage();
                    request.Method = HttpMethod.Post;
                    string data = JsonConvert.SerializeObject(changePasswordRequestModel);
                    request.Content = new StringContent(data, Encoding.UTF8, "application/json");
                    cookieContainer.Add(new Uri(UrlConnection.ChangePasswordUrl), new Cookie(".AspNet.ApplicationCookie", ParametersRepository.GetAuthTokenFromParameterTable()));
                    var response = await client.SendAsync(request);
                    var content = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        return "Succesful change password";
                    }
                    else
                    {
                        var res = JsonConvert.DeserializeObject<ExceptionHandlerResult>(content);
                        return res;
                    }
                 

                    


                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<HttpResponseMessage> Logout(string deviceId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage();
                    request.Method = HttpMethod.Get;
                    //string data = JsonConvert.SerializeObject(model);
                    //request.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    var response = await client.GetAsync(UrlConnection.Logout+"?deviceId=" + deviceId);
                    if (response.IsSuccessStatusCode)
                    {
                        ParametersRepository.DeletedAuthToken();
                        HttpResponseMessage hp = new HttpResponseMessage();
                        hp.StatusCode = response.StatusCode;
                        UserRepository ur = new UserRepository(DbBootstraper.Connection);
                        ur.DeleteAll();
                        return hp;
                    }
                    return null;



                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<string> GetTotalPointsByUserID(string userId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage();

                    var builder = new UriBuilder(UrlConnection.GetTotalPointsByUserIDUrl);

                    var query = HttpUtility.ParseQueryString(builder.Query);
                    query["userId"] = userId;

                    builder.Query = query.ToString();

                    string url = builder.ToString();
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return content;
                    }

                    return null; ;


                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<object> ResendActivationEmail(string username)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage();

                    var builder = new UriBuilder(UrlConnection.ResendActivationEmailUrl);

                    var query = HttpUtility.ParseQueryString(builder.Query);
                    query["username"] = username;

                    builder.Query = query.ToString();

                    string url = builder.ToString();
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var res = JsonConvert.DeserializeObject<bool>(content);
                        return res;
                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var res = JsonConvert.DeserializeObject<ExceptionHandlerResult>(content);
                        return res;
                    }



                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<object> CheckIfUserCanBuySpecialProduct(string productId, string userId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage();

                    var builder = new UriBuilder(UrlConnection.CheckIfUserCanBuySpecialProduct);

                    var query = HttpUtility.ParseQueryString(builder.Query);
                    query["productId"] = productId;
                    query["userId"] = userId;

                    builder.Query = query.ToString();

                    string url = builder.ToString();
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var res = JsonConvert.DeserializeObject<bool>(content);
                        return res;
                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var res = JsonConvert.DeserializeObject<ExceptionHandlerResult>(content);
                        return res;
                    }



                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<object> ResetPassword(string usernameOrEmail)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage();

                    var builder = new UriBuilder(UrlConnection.ResetPasswordUrl);

                    var query = HttpUtility.ParseQueryString(builder.Query);
                    query["usernameOrEmail"] = usernameOrEmail;

                    builder.Query = query.ToString();
                    StringContent stringContent = new StringContent("");
                    string url = builder.ToString();
                    var response = await client.PostAsync(url, stringContent);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var res = JsonConvert.DeserializeObject<bool>(content);
                        return res;
                    }

                    return null; ;


                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool IsLogged()
        {
            ParametersRepository pr = new ParametersRepository(DbBootstraper.Connection);
            var UsersToken = pr.FindByParameter("AuthToken");
            if (UsersToken != null)
            {
                return true;
            }
            return false;
        }

        public async Task<int> DeleteUser(string email)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var appRequest = new HttpRequestMessage();
                    var appBuilder = new UriBuilder(UrlConnection.DeleteUser);
                    var appQuery = HttpUtility.ParseQueryString(appBuilder.Query);
                    appQuery["email"] = email;
                    appBuilder.Query = appQuery.ToString();
                    string appUrl = appBuilder.ToString();
                    var appResponse = await client.GetAsync(appUrl);

                    //var webshopRequest = new HttpRequestMessage();
                    //var webshopBuilder = new UriBuilder(UrlConnection.DeleteUserWebshop);
                    //var webshopQuery = HttpUtility.ParseQueryString(webshopBuilder.Query);
                    //webshopQuery["email"] = email;
                    //webshopBuilder.Query = webshopQuery.ToString();
                    //string webshopUrl = webshopBuilder.ToString();
                    //var webshopResponse = await client.GetAsync(webshopUrl);


                    //if (appResponse.IsSuccessStatusCode && webshopResponse.IsSuccessStatusCode)
                    //{
                    //    ParametersRepository.DeletedAuthToken();
                    //    return 1;
                    //} 
                    if (appResponse.IsSuccessStatusCode)
                    {
                        ParametersRepository.DeletedAuthToken();
                        return 1;
                    }

                    return 0;


                }
            }
            catch (Exception)
            {
                return 0;
            }
        }


        public async Task<List<PushNotifications>> CheckForUserNotifications(string userId, DateTime dateTime)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new ArgumentException("UserId cannot be null or empty.");

                CookieContainer cookies = new();
                HttpClientHandler handler = new()
                {
                    CookieContainer = cookies
                };

                using (HttpClient httpClient = new HttpClient(handler))
                {
                    string authToken = ParametersRepository.GetAuthTokenFromParameterTable();
                    if (string.IsNullOrEmpty(authToken))
                        throw new InvalidOperationException("Authentication token is missing.");

                    cookies.Add(new Uri(UrlConnection.CheckNotifications), new Cookie(".AspNet.ApplicationCookie", authToken));

                    // Construct request URL with userId and lastCheckDateTime
                    string requestUrl = $"{UrlConnection.CheckNotifications}?userId={Uri.EscapeDataString(userId)}&lastCheckDateTime={dateTime:yyyy-MM-ddTHH:mm:ss}";

                    HttpResponseMessage response = await httpClient.GetAsync(requestUrl);

                    if (response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.NotFound)
                        return new List<PushNotifications>();

                    response.EnsureSuccessStatusCode();

                    string content = await response.Content.ReadAsStringAsync();

                    var notifications = JsonConvert.DeserializeObject<List<MobileNotifications>>(content);

                    return notifications.Select(n => new PushNotifications
                    {
                        NavigationID = "1",
                        ReceivedDateTime = n.TimeStamp,
                        IsRead = false, 
                        title = n.Title,
                        body = n.Body
                    }).ToList();
                }
            }
            catch (HttpRequestException ex)
            {
                //Console.WriteLine(ex, "HTTP request failed while checking notifications");
                return new List<PushNotifications>(); // Return empty list instead of throwing
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex, "Unexpected error in CheckForUserNotifications");
                return new List<PushNotifications>();
            }
        }
    }
}
