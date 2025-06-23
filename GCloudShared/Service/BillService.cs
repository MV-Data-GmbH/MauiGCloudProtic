using GCloudShared.Interface;
using System.Net;
using GCloudShared.Repository;

using Newtonsoft.Json;
using System.Web;
using GCloud.Shared.Dto.Domain;
using GCloud.Shared.Exceptions;
using GCloud.Shared.Dto.Api;

namespace GCloudShared.Service
{
    public class BillService:IBillService
    {
        public async Task<object> Csv(Guid? anonymousUserId)
        {
            try
            {
                HttpResponseMessage hp = new HttpResponseMessage();
                CookieContainer cookies = new CookieContainer();
                HttpClientHandler handler = new HttpClientHandler();
                handler.CookieContainer = cookies;
                using (var client = new HttpClient(handler))
                {

                    var builder = new UriBuilder(UrlConnection.CsvUrl);

                    var query = HttpUtility.ParseQueryString(builder.Query);
                    query["anonymousUserId"] = anonymousUserId.ToString();

                    builder.Query = query.ToString();
                    StringContent queryString = new StringContent("");
                    string url = builder.ToString();

                    cookies.Add(new Uri(url), new Cookie(".AspNet.ApplicationCookie", ParametersRepository.GetAuthTokenFromParameterTable()));
                    var response = await client.PostAsync(new Uri(url), queryString);
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
                            var result = JsonConvert.DeserializeObject<HttpContent>(content);
                            return result;
                        }



                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.InternalServerError)
                        {

                            if (content == "")
                            {
                                ExceptionHandlerResult ex = new ExceptionHandlerResult();
                                ex.Message = "Null reference exception";
                                return ex;

                            }


                        }
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

        public async Task<object> Get()
        {
            try
            {
                HttpResponseMessage hp = new HttpResponseMessage();
                CookieContainer cookies = new CookieContainer();
                HttpClientHandler handler = new HttpClientHandler();
                handler.CookieContainer = cookies;
                using (var client = new HttpClient(handler))
                {



                    cookies.Add(new Uri(UrlConnection.GetUrl), new Cookie(".AspNet.ApplicationCookie", ParametersRepository.GetAuthTokenFromParameterTable()));
                    var response = await client.GetAsync(new Uri(UrlConnection.GetUrl));
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
                            var result = JsonConvert.DeserializeObject<GetBillsResponseModel>(content);
                            return result;
                        }



                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.InternalServerError)
                        {

                            if (content == "")
                            {
                                ExceptionHandlerResult ex = new ExceptionHandlerResult();
                                ex.Message = "Null reference exception";
                                List<Bill_Out_Dto> listmanager= new List<Bill_Out_Dto>();
                                return new GetBillsResponseModel(listmanager);

                            }


                        }
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

        public async Task<object> GetById(Guid id)
        {
            try
            {
                HttpResponseMessage hp = new HttpResponseMessage();
                CookieContainer cookies = new CookieContainer();
                HttpClientHandler handler = new HttpClientHandler();
                handler.CookieContainer = cookies;
                using (var client = new HttpClient(handler))
                {


                    var cookie = new Cookie(".AspNet.ApplicationCookie", ParametersRepository.GetAuthTokenFromParameterTable());
                    cookie.Expires = DateTime.UtcNow.AddDays(365);
                    cookies.Add(new Uri(UrlConnection.GetByIdUrl+"?id="+id), cookie);

                    var response = await client.GetAsync(new Uri(UrlConnection.GetByIdUrl + "?id=" + id));
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
                            var result = JsonConvert.DeserializeObject<Bill_Out_Dto>(content);
                            return result;
                        }



                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.InternalServerError)
                        {

                            if (content == "")
                            {
                                ExceptionHandlerResult ex = new ExceptionHandlerResult();
                                ex.Message = "Null reference exception";
                                return ex;

                            }


                        }
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
