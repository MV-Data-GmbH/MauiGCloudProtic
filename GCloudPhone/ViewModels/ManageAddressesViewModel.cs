using GCloudPhone.Views.Settings.MyAccount;
using GCloudPhone.Views.Shop;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GCloudPhone.ViewModels
{
    public class ManageAddressesViewModel : INotifyPropertyChanged
    {
        private readonly UserAddressService _userAddressService;
        public ObservableCollection<Addresses> UserAddresses { get; set; }
        public ICommand DeleteAddressCommand { get; }
        public ICommand EditAddressCommand { get; }

        public ManageAddressesViewModel()
        {  
            _userAddressService = new UserAddressService();
            UserAddresses = new ObservableCollection<Addresses>();
            //LoadAddresses();
            DeleteAddressCommand = new Command<Addresses>(async (address) => await DeleteAddress(address));
            EditAddressCommand = new Command<Addresses>(OnEditAddress);
        }

        public async void LoadAddresses()
        {
            try
            {
                UserAddresses.Clear();
                UserRepository ur = new UserRepository(DbBootstraper.Connection);
                var user = ur.GetCurrentUser();
                string userId = user.UserId; 

                var addresses = await _userAddressService.GetAddressesByUserId(userId);

                foreach (var address in addresses)
                {
                    UserAddresses.Add(address);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading addresses: {ex.Message}");
            }
        }

        private async void OnEditAddress(Addresses address)
        {
            await Application.Current.MainPage.Navigation.PushAsync(new AddAddressPage(address));
        }

        private async Task DeleteAddress(Addresses address)
        {
            bool confirmed = await App.Current.MainPage.DisplayAlert("Bestätigung", "Möchten Sie diese Adresse wirklich löschen?", "Ja", "Nein");
            if (!confirmed) return;

            UserRepository ur = new UserRepository(DbBootstraper.Connection);
            var user = ur.GetCurrentUser();
            string userId = user.UserId;

            var result = await _userAddressService.DeleteAddress(userId, address.RemoteID);
            if (result == "Address Deleted.")
            {

                int rowsAffected = await SQL.DeleteAddressByRemoteIDAsync(address.RemoteID);

                if (rowsAffected > 0)
                {
                    UserAddresses.Remove(address);
                    await App.Current.MainPage.DisplayAlert("Erfolg", "Adresse erfolgreich gelöscht.", "OK");
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Fehler", "Die Adresse konnte nicht lokal gelöscht werden.", "OK");
                }
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Fehler", "Die Adresse konnte nicht gelöscht werden.", "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
