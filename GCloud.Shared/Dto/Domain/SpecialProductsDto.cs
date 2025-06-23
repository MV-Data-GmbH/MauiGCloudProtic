

namespace GCloud.Shared.Dto.Domain
{
    public class SpecialProductsDto
    {
        public Guid Id { get; set; }


        public string Name { get; set; }

        public string ShortDescription { get; set; }


        public decimal Value { get; set; }

        public string CreatedUserId { get; set; }

        public bool Enabled { get; set; }

        public bool IsDeleted { get; set; }


    }
}
