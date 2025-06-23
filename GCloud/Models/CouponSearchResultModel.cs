using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace GCloud.Models
{
    public class CouponSearchResultModel
    {
        [JsonProperty(PropertyName = "query")]
        public String Query { get; set; }

        [JsonProperty(PropertyName = "suggestions")]
        public List<CouponSearchResultSuggestionModel> Suggestions { get; set; }
    }

    public class CouponSearchResultSuggestionModel
    {
        [JsonProperty(PropertyName = "value")]
        public String Value { get; set; }
        [JsonProperty(PropertyName = "data")]
        public String Data { get; set; }
    }
}