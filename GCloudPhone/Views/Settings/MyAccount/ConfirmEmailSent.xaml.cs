using GCloudShared.Interface;

namespace GCloudPhone.Views.Settings.MyAccount;

public partial class ConfirmEmailSent : ContentPage
{
    private IAuthService _authService;

    public ConfirmEmailSent(IAuthService authService)
	{
		InitializeComponent();
        _authService = authService;
    }

    private void GoToLoginPageClicked(object sender, EventArgs e)
    {
       Navigation.PushAsync(new LoginPage(_authService));
    }
}