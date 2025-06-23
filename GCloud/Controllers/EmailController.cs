using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GCloud.Controllers.ViewModels.EMail;

namespace GCloud.Controllers
{
    public class EmailController : Controller
    {
        // GET: Email
        public ActionResult ForgotPasswordEmail(Guid userGuid, string code)
        {
            var model = new ForGotPasswordEmailViewModel()  { UserGuid = userGuid, Code = code };
            return View(model);
        }
    }
}