using GCloud.Shared.Dto.Domain;
using GCloud.Shared.Exceptions;
using GCloudShared.Interface;
using Newtonsoft.Json;
using System.Net;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using GCloud.Shared.Dto.Domain.CouponUsageAction;
using GCloud.Shared.Dto.Domain.CouponUsageRequirement;
using GCloudShared.Repository;

namespace GCloudShared.Service
{
    public class UserCouponService : IUserCouponService
    {
        public async Task<object> GetUserCoupons(bool skipUserValidation = false)
        {
            try
            {
                HttpResponseMessage hp = new HttpResponseMessage();
                CookieContainer cookies = new CookieContainer();
                HttpClientHandler handler = new HttpClientHandler();
                handler.CookieContainer = cookies;
                using (var client = new HttpClient(handler))
                {


                    cookies.Add(new Uri(UrlConnection.GetUserCouponsUrl+"?skipUserValidation=" + skipUserValidation), new Cookie(".AspNet.ApplicationCookie", ParametersRepository.GetAuthTokenFromParameterTable()));
                    var response = await client.GetAsync(new Uri(UrlConnection.GetUserCouponsUrl + "?skipUserValidation=" + skipUserValidation));
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        if (content.Contains("<!DOCTYPE html>"))
                        {
                            throw new SessionExpiredException("Sitzung abgelaufen. Bitte melden Sie sich erneut an.");
                            
                        }
                        else
                        {
                            var result = JsonConvert.DeserializeObject<List<CouponDto>>(content);
                            return result;
                        }



                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ExceptionHandlerResult>(content);
                        return result;
                    }


                }
            }
            catch (Exception ex)
            {
                var s=ex.Message;   
                return null;
            }
        }

