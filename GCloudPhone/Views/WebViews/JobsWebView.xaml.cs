namespace GCloudPhone.Views.WebViews;

public partial class JobsWebView : ContentPage
{
    public JobsWebView()
    {
        InitializeComponent();
        webView.Source = new Uri("https://www.schnitzelwelt.at/jobs");
    }

}