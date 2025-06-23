using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GCloud.Models
{
    public class RecoveryPasswordToWebShopModel
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
        public string Result { get; set; }
    }
}