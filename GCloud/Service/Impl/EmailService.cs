using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace GCloud.Service.Impl
{
    public class EmailService : IEmailService
    {
        private string GetBaseUrl(HttpContext currentContext = null)
        {
            //bug: Not working due to ReverseProxy Setup. Returns the wrong baseUrl (http://127.0.0.1:8098)

            //var req = currentContext.Request;
            //string baseUrl = $"{req.Url.Scheme}://{req.Url.Authority}/";
            //if (!string.IsNullOrWhiteSpace(req.ApplicationPath) && !req.ApplicationPath.Equals("/"))
            //{
            //    baseUrl += req.ApplicationPath;
            //}
            //return baseUrl;

            return GCloud.Shared.BaseUrlContainer.BaseUri.ToString();
        }

        public bool IsEmailValid(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public async Task SendForgotPasswordEmail(Guid userGuid, ApplicationUserManager userManager)
        {
            var urlHelper = new UrlHelper();
            var code = await userManager.GeneratePasswordResetTokenAsync(userGuid.ToString());

            var page = GetBaseUrl() + $"/Email/ForgotPasswordEmail?userGuid={HttpUtility.UrlEncode(userGuid.ToString())}&code={HttpUtility.UrlEncode(code)}";

            ////za lokal:
            //var page = GetBaseUrl().Replace(":80", "") + "GCloud/" + $"/Email/ForgotPasswordEmail?userGuid={HttpUtility.UrlEncode(userGuid.ToString())}&code={HttpUtility.UrlEncode(code)}";
            
            using (var client = new HttpClient())
            using (var response = await client.GetAsync(page))
            using (var content = response.Content)
            {
                var result = await content.ReadAsStringAsync();

                if (result != null && result.Length >= 50)
                {
                    await userManager.SendEmailAsync(userGuid.ToString(), "Passwort zurücksetzen", result);
                }
            }
        }
    }
}