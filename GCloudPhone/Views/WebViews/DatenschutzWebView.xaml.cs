
namespace GCloudPhone.Views.WebViews;

public partial class DatenschutzWebView : ContentPage
{
	public DatenschutzWebView()
	{
		InitializeComponent();

        webView.Source = new Uri("https://www.schnitzelundmehr.at/datenschutz");

    }
}

