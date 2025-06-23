using GCloudPhone.Views.Aktionen;
using GCloudPhone.Views.Points;
using GCloudPhone.Views.Shop;
using GCloudPhone.Views.Shop.OrderProccess;
using GCloudShared.Interface;

namespace GCloudPhone.Views;

public partial class ProduktePage : ContentPage
{
    private IAuthService _authService;

    public ProduktePage(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
        webView.Source = new Uri("https://schnitzelwelt.willessen.online/index.php/schnitzelwelt-homepage/");

        // Pretplata na doga?aje NavigationBar-a
        navigationBar.HomeTapped += NavigationBar_HomeTapped;
        navigationBar.ProductTapped += NavigationBar_ProductTapped;
        navigationBar.BestellenTapped += NavigationBar_BestellenTapped;
        navigationBar.AktionenTapped += NavigationBar_AktionenTapped;
        navigationBar.PunkteTapped += NavigationBar_PunkteTapped;
    }

    private async void NavigationBar_HomeTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MainPage());
    }

    private async void NavigationBar_ProductTapped(object sender, EventArgs e)
    {
        // Ve? se nalazimo na strani ProduktePage – ovde možete izvršiti osvežavanje ili ostaviti prazno.
    }

    private async void NavigationBar_BestellenTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new OrderTypePage(_authService));
    }

    private async void NavigationBar_AktionenTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AktionenPage(_authService));
    }

    private async void NavigationBar_PunkteTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MyPointsPage(_authService));
    }
}
