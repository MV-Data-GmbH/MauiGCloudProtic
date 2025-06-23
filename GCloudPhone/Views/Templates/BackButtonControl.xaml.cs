namespace GCloudPhone.Views.Templates;

public partial class BackButtonControl : ContentView
{
	public BackButtonControl()
	{
		InitializeComponent();
	}
    private void OnBackButtonClicked(object sender, System.EventArgs e)
    {
        Navigation.PopAsync();
    }
}