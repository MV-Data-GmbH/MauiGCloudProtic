
namespace GCloudShared.Domain
{
    public class Parameters:BasePersistable
    {
        public string ParameterName { get; set; }
        public string ParameterValue { get; set; }
        public TypeParameter TypeParameter { get; set; }
    }
}
