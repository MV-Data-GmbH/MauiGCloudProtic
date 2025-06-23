

namespace GCloudShared.Service.Dto
{
    public class RegisterToWebShopResult
    {
        public IList<string> Errors { get; set; }

        public RegisterToWebShopResult()
        {
            this.Errors = new List<string>();
        }

        public bool Success
        {
            get { return this.Errors.Count == 0; }
        }

        public void AddError(string error)
        {
            this.Errors.Add(error);
        }
    }
}
