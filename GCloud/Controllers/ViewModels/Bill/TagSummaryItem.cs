using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GCloud.Controllers.ViewModels.Bill
{
    public class TagSummaryItem
    {
        public string TagName { get; set; }
        public int Count { get; set; }
        public decimal Amount { get; set; }
    }
}