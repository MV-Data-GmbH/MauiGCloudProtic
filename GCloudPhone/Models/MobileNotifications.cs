using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCloudPhone.Models
{
    public class MobileNotifications
    {
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
