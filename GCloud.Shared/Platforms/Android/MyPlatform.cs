using Android.App;

namespace GCloudPhone.Platforms.Android
{
    public static class MyPlatform
    {
        // Postavite trenutnu aktivnost. U MainActivity.cs, u OnCreate, dodajte:
        // MyPlatform.CurrentActivity = this;
        public static Activity CurrentActivity { get; set; }
    }
}