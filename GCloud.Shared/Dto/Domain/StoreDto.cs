

using Newtonsoft.Json;

namespace GCloud.Shared.Dto.Domain
{
    public class StoreDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string HouseNr { get; set; }
        public string Plz { get; set; }
        public DateTime CreationDateTime { get; set; }
        public CompanyDto Company { get; set; }
        public CountryDto Country { get; set; }
        public ICollection<TagDto> Tags { get; set; }
        //Zeigt an, ob der aktuell angemeldete Benutzer diesem Store folgt.
        public bool IsUserFollowing { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string BannerImage { get; set; }
        public string StoreWebSite { get; set; }

        [JsonProperty("FID")]
        public string ShortId { get; set; }
    }
}