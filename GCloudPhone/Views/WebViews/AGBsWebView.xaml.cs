namespace GCloudPhone.Views.WebViews;

public partial class AGBsWebView : ContentPage
{
	public AGBsWebView()
	{
		InitializeComponent();

        webView.Source = new Uri("https://schnitzelwelt.willessen.online/index.php/agb/");
    }

}
