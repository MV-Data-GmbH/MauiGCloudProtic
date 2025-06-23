using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Maui.Views;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Shared;
using Microsoft.Maui.Controls;

namespace GCloudPhone.Views.Settings.MyAccount
{
    public partial class AddressesBottomPopup : Popup
    {
        public ObservableCollection<Addresses> SavedAddresses { get; }
        private readonly Action<Addresses> _onAddressSelectedCallback;
        private readonly UserAddressService _userAddressService;

        public AddressesBottomPopup(Action<Addresses> onAddressSelectedCallback)
        {
            InitializeComponent();
            SavedAddresses = new ObservableCollection<Addresses>();
            _userAddressService = new UserAddressService();
            BindingContext = this;
            _onAddressSelectedCallback = onAddressSelectedCallback;
            LoadAddresses();
        }

        private async void LoadAddresses()
        {
            try
            {
                var ur = new UserRepository(DbBootstraper.Connection);
                var user = ur.GetCurrentUser();
                var addresses = await _userAddressService.GetAddressesByUserId(user.UserId);

                if (addresses == null || !addresses.Any())
                {
                    NoAddressesLabel.IsVisible = true;
                }
                else
                {
                    foreach (var addr in addresses)
                        SavedAddresses.Add(addr);
                }
            }
            catch (Exception)
            {
                await Application.Current.MainPage
                    .DisplayAlert("Error", "Unable to load addresses. Please try again.", "OK");
            }
        }

        private void OnCloseButtonClicked(object sender, EventArgs e)
            => Close();

        private async void OnAddNewAddressTapped(object sender, EventArgs e)
        {
            Close();
            await Application.Current.MainPage.Navigation.PushAsync(new AddAddressPage());
        }

        // ovo se poziva kad korisnik izabere stavku
        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = e.CurrentSelection.FirstOrDefault() as Addresses;
            if (selected != null)
            {
                _onAddressSelectedCallback?.Invoke(selected);
                Close();
            }
        }

        private async void ManageAddressesTapped(object sender, EventArgs e)
        {
            Close();
            await Application.Current.MainPage.Navigation.PushAsync(new ManageAddressesPage());
        }
    }
}
