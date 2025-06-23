using GCloudPhone.ViewModels;
using GCloudPhone.Views.Shop;

namespace GCloudPhone.Views.Settings.MyAccount;

public partial class ManageAddressesPage : ContentPage
{
    private ManageAddressesViewModel _viewModel;
    public ManageAddressesPage()
	{
		InitializeComponent();
        _viewModel = new ManageAddressesViewModel();
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadAddresses();
    }

    private async void AddAddressClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddAddressPage());
    }
}