using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GCloud.Shared.Exceptions;
using GCloudShared.Interface;
using GCloudShared.Repository;
using GCloudShared.Service.Dto;
using GCloudShared.Shared;
using GCloudShared.WebShopDto;
using Newtonsoft.Json;
using System.Web;

namespace GCloudShared.Service
{
    public class WebShopService : IWebShopService
    {
        public async Task<object> CheckIfUserIsAlreadyRegistredInWebShop(RegisterToWebShopModel model)
        {
            try
            {
                var handler = new HttpClientHandler { CookieContainer = new CookieContainer() };
                using var client = new HttpClient(handler);
                var url = UrlConnection.CheckIfUserIsAlreadyRegistredInWebShopUrl;
                client.DefaultRequestHeaders.Add("Cookie",
                    $"SMARTSTORE.AUTH={ParametersRepository.GetWebAuthTokenFromParameterTable()}");

                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Debug.WriteLine($"[WebShopService] POST {url} body: {json}");
                var response = await client.PostAsync(url, content);
                var respBody = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[WebShopService] Status {(int)response.StatusCode}: {respBody}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<WebError>(respBody);
                    if (result != null)
                        return result;
                    else
                        return new ExceptionHandlerResult
                        {
                            Message = "Deserializacija WebError nije uspela"
                        };
                }
                else
                {
                    return new ExceptionHandlerResult
                    {
                        Message = $"HTTP {(int)response.StatusCode}: {respBody}"
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WebShopService] Greška u CheckIfUser...: {ex}");
                return new ExceptionHandlerResult { Message = ex.Message };
            }
        }


