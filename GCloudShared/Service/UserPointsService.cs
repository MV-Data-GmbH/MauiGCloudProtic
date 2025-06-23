using GCloud.Shared.Exceptions;
using GCloudShared.Repository;
using Newtonsoft.Json;
using System.Net;


namespace GCloudShared.Service
{
    public class UserPointsService
    {
        public async Task<object> UpdateUserPointsAfterPurchase(int amount, string storeId, string userId)
        {
            try
            {
  
                CookieContainer cookies = new CookieContainer();
                HttpClientHandler handler = new()
                {
                    CookieContainer = cookies
                };
                using (var client = new HttpClient(handler))
                {
                    cookies.Add(new Uri(UrlConnection.GetPointsAfterPurchase), new Cookie(".AspNet.ApplicationCookie", ParametersRepository.GetAuthTokenFromParameterTable()));

                    string requestUrl = $"{UrlConnection.GetPointsAfterPurchase}?ammount={amount}&storeId={storeId}&userId={userId}";
                    //string requestUrl = $"http://10.0.2.2/GCloud/api/HomeApi/UpdateUserTotalPointsAsync?ammount={amount}&storeId={storeId}&userId={userId}";

                    var response = await client.GetAsync(new Uri(requestUrl));

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
                            return "User points updated successfully.";
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

        public async Task<string> TransferPointsToUser(string userIdSender, string userReceiverEMail, int pointsToTransfer)
        {
            try
            {
                CookieContainer cookies = new();
                HttpClientHandler handler = new()
                {
                    CookieContainer = cookies
                };

                using (HttpClient httpClient = new HttpClient(handler))
                {
                    cookies.Add(new Uri(UrlConnection.TransferPointsToUser), new Cookie(".AspNet.ApplicationCookie", ParametersRepository.GetAuthTokenFromParameterTable()));

                    string requestUrl = $"{UrlConnection.TransferPointsToUser}?userIdSender={userIdSender}&userRecieverEMail={userReceiverEMail}&pointsToTransfer={pointsToTransfer}";
                    //string requestUrl = $"http://10.0.2.2/GCloud/api/HomeApi/TransferPointsProcedura?userIdSender={userIdSender}&userRecieverEMail={userReceiverEMail}&pointsToTransfer={pointsToTransfer}";


                    var response = await httpClient.GetAsync(requestUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        if (content.Contains("<!DOCTYPE html>"))
                        {
                            return "Error: Unauthorized access.";
                        }
                        else
                        {
                            return "Points transferred.";
                        }
                    }
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return "User not found";
                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ExceptionHandlerResult>(content);
                        return $"Error: {result?.Message ?? "Unknown error occurred."}";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public async Task<string> DecreasePointsFromUser(string userId, int points)
        {
            try
            {
                CookieContainer cookies = new();
                HttpClientHandler handler = new()
                {
                    CookieContainer = cookies
                };

                using (HttpClient httpClient = new HttpClient(handler))
                {
                    cookies.Add(new Uri(UrlConnection.DecreasePoints), new Cookie(".AspNet.ApplicationCookie", ParametersRepository.GetAuthTokenFromParameterTable()));

                    string requestUrl = $"{UrlConnection.DecreasePoints}?userId={userId}&points={points}";
                    //string requestUrl = $"http://10.0.2.2/GCloud/api/HomeApi/DecreasePointsProcedura?userId={userId}&points={points}";


                    var response = await httpClient.GetAsync(requestUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        if (content.Contains("<!DOCTYPE html>"))
                        {
                            return "Error: Unauthorized access.";
                        }
                        else
                        {
                            return "Points decreased.";
                        }
                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ExceptionHandlerResult>(content);
                        return $"Error: {result?.Message ?? "Unknown error occurred."}";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public async Task<string> BuyPoints(string userId, int moneyAmount, string storeId)
        {
            try
            {
                CookieContainer cookies = new();
                HttpClientHandler handler = new()
                {
                    CookieContainer = cookies
                };

                using (HttpClient httpClient = new HttpClient(handler))
                {
                    cookies.Add(new Uri(UrlConnection.BuyPoints), new Cookie(".AspNet.ApplicationCookie", ParametersRepository.GetAuthTokenFromParameterTable()));

                    string requestUrl = $"{UrlConnection.BuyPoints}?userId={userId}&moneyAmount={moneyAmount}&storeId={storeId}";
                    //string requestUrl = $"http://10.0.2.2/GCloud/api/HomeApi/BuyPointsProcedura?userId={userId}&moneyAmount={moneyAmount}&storeId={storeId}";


                    var response = await httpClient.GetAsync(requestUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        if (content.Contains("<!DOCTYPE html>"))
                        {
                            return "Error: Unauthorized access.";
                        }
                        else
                        {
                            return "Points bought.";
                        }
                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ExceptionHandlerResult>(content);
                        return $"Error: {result?.Message ?? "Unknown error occurred."}";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

    }
}
