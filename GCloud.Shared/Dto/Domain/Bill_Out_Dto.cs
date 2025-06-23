

namespace GCloud.Shared.Dto.Domain
{
    public class Bill_Out_Dto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public Invoice Invoice { get; set; }
        public string Tags { get; set; }
    }
}