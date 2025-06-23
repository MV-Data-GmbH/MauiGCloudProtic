

namespace GCloud.Shared.Dto.Domain
{
    public class CompanyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string TaxNumber { get; set; }
        public string CommercialRegisterNumber { get; set; }
        public bool IsCashbackEnabled { get; set; }
        public string CompanyLogoBase64 { get; set; }
    }
}