        public async Task<object> GetLastOrder()
        {
            try
            {
                var handler = new HttpClientHandler { CookieContainer = new CookieContainer() };
                using var client = new HttpClient(handler);

                var user = new UserRepository(DbBootstraper.Connection).GetCurrentUser();
                var builder = new UriBuilder(UrlConnection.GetLastOrder);
                var qs = HttpUtility.ParseQueryString(builder.Query);
                qs["email"] = user.Email;
                qs["password"] = user.Password;
                builder.Query = qs.ToString();
                var url = builder.ToString();

                Debug.WriteLine($"[WebShopService] GET {url}");
                var response = await client.GetAsync(url);
                var respBody = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[WebShopService] Status {(int)response.StatusCode}: {respBody}");

                if (response.IsSuccessStatusCode)
                {
                    // umesto OrderKuechenDisplay result = ... ?? new ExceptionHandlerResult(...)
                    var order = JsonConvert.DeserializeObject<OrderKuechenDisplay>(respBody);
                    if (order != null)
                    {
                        Debug.WriteLine("Deserializacija OrderKuechenDisplay uspela");
                        return order;
                    }
                    else
                    {
                        Debug.WriteLine("Deserializacija OrderKuechenDisplay NIJE uspela");
                        return new ExceptionHandlerResult
                        {
                            Message = "Deserializacija OrderKuechenDisplay nije uspela"
                        };
                    }
                }
                else
                {
                    Debug.WriteLine($"HTTP greška {(int)response.StatusCode}: {respBody}");
                    return new ExceptionHandlerResult
                    {
                        Message = $"HTTP {(int)response.StatusCode}: {respBody}"
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Greška u GetLastOrder: {ex}");
                return new ExceptionHandlerResult { Message = ex.Message };
            }
        }


        public async Task<object> GetOrdersInLastHour()
        {
            try
            {
                var handler = new HttpClientHandler { CookieContainer = new CookieContainer() };
                using var client = new HttpClient(handler);

                var user = new UserRepository(DbBootstraper.Connection).GetCurrentUser();
                var builder = new UriBuilder(UrlConnection.GetOrdersInLastHour);
                var qs = HttpUtility.ParseQueryString(builder.Query);
                qs["email"] = user.Email;
                qs["password"] = user.Password;
                builder.Query = qs.ToString();
                var url = builder.ToString();

                Debug.WriteLine($"[WebShopService] GET {url}");
                var response = await client.GetAsync(url);
                var respBody = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[WebShopService] Status {(int)response.StatusCode}: {respBody}");

                if (response.IsSuccessStatusCode)
                {
                    var list = JsonConvert.DeserializeObject<List<OrderKuechenDisplay>>(respBody);
                    if (list != null)
                    {
                        Debug.WriteLine($"Deserializovano {list.Count} narudžbina");
                        return list;
                    }
                    else
                    {
                        Debug.WriteLine("Deserializacija liste narudžbina nije uspela");
                        return new ExceptionHandlerResult
                        {
                            Message = "Deserializacija liste narudžbina nije uspela"
                        };
                    }
                }
                else
                {
                    Debug.WriteLine($"HTTP greška {(int)response.StatusCode}: {respBody}");
                    return new ExceptionHandlerResult
                    {
                        Message = $"HTTP {(int)response.StatusCode}: {respBody}"
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Greška u GetOrdersInLastHour: {ex}");
                return new ExceptionHandlerResult { Message = ex.Message };
            }
        }


        public async Task<object> Register(RegisterToWebShopModel model)
        {
            try
            {
                var handler = new HttpClientHandler { CookieContainer = new CookieContainer() };
                using var client = new HttpClient(handler);

                // Izvuci URL i loguj ga
                var url = UrlConnection.RegisterWebUrl;
                Debug.WriteLine($"[WebShopService] Koristim URL za registraciju: {url}");

                // Priprema tela zahteva
                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Debug.WriteLine($"[WebShopService] POST {url} body: {json}");
                var response = await client.PostAsync(url, content);
                var respBody = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[WebShopService] Status {(int)response.StatusCode}: {respBody}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<RegisterResult>(respBody);
                    if (result != null)
                    {
                        // čuvanje kolačića
                        var cookies = handler.CookieContainer.GetCookies(new Uri(url)).Cast<Cookie>();
                        foreach (var c in cookies)
                        {
                            if (c.Name == "smartstore.customer")
                                ParametersRepository.SetWebDeviceIdToParameterTable(c.Value);
                            if (c.Name == "SMARTSTORE.AUTH")
                                ParametersRepository.SetWebAuthTokenToParameterTable(c.Value);
                        }
                        Debug.WriteLine("Registracija uspešna, vraćam RegisterResult");
                        return result;
                    }
                    else
                    {
                        Debug.WriteLine("Deserializacija RegisterResult nije uspela");
                        return new ExceptionHandlerResult
                        {
                            Message = "Deserializacija RegisterResult nije uspela"
                        };
                    }
                }
                else
                {
                    Debug.WriteLine($"HTTP greška {(int)response.StatusCode}: {respBody}");
                    return new ExceptionHandlerResult
                    {
                        Message = $"HTTP {(int)response.StatusCode}: {respBody}"
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Greška u Register: {ex}");
                return new ExceptionHandlerResult { Message = ex.Message };
            }
        }



        public async Task<string> ResetPasswordInWebShopFromGcloud(RecoveryPasswordToWebShopModel model)
        {
            try
            {
                using var client = new HttpClient(new HttpClientHandler { CookieContainer = new CookieContainer() });
                var url = UrlConnection.ResetPasswordInWebShopFromGcloudUrl;
                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Debug.WriteLine($"[WebShopService] POST {url} body: {json}");
                var response = await client.PostAsync(url, content);
                var respBody = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[WebShopService] Status {(int)response.StatusCode}: {respBody}");

                return response.IsSuccessStatusCode ? respBody : throw new Exception($"HTTP {(int)response.StatusCode}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WebShopService] Greška u ResetPassword...: {ex}");
                throw;
            }
        }

        public async Task<string> SetWelcomeEmailToWebShopFromGcloud(RecoveryPasswordToWebShopModel model)
        {
            try
            {
                using var client = new HttpClient(new HttpClientHandler { CookieContainer = new CookieContainer() });
                var url = UrlConnection.SetWelcomeEmailToWebShopFromGcloudUrl;
                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Debug.WriteLine($"[WebShopService] POST {url} body: {json}");
                var response = await client.PostAsync(url, content);
                var respBody = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[WebShopService] Status {(int)response.StatusCode}: {respBody}");

                return response.IsSuccessStatusCode ? respBody : throw new Exception($"HTTP {(int)response.StatusCode}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WebShopService] Greška u SetWelcomeEmail...: {ex}");
                throw;
            }
        }
    }
}
