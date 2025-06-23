using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GCloud.Controllers.ViewModels.EMail
{
    public class ForGotPasswordEmailViewModel
    {
        public Guid UserGuid { get; set; }
        public string Code { get; set; }
    }
}