using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GCloud.Controllers.ViewModels.Bill
{
    public class ProfileViewModel
    {
        public string Name { get; set; }
        public string SurName { get; set; }
        public int BillCount { get; set; }
        public decimal BillSum { get; set; }
        public int StoreCount { get; set; }
        public bool RegisteredUser { get; set; }
    }
}