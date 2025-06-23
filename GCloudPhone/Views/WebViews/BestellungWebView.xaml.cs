using GCloudShared.Interface;
using GCloudShared.Repository;
using GCloudShared.Shared;


namespace GCloudPhone.Views.WebViews;

public partial class BestellungWebView : ContentPage
{
    private IAuthService _authService;

    private LoadingPage loadingPage;


    public BestellungWebView(string storeUrl, IAuthService authService)
    {
        InitializeComponent();

        _authService = authService;

        loadingPage = new LoadingPage();

        UserRepository ur = new UserRepository(DbBootstraper.Connection);
        var user = ur.GetCurrentUser();

        //storeUrl = "https://globalpaymentintegrationtest1.willessen.online";

        var validationUrl = $"{storeUrl}/customer/ValidationExample" + $"?email={user.Username}&password={user.Password}";
        //string wifiProtocol = "0";

        //var validationUrl = $"{storeUrl}customer/ValidationExample" +
        //$"?email={user.Email}&password={user.Password}&wifiProtocol={wifiProtocol}";

        webView.Source = new UrlWebViewSource { Url = validationUrl };

        Navigation.PushModalAsync(loadingPage);

        webView.Navigated += (sender, args) =>
        {
            Navigation.PopModalAsync(true);
        };
    }
   
    private void Button_Clicked(object sender, EventArgs e)
    {  
        Navigation.PopAsync();
    }
}