using GCloudPhone.Services;
#if IOS
using GCloudPhone.Platforms.iOS;
#elif ANDROID
using GCloudPhone.Platforms.Android;
#endif
using Microsoft.Maui;
using Microsoft.Extensions.Logging.Debug;
using Microsoft.Maui.Hosting;
using CommunityToolkit.Maui;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using Microsoft.Extensions.Logging; 

namespace GCloudPhone
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiCommunityToolkit()
                .UseMauiApp<App>()
               
                .UseBarcodeReader() // Inicijalizacija ZXing.Net.MAUI barcode readera
                .ConfigureMauiHandlers(handlers =>
                {
                    // Dodajte platform-specifične handlere ako su potrebni
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("montserrat_regular.ttf", "Montserrat");
                    fonts.AddFont("Qucikand-Regular.ttf", "Quicksand");
                    fonts.AddFont("Qucikand-Bold.ttf", "Quicksand");
                    fonts.AddFont("CascadiaCode-Regular-VTT.ttf", "CascadiaCode");
                });

           


#if IOS
            builder.Services.AddSingleton<IQrScannerService, QrScannerService_iOS>();
#elif ANDROID
            builder.Services.AddSingleton<IQrScannerService, QrScannerService_Android>();
#else
            // Registrujte servise za ostale platforme ako ih imate
#endif

            return builder.Build();
        }
    }
}
