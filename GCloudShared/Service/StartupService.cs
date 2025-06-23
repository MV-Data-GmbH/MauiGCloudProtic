using GCloud.Shared.Dto.Api;
using GCloudShared.Interface;
using GCloud.Shared.Exceptions;
using GCloudShared.Service.Dto;
using Newtonsoft.Json;
using System.Web;

namespace GCloudShared.Service
{
    public class StartupService:IStartupService
    {
        public async Task<object> GetBackGroundImages(string alreadyDownloaded)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage();

                    var builder = new UriBuilder(UrlConnection.GetBackGroundImagesUrl);

                    var query = HttpUtility.ParseQueryString(builder.Query);
                    query["alreadyDownloaded"] = alreadyDownloaded;


                    builder.Query = query.ToString();

                    string url = builder.ToString();
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        JsonSerializerSettings js = new JsonSerializerSettings()
                        {
                            ContractResolver = new ShouldDeserializeContractResolver()
                        };

                        var content = await response.Content.ReadAsStringAsync();
                        var res = JsonConvert.DeserializeObject<List<ImageViewModel>>(content, js);
                        return res;
                    }
                    else
                    {
                        JsonSerializerSettings js = new JsonSerializerSettings()
                        {
                            ContractResolver = new ShouldDeserializeContractResolver()
                        };

                        var content = await response.Content.ReadAsStringAsync();
                        var res = JsonConvert.DeserializeObject<ExceptionHandlerResult>(content, js);
                        return res;
                    }

                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<object> LoadInitialData(bool includeImage, bool includeBanner, bool includeCompanyLogo)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage();

                    var builder = new UriBuilder(UrlConnection.LoadInitialDataUrl);

                    var query = HttpUtility.ParseQueryString(builder.Query);
                    query["includeImage"] = includeImage.ToString();
                    query["includeBanner"] = includeBanner.ToString();
                    query["includeCompanyLogo"] = includeCompanyLogo.ToString();

                    builder.Query = query.ToString();

                    string url = builder.ToString();
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        JsonSerializerSettings js = new JsonSerializerSettings()
                        {
                            ContractResolver = new ShouldDeserializeContractResolver()
                        };

                        var content = await response.Content.ReadAsStringAsync();
                        var res = JsonConvert.DeserializeObject<LoadInitialDataResponseModel>(content, js);
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
    }
}
