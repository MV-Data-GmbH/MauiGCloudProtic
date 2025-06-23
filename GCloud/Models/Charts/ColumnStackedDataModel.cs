using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GCloud.Models.Charts
{
    public class ColumnStackedDataModel
    {
        public string Name { get; set; }
        public IList<int> Values { get; set; }
    }
}