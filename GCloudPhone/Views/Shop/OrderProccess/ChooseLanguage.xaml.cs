namespace GCloudPhone.Views.Shop.OrderProccess;

public partial class ChooseLanguage : ContentPage
{
	public ChooseLanguage()
	{
		InitializeComponent();
	}
    private async void OnLanguageTapped(object sender, EventArgs e)
    {
        var frame = (Frame)sender;
        string selectedLanguage = (string)((TapGestureRecognizer)frame.GestureRecognizers[0]).CommandParameter;

        switch (selectedLanguage)
        {
            case "German":
                // Handle German selection
                await DisplayAlert("Language Selected", "You selected German.", "OK");
                break;
            case "English":
                // Handle English selection
                await DisplayAlert("Language Selected", "You selected English.", "OK");
                break;
            case "Hungarian":
                // Handle Hungarian selection
                await DisplayAlert("Language Selected", "You selected Hungarian.", "OK");
                break;
            default:
                break;
        }
    }
}