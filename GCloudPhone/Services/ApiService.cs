using GCloudPhone.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GCloudPhone.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<UrlResponse> GetApiDataAsync(string parameter, string ordernumber, string? fastpayid, string paymethods = "CRD")
        {
            // Define the API URL with the query parameter
            string url = $"https://testpaymentsmobilelocalv2protic.willessen.online/api/values?amount={parameter}&ordernumber={ordernumber}&fastpayid={fastpayid}&paymethods={paymethods}";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();

                UrlResponse? urlResponse = JsonSerializer.Deserialize<UrlResponse>(content);

                return urlResponse;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Request error: {ex.Message}");
                return null;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON error: {ex.Message}");
                return null;
            }
        }
    }

}
