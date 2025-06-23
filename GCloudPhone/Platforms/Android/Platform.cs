using Android.App;

namespace GCloudPhone.Platforms.Android
{
    public static class Platform
    {
        // Čuva referencu na trenutnu aktivnost
        public static Activity CurrentActivity { get; set; }
    }
}