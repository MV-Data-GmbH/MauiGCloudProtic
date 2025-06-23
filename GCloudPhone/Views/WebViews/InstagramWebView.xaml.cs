namespace GCloudPhone.Views.WebViews;

public partial class InstagramWebView : ContentPage
{
    public InstagramWebView()
    {
        InitializeComponent();
        webView.Source = new Uri("https://www.instagram.com/schnitzelwelt/?igshid=MzRlODBiNWFlZA%3D%3D");
    }
}