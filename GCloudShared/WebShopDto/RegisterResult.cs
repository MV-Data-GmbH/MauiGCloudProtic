using Newtonsoft.Json;


namespace GCloudShared.WebShopDto
{
    public class RegisterResult
    {
        public String Status;
        public String RegisterType;

        [JsonIgnore]
        public RegisterResultStatus StatusE { get; set; }
        public UserRegistrationType RegisterTypeE { get; set; }
        public List<ModelError> Errors { get; set; }
    }
    public enum RegisterResultStatus
    {
        Success,
        Failed
    }
    public enum UserRegistrationType : int
    {
        /// <summary>
        /// Standard account creation
        /// </summary>
        Standard = 1,
        /// <summary>
        /// Email validation is required after registration
        /// </summary>
        EmailValidation = 2,
        /// <summary>
        /// A customer should be approved by administrator
        /// </summary>
        AdminApproval = 3,
        /// <summary>
        /// Registration is disabled
        /// </summary>
        Disabled = 4,
    }
}
