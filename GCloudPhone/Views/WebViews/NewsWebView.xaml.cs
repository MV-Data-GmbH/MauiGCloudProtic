namespace GCloudPhone.Views.WebViews;

public partial class NewsWebView : ContentPage
{
    public NewsWebView()
    {
        InitializeComponent();
        webView.Source = new Uri("https://www.schnitzelwelt.at/news");
    }
}