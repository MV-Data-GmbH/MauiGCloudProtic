using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace GCloud.Service
{
    public interface IEmailService
    {
        Task SendForgotPasswordEmail(Guid userGuid, ApplicationUserManager userManager);
        bool IsEmailValid(string emailaddress);

    }
}