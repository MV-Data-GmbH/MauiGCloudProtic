

namespace GCloudShared.Service.Dto
{
    public class SpecialProductsResultModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string ShortDescription { get; set; }

        public decimal Value { get; set; }

        public Guid CreatedUserId { get; set; }

        public bool Enabled { get; set; }

        public bool IsDeleted { get; set; }
    }
}
