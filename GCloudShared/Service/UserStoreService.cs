
using GCloud.Shared.Dto.Domain;
using GCloud.Shared.Exceptions;
using GCloudShared.Interface;
using Newtonsoft.Json;
using System.Net;
using GCloudShared.Repository;
namespace GCloudShared.Service
{
    public class UserStoreService : IUserStoreService
    {
        public async Task<object> AddToWatchList(string guid)
        {
            try
            {
                HttpResponseMessage hp = new HttpResponseMessage();
                CookieContainer cookies = new CookieContainer();
                HttpClientHandler handler = new HttpClientHandler();
                handler.CookieContainer = cookies;
                using (var client = new HttpClient(handler))
                {

                    client.BaseAddress = new Uri(UrlConnection.AddToWatchListUrl+guid);
                    var request = new HttpRequestMessage();
                    request.Method = HttpMethod.Put;
          
                    cookies.Add(new Uri(UrlConnection.AddToWatchListUrl), new Cookie(".AspNet.ApplicationCookie", ParametersRepository.GetAuthTokenFromParameterTable()));
                    var response = await client.SendAsync(request);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        if (content.Contains("<!DOCTYPE html>"))
                        {
                            hp.StatusCode = System.Net.HttpStatusCode.Forbidden;
                        }
                        else
                        {
                            hp.StatusCode = System.Net.HttpStatusCode.OK;
                        }
                        return hp;
                    }
                    else
                    {
                        
                            hp.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                            return hp;
                       
                        
                    }


                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<object> DeleteFromWatchlist(string guid)
        {
            try
            {
                HttpResponseMessage hp = new HttpResponseMessage();
                CookieContainer cookies = new CookieContainer();
                HttpClientHandler handler = new HttpClientHandler();
                handler.CookieContainer = cookies;
                using (var client = new HttpClient(handler))
                {

                    client.BaseAddress = new Uri(UrlConnection.DeleteFromWatchlist + guid);
                    var request = new HttpRequestMessage();
                    request.Method = HttpMethod.Delete;

                    cookies.Add(new Uri(UrlConnection.DeleteFromWatchlist), new Cookie(".AspNet.ApplicationCookie", ParametersRepository.GetAuthTokenFromParameterTable()));
                    var response = await client.SendAsync(request);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var content=await response.Content.ReadAsStringAsync();
                        if(content.Contains("<!DOCTYPE html>"))
                        {
                            hp.StatusCode = System.Net.HttpStatusCode.Forbidden;
                        }
                        else
                        {
                            hp.StatusCode = System.Net.HttpStatusCode.OK;
                        }
                      
                        return hp;
                    }
                    else
                    {

                        hp.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                        return hp;

                      
                    }









                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<object> GetManagerStores()
        {
            try
            {
                HttpResponseMessage hp = new HttpResponseMessage();
                CookieContainer cookies = new CookieContainer();
                HttpClientHandler handler = new HttpClientHandler();
                handler.CookieContainer = cookies;
                using (var client = new HttpClient(handler))
                {



                    cookies.Add(new Uri(UrlConnection.GetManagerStoresUrl), new Cookie(".AspNet.ApplicationCookie", ParametersRepository.GetAuthTokenFromParameterTable()));
                    var response = await client.GetAsync(new Uri(UrlConnection.GetManagerStoresUrl));
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
                            var result = JsonConvert.DeserializeObject<List<StoreDto>>(content);
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
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<object> GetUserStores()
        {
            try
            {
                HttpResponseMessage hp = new HttpResponseMessage();
                CookieContainer cookies = new CookieContainer();
                HttpClientHandler handler = new HttpClientHandler();
                handler.CookieContainer = cookies;
                using (var client = new HttpClient(handler))
                {



                    cookies.Add(new Uri(UrlConnection.GetUserStoresUrl), new Cookie(".AspNet.ApplicationCookie", ParametersRepository.GetAuthTokenFromParameterTable()));
                    var response = await client.GetAsync(new Uri(UrlConnection.GetUserStoresUrl));
                    if (response.IsSuccessStatusCode)
                    {
                        var content= await response.Content.ReadAsStringAsync();
                        if(content.Contains("<!DOCTYPE html>"))
                        {
                            HttpResponseMessage HRM = new HttpResponseMessage();
                            HRM.StatusCode = HttpStatusCode.Forbidden;
                            return HRM;
                        }
                        else
                        {
                            var result = JsonConvert.DeserializeObject<List<StoreDto>>(content);
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
            catch (Exception)
            {
                return null;
            }
        }
        
    }
}
