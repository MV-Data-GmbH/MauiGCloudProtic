using GCloud.Shared.Exceptions;
using GCloudPhone;
using GCloudShared.Repository;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace GCloudShared.Service
{
    public class UserAddressService
    {
        public async Task<string> AddUserAddress(Addresses addresses)
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

                    string requestUrl = $"{UrlConnection.AddAddress}";

                    var jsonContent = JsonConvert.SerializeObject(addresses);
                    var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync(requestUrl, stringContent);

                    if (response.IsSuccessStatusCode)
                    {
                        return "Address Added.";
                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        try
                        {
                            var result = JsonConvert.DeserializeObject<ExceptionHandlerResult>(content);
                            return $"Error: {result?.Message ?? "Unknown error occurred."}";
                        }
                        catch
                        {
                            return $"Error: {response.StatusCode} - {content}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public async Task<List<Addresses>> GetAddressesByUserId(string userId)
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
                    cookies.Add(new Uri(UrlConnection.GetAddressByUserId), new Cookie(".AspNet.ApplicationCookie", ParametersRepository.GetAuthTokenFromParameterTable()));

                    string requestUrl = $"{UrlConnection.GetAddressByUserId}?userId={Uri.EscapeDataString(userId)}";

                    var response = await httpClient.GetAsync(requestUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<List<Addresses>>(content);
                    }
                    else
                    {
                        throw new HttpRequestException($"Error: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting addresses: {ex.Message}");
            }
        }

        public async Task<string> UpdateAddress(Addresses addresses)
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

                    string requestUrl = $"{UrlConnection.UpdateAddress}";

                    var jsonContent = JsonConvert.SerializeObject(addresses);
                    var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    var response = await httpClient.PutAsync(requestUrl, stringContent);

                    if (response.IsSuccessStatusCode)
                    {
                        return "Address Updated.";
                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return $"Error: {response.StatusCode} - {content}";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public async Task<string> DeleteAddress(string userId, string remoteId)
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

                    string requestUrl = $"{UrlConnection.DeleteAddress}?userId={Uri.EscapeDataString(userId)}&remoteId={Uri.EscapeDataString(remoteId)}";

                    var response = await httpClient.DeleteAsync(requestUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        return "Address Deleted.";
                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return $"Error: {response.StatusCode} - {content}";
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
