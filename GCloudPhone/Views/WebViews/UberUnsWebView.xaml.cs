namespace GCloudPhone.Views.WebViews;

public partial class UberUnsWebView : ContentPage
{
    public UberUnsWebView()
    {
        InitializeComponent();

        webView.Source = new UrlWebViewSource
        {
            Url = "https://www.schnitzelwelt.at/home#Qualitaet"
        };
    }
}