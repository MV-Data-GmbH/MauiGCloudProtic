using Newtonsoft.Json;

namespace GCloudPhone.Models
{
    public class QRCodeData
    {
        // Mapiranje ključa "FID" iz JSON-a na property ShortId
        [JsonProperty("FID")]
        public string ShortId { get; set; }

        // Mapiranje ključa "TN" iz JSON-a na property TableNumber
        [JsonProperty("TN")]
        public string TableNumber { get; set; }
    }
}