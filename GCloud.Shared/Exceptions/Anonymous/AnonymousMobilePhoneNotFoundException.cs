

namespace GCloud.Shared.Exceptions.Anonymous
{
    public class AnonymousMobilePhoneNotFoundException : BaseGustavException
    {
        public Guid MobilePhoneGuid { get; set; }

        public AnonymousMobilePhoneNotFoundException(Guid mobilePhoneGuid) : base(ExceptionStatusCode.AnonymousMobilePhoneNotFound, "MobilePhoneId nicht gefunden.")
        {
            this.MobilePhoneGuid = mobilePhoneGuid;
        }
    }
}
