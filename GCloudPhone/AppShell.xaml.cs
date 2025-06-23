using GCloudPhone.Views.Shop;
using GCloudPhone.Views.Shop.Payments;
using GCloudPhone.Views.WebViews;

namespace GCloudPhone;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
        Routing.RegisterRoute(nameof(WebViewPage), typeof(WebViewPage));
        Routing.RegisterRoute(nameof(SuccessfulPayment), typeof(SuccessfulPayment));
       
    }
}