        public async Task<object> GetUserCouponsByStore(string guid, bool skipUserValidation = false, bool includeImage = true)
        {
            try
            {
                HttpResponseMessage hp = new HttpResponseMessage();
                CookieContainer cookies = new CookieContainer();
                HttpClientHandler handler = new HttpClientHandler();
                handler.CookieContainer = cookies;


                cookies.Add(new Uri(UrlConnection.GetUserCouponsByStoreUrl + guid + "?skipUserValidation=" + skipUserValidation), new Cookie(".AspNet.ApplicationCookie", ParametersRepository.GetAuthTokenFromParameterTable()));

                var uri = new Uri(UrlConnection.GetUserCouponsByStoreUrl + guid + "?skipUserValidation=" + skipUserValidation);
                var cookieCollection = cookies.GetCookies(uri);

                foreach (Cookie cookie in cookieCollection)
                {
                    if (cookie.Name == ".AspNet.ApplicationCookie")
                    {
                        // Output the cookie value and expiration date
                        Console.WriteLine("Cookie value: " + cookie.Value);
                        Console.WriteLine("Expiration date: " + cookie.Expires);
      
                        break;
                    }
                }

                using (var client = new HttpClient(handler))
                {



                    cookies.Add(new Uri(UrlConnection.GetUserCouponsByStoreUrl + guid + "?skipUserValidation=" + skipUserValidation), new Cookie(".AspNet.ApplicationCookie", ParametersRepository.GetAuthTokenFromParameterTable()));
                    var response = await client.GetAsync(new Uri(UrlConnection.GetUserCouponsByStoreUrl + guid + "?skipUserValidation=" + skipUserValidation));

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Raw JSON: " + content);

                        if (content.Contains("<!DOCTYPE html>"))
                        {
                            throw new SessionExpiredException("Sitzung abgelaufen. Bitte melden Sie sich erneut an.");
                            //HttpResponseMessage HRM = new HttpResponseMessage();
                            //HRM.StatusCode = HttpStatusCode.Forbidden;
                            //return HRM;
                        }
                        else
                        {
                            JsonSerializerSettings js = new JsonSerializerSettings()
                            {
                                ContractResolver = new ShouldDeserializeContractResolver()
                            };

                            var result = JsonConvert.DeserializeObject<List<CouponDto>>(content, js);
                            return result;
                        }



                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ExceptionHandlerResult>(content);
                        return result;
                    }




                }
            }
            catch (Exception ex)
            {
                var s = ex.Message;
                return null;
            }
        }
        public async Task<object> GetCouponImage(string guid)
        {
            try
            {
                HttpResponseMessage hp = new HttpResponseMessage();
                CookieContainer cookies = new CookieContainer();
                HttpClientHandler handler = new HttpClientHandler();
                handler.CookieContainer = cookies;
                using (var client = new HttpClient(handler))
                {



                    cookies.Add(new Uri(UrlConnection.GetCouponImageUrl + guid), new Cookie(".AspNet.ApplicationCookie", ParametersRepository.GetAuthTokenFromParameterTable()));
                    var response = await client.GetAsync(new Uri(UrlConnection.GetCouponImageUrl + guid));
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        if (content.Contains("<!DOCTYPE html>"))
                        {

                            HttpResponseMessage HRM = new HttpResponseMessage();
                            HRM.StatusCode = HttpStatusCode.Forbidden;
                            return HRM;
                        }
                        else
                        {
                            var result = await response.Content.ReadAsStreamAsync();
                            return result;
                        }



                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ExceptionHandlerResult>(content);
                        return result;
                    }

                }
            }
            catch (Exception ex)
            {
                var s = ex.Message;
                return null;
            }
        }

        public async Task<object> GetCouponQrCode(string guid)
        {
            try
            {
                HttpResponseMessage hp = new HttpResponseMessage();
                CookieContainer cookies = new CookieContainer();
                HttpClientHandler handler = new HttpClientHandler();
                handler.CookieContainer = cookies;
                using (var client = new HttpClient(handler))
                {



                    cookies.Add(new Uri(UrlConnection.GetCouponQrCodeUrl + guid), new Cookie(".AspNet.ApplicationCookie", ParametersRepository.GetAuthTokenFromParameterTable()));
                    var response = await client.GetAsync(new Uri(UrlConnection.GetCouponQrCodeUrl + guid));
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        if (content.Contains("<!DOCTYPE html>"))
                        {
                            HttpResponseMessage HRM = new HttpResponseMessage();
                            HRM.StatusCode = HttpStatusCode.Forbidden;
                            return HRM;
                        }
                        else
                        {
                            var result = await response.Content.ReadAsStreamAsync();
                            return result;
                        }



                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ExceptionHandlerResult>(content);
                        return result;
                    }

                }
            }
            catch (Exception ex)
            {
                var s = ex.Message;
                return null;
            }
        }

        public async Task<object> GetManagerCoupons()
        {
            try
            {
                HttpResponseMessage hp = new HttpResponseMessage();
                CookieContainer cookies = new CookieContainer();
                HttpClientHandler handler = new HttpClientHandler();
                handler.CookieContainer = cookies;
                using (var client = new HttpClient(handler))
                {



                    cookies.Add(new Uri(UrlConnection.GetManagerCouponsUrl), new Cookie(".AspNet.ApplicationCookie", ParametersRepository.GetAuthTokenFromParameterTable()));
                    var response = await client.GetAsync(new Uri(UrlConnection.GetManagerCouponsUrl));
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        if (content.Contains("<!DOCTYPE html>"))
                        {
                            HttpResponseMessage HRM = new HttpResponseMessage();
                            HRM.StatusCode = HttpStatusCode.Forbidden;
                            return HRM;
                        }
                        else
                        {
                            JsonSerializerSettings js = new JsonSerializerSettings()
                            {
                                ContractResolver = new ShouldDeserializeContractResolver()
                            };

                            var result = JsonConvert.DeserializeObject<List<CouponDto>>(content, js);
                            return result;
                        }



                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ExceptionHandlerResult>(content);
                        return result;
                    }




                }
            }
            catch (Exception ex)
            {
                var s = ex.Message;
                return null;
            }
        }

        public async Task<object> GetUserCoupon(string guid)
        {
            try
            {
                HttpResponseMessage hp = new HttpResponseMessage();
                CookieContainer cookies = new CookieContainer();
                HttpClientHandler handler = new HttpClientHandler();
                handler.CookieContainer = cookies;
                using (var client = new HttpClient(handler))
                {


                    cookies.Add(new Uri(UrlConnection.GetUserCouponUrl + guid), new Cookie(".AspNet.ApplicationCookie", ParametersRepository.GetAuthTokenFromParameterTable()));
                    var response = await client.GetAsync(new Uri(UrlConnection.GetUserCouponUrl + guid));

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        if (content.Contains("<!DOCTYPE html>"))
                        {
                            HttpResponseMessage HRM = new HttpResponseMessage();
                            HRM.StatusCode = HttpStatusCode.Forbidden;
                            return HRM;
                        }
                        else
                        {
                            JsonSerializerSettings js = new JsonSerializerSettings()
                            {
                                ContractResolver = new ShouldDeserializeContractResolver()
                            };

                            var result = JsonConvert.DeserializeObject<CouponDto>(content, js);
                            return result;
                        }


                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ExceptionHandlerResult>(content);
                        return result;
                    }

                }
            }
            catch (Exception ex)
            {
                var s = ex.Message;
                return null;
            }
        }

    }
    public class ShouldDeserializeContractResolver : DefaultContractResolver
    {
        public static readonly ShouldDeserializeContractResolver Instance = new ShouldDeserializeContractResolver();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (property.PropertyType == typeof(List<AbstractUsageActionDto>) || property.PropertyType == typeof(List<AbstractUsageRequirementDto>))
            {
                property.ShouldDeserialize = o => false;
            }

            return property;
        }
    }
}
