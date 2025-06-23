using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GCloud.Shared.Dto.Api;
using GCloud.Shared.Dto.Domain;
using GCloud.Shared.Exceptions;
using GCloudShared.Interface;
using GCloudShared.Repository;
using GCloudPhone.Helpers; // za Logger
using Newtonsoft.Json;

namespace GCloudShared.Service
{
    public class StoreService : IStoresService
    {
        public async Task<object> GetStores()
        {
            Debug.WriteLine("[StoreService] GetStores: Ulazim u metodu");
            Logger.LogInfo("[StoreService] GetStores: Ulazim u metodu");

            try
            {
                using (var client = new HttpClient())
                {
                    string url = "https://protictest1.willessen.online/api/StoresApi";
                    Debug.WriteLine($"[StoreService] GetStores: Pravim GET zahtev na URL: {url}");
                    Logger.LogInfo($"[StoreService] GetStores: Pravim GET zahtev na URL: {url}");

                    var response = await client.GetAsync(url);
                    Debug.WriteLine($"[StoreService] GetStores: Response StatusCode = {(int)response.StatusCode} {response.StatusCode}");
                    Logger.LogInfo($"[StoreService] GetStores: Response StatusCode = {(int)response.StatusCode} {response.StatusCode}");

                    string content = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[StoreService] GetStores: Response Content (JSON) = {content}");
                    Logger.LogInfo($"[StoreService] GetStores: Response Content (JSON) length = {content?.Length}");

                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        Debug.WriteLine("[StoreService] GetStores: StatusCode == BadRequest, pokušavam deserialize u ExceptionHandlerResult");
                        Logger.LogInfo("[StoreService] GetStores: StatusCode == BadRequest, deserialize u ExceptionHandlerResult");

                        var errorResult = JsonConvert.DeserializeObject<ExceptionHandlerResult>(content);
                        Debug.WriteLine("[StoreService] GetStores: Vraćam ExceptionHandlerResult");
                        Logger.LogInfo("[StoreService] GetStores: Vraćam ExceptionHandlerResult");
                        return errorResult;
                    }
                    else
                    {
                        Debug.WriteLine("[StoreService] GetStores: StatusCode != BadRequest, pokušavam deserialize u List<StoreDto>");
                        Logger.LogInfo("[StoreService] GetStores: Deserialize u List<StoreDto>");

                        var list = JsonConvert.DeserializeObject<List<StoreDto>>(content);
                        Debug.WriteLine($"[StoreService] GetStores: Deserializovano {list?.Count ?? 0} stavki");
                        Logger.LogInfo($"[StoreService] GetStores: Deserializovano {list?.Count ?? 0} stavki");

                        return list;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[StoreService] GetStores: EXCEPTION: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
                Logger.LogError("Error in GetStores: " + ex.Message);
                return null;
            }
        }
        public async Task<Stream> GetStoreImage(string guid)
        {
            Debug.WriteLine($"[StoreService] GetStoreImage: Ulazim u metodu sa guid = {guid}");
            Logger.LogInfo($"[StoreService] GetStoreImage: Ulazim sa guid = {guid}");

            try
            {
                var cookieContainer = new CookieContainer();
                using (var handler = new HttpClientHandler { CookieContainer = cookieContainer })
                using (var client = new HttpClient(handler))
                {
                    // Složi URL i ispiši ga
                    string url = UrlConnection.GetStoreImageUrl + guid;
                    Debug.WriteLine($"[StoreService] GetStoreImage: Pravim GET zahtev na URL: {url}");
                    Logger.LogInfo($"[StoreService] GetStoreImage: Pravim GET zahtev na URL: {url}");

                    
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        Content = new StringContent("image/png")
                    };

                    // Pošalji zahtev
                    var response = await client.SendAsync(request);
                    Debug.WriteLine($"[StoreService] GetStoreImage: Response StatusCode = {(int)response.StatusCode} {response.StatusCode}");
                    Logger.LogInfo($"[StoreService] GetStoreImage: Response StatusCode = {(int)response.StatusCode} {response.StatusCode}");

                    if (!response.IsSuccessStatusCode)
                    {
                        Debug.WriteLine($"[StoreService] GetStoreImage: Neuspešan status kod, vraćam null");
                        Logger.LogError($"[StoreService] GetStoreImage: Neuspešan status kod: {(int)response.StatusCode} {response.StatusCode}");
                        return null;
                    }

                    // Čitaj stream
                    var contentStream = await response.Content.ReadAsStreamAsync();
                    Debug.WriteLine($"[StoreService] GetStoreImage: Uspesno preuzet Stream");
                    Logger.LogInfo($"[StoreService] GetStoreImage: Uspesno preuzet Stream");

                    return contentStream;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[StoreService] GetStoreImage: EXCEPTION: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
                Logger.LogError("Error in GetStoreImage: " + ex.Message);
                return null;
            }
        }

        

        public async Task<HttpResponseMessage> UpdateStore(StoreManagerEditModel storeManagerEditModel)
        {
            Debug.WriteLine($"[StoreService] UpdateStore: Ulazim u metodu sa storeManagerEditModel.Id = {storeManagerEditModel?.Id}");
            Logger.LogInfo($"[StoreService] UpdateStore: Ulazim sa storeManagerEditModel.Id = {storeManagerEditModel?.Id}");

            try
            {
                CookieContainer cookies = new CookieContainer();
                using (var handler = new HttpClientHandler { CookieContainer = cookies })
                using (var client = new HttpClient(handler))
                {
                    string baseUrl = UrlConnection.UpdateStoreUrl;
                    client.BaseAddress = new Uri(baseUrl);
                    Debug.WriteLine($"[StoreService] UpdateStore: BaseAddress postavljen na {baseUrl}");
                    Logger.LogInfo($"[StoreService] UpdateStore: BaseAddress postavljen na {baseUrl}");

                    // Serialize model u JSON
                    string jsonPayload = JsonConvert.SerializeObject(storeManagerEditModel);
                    Debug.WriteLine($"[StoreService] UpdateStore: Request JSON = {jsonPayload}");
                    Logger.LogInfo($"[StoreService] UpdateStore: Request JSON length = {jsonPayload?.Length}");

                    // Dodaj cookie za autentikaciju
                    string authToken = ParametersRepository.GetAuthTokenFromParameterTable();
                    cookies.Add(new Uri(baseUrl), new Cookie(".AspNet.ApplicationCookie", authToken));
                    Debug.WriteLine($"[StoreService] UpdateStore: Dodat auth cookie (duzina tokena = {authToken?.Length})");
                    Logger.LogInfo($"[StoreService] UpdateStore: Dodat auth cookie (duzina tokena = {authToken?.Length})");

                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
                    };

                    Debug.WriteLine($"[StoreService] UpdateStore: Šaljem POST na {baseUrl}");
                    Logger.LogInfo($"[StoreService] UpdateStore: Šaljem POST na {baseUrl}");

                    var response = await client.SendAsync(request);
                    Debug.WriteLine($"[StoreService] UpdateStore: Response StatusCode = {(int)response.StatusCode} {response.StatusCode}");
                    Logger.LogInfo($"[StoreService] UpdateStore: Response StatusCode = {(int)response.StatusCode} {response.StatusCode}");

                    if (response.StatusCode == HttpStatusCode.NoContent)
                    {
                        Debug.WriteLine("[StoreService] UpdateStore: StatusCode == NoContent, vraćam HttpResponseMessage sa NoContent");
                        Logger.LogInfo("[StoreService] UpdateStore: StatusCode == NoContent");
                        var hp = new HttpResponseMessage(HttpStatusCode.NoContent);
                        return hp;
                    }
                    else if (response.StatusCode == HttpStatusCode.OK)
                    {
                        Debug.WriteLine("[StoreService] UpdateStore: StatusCode == OK, vraćam HttpResponseMessage sa Forbidden");
                        Logger.LogInfo("[StoreService] UpdateStore: StatusCode == OK -> Forbidden");
                        var hp = new HttpResponseMessage(HttpStatusCode.Forbidden);
                        return hp;
                    }
                    else
                    {
                        Debug.WriteLine($"[StoreService] UpdateStore: Neočekivani status kod {(int)response.StatusCode} {response.StatusCode}, vraćam null");
                        Logger.LogError($"[StoreService] UpdateStore: Neočekivani status kod {(int)response.StatusCode} {response.StatusCode}");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[StoreService] UpdateStore: EXCEPTION: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
                Logger.LogError("Error in UpdateStore: " + ex.Message);
                return null;
            }
        }
    }
}
