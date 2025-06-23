

namespace GCloud.Shared.Exceptions.Anonymous
{
    public class AnonymousUserNotFoundException : BaseGustavException
    {
        public Guid AnonymerUserId { get; set; }

        public AnonymousUserNotFoundException(Guid anonymousUserId) : base(ExceptionStatusCode.AnonymousUserNotFound, "Anonymer Benutzer wurde nicht gefunden.")
        {
            AnonymerUserId = anonymousUserId;
        }
    }
}
