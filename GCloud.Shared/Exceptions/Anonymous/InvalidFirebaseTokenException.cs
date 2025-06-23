

namespace GCloud.Shared.Exceptions.Anonymous
{
    public class InvalidFirebaseTokenException : BaseGustavException
    {
        public string FirebaseToken { get; set; }

        public InvalidFirebaseTokenException(string firebaseToken) : base(ExceptionStatusCode.InvalidFirebaseToken,
            "Es wurde ein ungültiger Firebase Token übergeben.")
        {
            this.FirebaseToken = firebaseToken;
        }
    }
}
