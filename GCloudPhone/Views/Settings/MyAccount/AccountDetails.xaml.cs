namespace GCloudPhone.Views.Settings.MyAccount;

public partial class AccountDetails : ContentPage
{
	public AccountDetails()
	{
		InitializeComponent();
	}
 
    private void OnNameTapped(object sender, EventArgs e)
    {
        //// Navigate to Name Edit Page
        //await Navigation.PushAsync(new NameEditPage());
    }

    private void OnEmailTapped(object sender, EventArgs e)
    {
        //// Navigate to Email Edit Page
        //await Navigation.PushAsync(new EmailEditPage());
    }

    private void OnPhoneNumberTapped(object sender, EventArgs e)
    {
        //// Navigate to Phone Number Edit Page
        //await Navigation.PushAsync(new PhoneNumberEditPage());
    }
}