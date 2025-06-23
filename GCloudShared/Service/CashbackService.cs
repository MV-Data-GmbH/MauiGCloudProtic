using GCloud.Shared.Dto.Domain;
using GCloud.Shared.Exceptions;
using GCloudShared.Interface;
using Newtonsoft.Json;
using System.Web;

namespace GCloudShared.Service
{
    public class CashbackService : ICashbackService
    {
        //public static async Task<object> GetCashbacksForStore(string storeGuid)
        //{
        //    try
        //    {
        //        using (var client = new HttpClient())
        //        {
        //            var request = new HttpRequestMessage();

        //            var builder = new UriBuilder("http://dimitry.foodjet.online/api/Cashback");

        //            var query = HttpUtility.ParseQueryString(builder.Query);
        //            query["storeGuid"] = storeGuid;

        //            builder.Query = query.ToString();

        //            string url = builder.ToString();
        //            var response = await client.GetAsync(url);
        //            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        //            {
        //                var content = await response.Content.ReadAsStringAsync();
        //                var res = JsonConvert.DeserializeObject<ExceptionHandlerResult>(content);
        //                return res;
        //            }
        //            else
        //            {
        //                var content = await response.Content.ReadAsStringAsync();
        //                var res = JsonConvert.DeserializeObject<List<CashbackDto>>(content);
        //                return res;
        //            }





        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}
        public async Task<object> GetCashbacksForStore(string storeGuid)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage();

                    var builder = new UriBuilder(UrlConnection.GetCashbacksForStoreUrl);

                    var query = HttpUtility.ParseQueryString(builder.Query);
                    query["storeGuid"] = storeGuid;

                    builder.Query = query.ToString();

                    string url = builder.ToString();
                    var response = await client.GetAsync(url);
                    if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var res = JsonConvert.DeserializeObject<ExceptionHandlerResult>(content);
                        return res;
                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var res = JsonConvert.DeserializeObject<List<CashbackDto>>(content);
                        return res;
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
