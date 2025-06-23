

namespace GCloud.Shared.Dto.Domain
{
    public class UserDto 
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public bool IsActive { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public virtual ICollection<CompanyDto> Companies { get; set; }
        public virtual ICollection<StoreDto> InterrestedStores { get; set; }

        public virtual ICollection<UserDto> CreatedUsers { get; set; }

        public CashbackDto LastCashback { get; set; }

        public string CreatedById { get; set; }
        public virtual UserDto CreatedBy { get; set; }
    }
}