using GCloudShared.Interface;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Shared;
using GCloudShared.SocketServer;
using GCloudShared.WebShopDto;
using Newtonsoft.Json;
using GCloud.Shared.Dto.Domain;
using GCloudPhone.Services;

namespace GCloudPhone.Views.WebViews;

public partial class BestellungWebView2 : ContentPage
{
    private IAuthService _authService;

    private LoadingPage loadingPage;


    public BestellungWebView2(string storeUrl, IAuthService authService)
    {
        InitializeComponent();

        _authService = authService;



        UserRepository ur = new UserRepository(DbBootstraper.Connection);
        var user = ur.GetCurrentUser();

        loadingPage = new LoadingPage();

        //storeUrl = "http://192.168.2.132/";
        //storeUrl = "https://schnitzelundmehrtest.foodjet.online/";
        //storeUrl = "https://filiale6.foodjet.online/";
        //storeUrl = "https://globalpaymentintegrationtest1.willessen.online/";

        //wifiProtocol - 0 - full webshop
        //wifiProtocol - 1 - partial webshop
        string wifiProtocol = "1";

        var validationUrl = $"{storeUrl}/customer/ValidationExample" +
        $"?email={user.Email}&password={user.Password}&wifiProtocol={wifiProtocol}";

        //string hardcodedEmail = "aaaa@gmail.com";
        //string hardcodedPassword = "test12345"; 

        //var validationUrl = $"{storeUrl}customer/ValidationExample" +
        //$"?email={hardcodedEmail}&password={hardcodedPassword}&wifiProtocol={wifiProtocol}";

     


        webView.Source = new UrlWebViewSource { Url = validationUrl };

        Navigation.PushModalAsync(loadingPage);

        try
        {
            webView.Navigating += async (sender, args) =>
            {
                var url = args.Url;
                if (args.Url.Contains("Complete"))
                {
                    await GetandInsertLastOrder();
                }
            };

           

        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred during navigation event handling: {ex.Message}");
            DisplayAlert("error", "navigacija", "ok");
        }
       

        webView.Navigated += (sender, args) =>
        {
            Navigation.PopModalAsync(true);
        };

    }


    private async Task GetandInsertLastOrder()
    {
        WebShopService ws = new WebShopService();
        var results = await ws.GetLastOrder();
        if (results == null)
        {
            await DisplayAlert("Fehler beim Abrufen der Bestellungen", "OK", "Abbrechen");
            return;
        }

        var info = results as OrderKuechenDisplay;
        if (info != null)
        {

            SendToDispayKuechenService sc = new SendToDispayKuechenService();
            OrderKuechenDisplay order = new OrderKuechenDisplay();
            UserRepository ur = new UserRepository(DbBootstraper.Connection);
            var user = ur.GetCurrentUser();

            info.Email = user.Email;

            var resultlist = sc.GetService(info);
            if (resultlist != null)
            {
                if (resultlist[0] == 1)
                {
                    //  DisplayAlert("Uspesna narudzbina", "OK", "Cancel");
                }
                else
                {
                    var infoJson = JsonConvert.SerializeObject(info);
                    var dataforQRCode = CompressString.Compress(infoJson);
                    await Navigation.PushAsync(new OrderQRCode(dataforQRCode));
                }

            }
            else
            {
                var infoJson = JsonConvert.SerializeObject(info);
                var dataforQRCode = CompressString.Compress(infoJson);
                await Navigation.PushAsync(new OrderQRCode(dataforQRCode));
            }
        }

    }
    private void Button_Clicked(object sender, EventArgs e)
    {
        var qrScannerService = ((App)Application.Current).Services.GetService(typeof(IQrScannerService)) as IQrScannerService;
       Navigation.PushAsync(new BestellungInDerFiliale(_authService, qrScannerService));
    }

    protected override bool OnBackButtonPressed()
    {
        var qrScannerService = ((App)Application.Current).Services.GetService(typeof(IQrScannerService)) as IQrScannerService;
        Navigation.PushAsync(new BestellungInDerFiliale(_authService, qrScannerService));
        return true;
    }
}