

namespace GCloud.Shared.Dto.Api
{
    public class AvailableStoresResponse
    {
        public Guid StoreId { get; set; }
        public string StoreName { get; set; }
    }

    public class DeviceConfigResponse
    {
        public string ApiToken { get; set; }
        public Guid CashRegisterId { get; set; }
    }
}