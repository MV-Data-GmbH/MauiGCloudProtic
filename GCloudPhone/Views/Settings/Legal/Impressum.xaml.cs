namespace GCloudPhone.Views.Settings.Legal;

public partial class Impressum : ContentPage
{
	public Impressum()
	{
		InitializeComponent();
        webView.Source = new Uri("https://www.schnitzelundmehr.at/impressum");
    }
}
