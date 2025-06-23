using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GCloud.Shared.Dto.Domain;
using GCloudShared.Interface;
using GCloudShared.Service;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using GCloudPhone.Views.Aktionen;
using GCloudPhone.Views.Points;
using GCloudPhone.Views.Settings.MyAccount;
using GCloudPhone.Views.Shop;
using GCloudPhone.Services;

namespace GCloudPhone.Views.Shop.OrderProccess
{
    public partial class ChooseFiliale : ContentPage, INotifyPropertyChanged
    {
       
        private List<Stores> _stores = new List<Stores>();
        public List<Stores> Stores
        {
            get => _stores;
            set
            {
                if (_stores != value)
                {
                    _stores = value;
                    OnPropertyChanged(nameof(Stores));
                }
            }
        }

        private readonly IAuthService _authService;
        private bool IsLogging;

        public ICommand TapCommand { get; }

        public ChooseFiliale(IAuthService authService)
        {
            InitializeComponent();

            Debug.WriteLine("[ChooseFiliale] Konstruktor: Ulazim u konstruktor");

            BindingContext = this;

            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            Debug.WriteLine("[ChooseFiliale] Konstruktor: authService nije null");

            // Provera da li je korisnik prijavljen
            BestelungLogged(authService);

            // Komanda za tap na pojedinačni store (prosleđuje se ID kao string)
            TapCommand = new Command<string>(ExecuteTapCommand);

            Debug.WriteLine("[ChooseFiliale] Konstruktor: TapCommand postavljen");
            // Pretplata na događaje NavigationBar-a
            navigationBar.HomeTapped += NavigationBar_HomeTapped;
            navigationBar.ProductTapped += NavigationBar_ProductTapped;
            navigationBar.BestellenTapped += NavigationBar_BestellenTapped;
            navigationBar.AktionenTapped += NavigationBar_AktionenTapped;
            navigationBar.PunkteTapped += NavigationBar_PunkteTapped;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Debug.WriteLine("[ChooseFiliale] OnAppearing: Pozvana metoda");

            try
            {
                if (IsLogging)
                {
                    Debug.WriteLine("[ChooseFiliale] OnAppearing: IsLogging == true, čistim Stores i učitavam ponovo...");
                    Stores.Clear();
                    LoadStores();
                }
                else
                {
                    Debug.WriteLine("[ChooseFiliale] OnAppearing: IsLogging == false, ne učitavam Stores");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ChooseFiliale] OnAppearing: Exception -> {ex.Message}");
                DisplayAlert("Greška", $"Dogodila se greška u OnAppearing: {ex.Message}", "OK");
            }
        }

        private async void BestelungLogged(IAuthService authService)
        {
            Debug.WriteLine("[ChooseFiliale] BestelungLogged: Proveravam authService.IsLogged()");

            if (authService == null)
            {
                Debug.WriteLine("[ChooseFiliale] BestelungLogged: authService == null -> bacam ArgumentNullException");
                throw new ArgumentNullException(nameof(authService));
            }

            try
            {
                if (!authService.IsLogged())
                {
                    Debug.WriteLine("[ChooseFiliale] BestelungLogged: Korisnik NIJE prijavljen");
                    IsLogging = false;
                    await DisplayAlert("Fehler", "Sie müssen angemeldet sein, um fortzufahren!", "OK");
                    Debug.WriteLine("[ChooseFiliale] BestelungLogged: Navigacija ka LoginPage");
                    await Navigation.PushAsync(new LoginPage(authService));
                }
                else
                {
                    Debug.WriteLine("[ChooseFiliale] BestelungLogged: Korisnik je prijavljen");
                    IsLogging = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ChooseFiliale] BestelungLogged: Exception -> {ex.Message}");
                await DisplayAlert("Greška", $"Neuspešna provera logovanja: {ex.Message}", "OK");
            }
        }

        private async void LoadStores()
        {
            Debug.WriteLine("[ChooseFiliale] LoadStores: Ulazim u metodu");

            try
            {
                // Ovde zovemo servis za dohvat svih filijala
                Debug.WriteLine("[ChooseFiliale] LoadStores: Pozivam SQL.GetAllStoresAsync()");
                var storesList = await SQL.GetAllStoresAsync();

                if (storesList != null && storesList.Any())
                {
                    Debug.WriteLine($"[ChooseFiliale] LoadStores: Dobio sam {storesList.Count} stavki");

                    foreach (var store in storesList)
                    {
                        Debug.WriteLine($"[ChooseFiliale] LoadStores: Dodajem store.Id = {store.Id}, store.Name = {store.Name}");

                        Stores.Add(new Stores
                        {
                            Id = store.Id,
                            Name = store.Name,
                            City = store.City,
                            Street = store.Street,
                            HouseNr = store.HouseNr,
                            Plz = store.Plz,
                            Latitude = store.Latitude,
                            Longitude = store.Longitude,
                        });
                    }

                    // Već koristimo BindingContext = this, pa je StoresList.ItemsSource automatski povezan
                    Debug.WriteLine("[ChooseFiliale] LoadStores: Postavljam StoresList.ItemsSource = Stores");
                    StoresList.ItemsSource = Stores;
                }
                else
                {
                    Debug.WriteLine("[ChooseFiliale] LoadStores: storesList je prazan ili null");
                    await DisplayAlert("Fehler", "Es ist ein Fehler aufgetreten oder es gibt keine Filialen!", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ChooseFiliale] LoadStores: Exception -> {ex.Message}");
                await DisplayAlert("Error", $"Failed to load stores: {ex.Message}", "OK");
            }
        }

        private async void ExecuteTapCommand(string id)
        {
            Debug.WriteLine($"[ChooseFiliale] ExecuteTapCommand: Ulazni id = '{id}'");

            try
            {
                // Pronađi store u lokalnoj kolekciji po ID-ju
                var selectedStore = Stores.FirstOrDefault(store => store.Id == id);
                Debug.WriteLine(selectedStore != null
                    ? $"[ChooseFiliale] ExecuteTapCommand: Pronađen store: Id='{selectedStore.Id}', Name='{selectedStore.Name}'"
                    : "[ChooseFiliale] ExecuteTapCommand: Nije pronađen nijedan store sa tim ID-jem");

                if (selectedStore != null)
                {
                    Debug.WriteLine($"[ChooseFiliale] ExecuteTapCommand: Čuvam Preferences: SelectedStoreId = '{id}'");
                    Debug.WriteLine($"[ChooseFiliale] ExecuteTapCommand: Čuvam Preferences: SelectedStoreName = '{selectedStore.Name}'");
                    Debug.WriteLine($"[ChooseFiliale] ExecuteTapCommand: Čuvam Preferences: SelectedStoreAddress = '{selectedStore.Street} {selectedStore.HouseNr}'");

                    Preferences.Set("SelectedStoreId", id);
                    Preferences.Set("SelectedStoreName", selectedStore.Name);
                    Preferences.Set("SelectedStoreAddress", $"{selectedStore.Street} {selectedStore.HouseNr}");

                    Debug.WriteLine("[ChooseFiliale] ExecuteTapCommand: Navigacija na CategoriesPage");
                    await Navigation.PushAsync(new CategoriesPage());
                }
                else
                {
                    Debug.WriteLine("[ChooseFiliale] ExecuteTapCommand: selectedStore == null, prikazujem alert");
                    await DisplayAlert("Fehler", "Filiale konnte nicht gefunden werden.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ChooseFiliale] ExecuteTapCommand: Exception -> {ex.Message}");
                await DisplayAlert("Error", $"Navigation error: {ex.Message}", "OK");
            }
        }

        private void OnSwipedRight(object sender, SwipedEventArgs e)
        {
            Debug.WriteLine("[ChooseFiliale] OnSwipedRight: Pozvan swipe event, vraćam se nazad");
            try
            {
                Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ChooseFiliale] OnSwipedRight: Exception -> {ex.Message}");
            }
        }

        private async void NavigationBar_HomeTapped(object sender, EventArgs e)
        {
            // Već smo na Home, možete ostaviti prazno ili refresovati.
        }

        private async void NavigationBar_ProductTapped(object sender, EventArgs e)
        {
            if (Connectivity.NetworkAccess.HasFlag(NetworkAccess.Internet))
                await Navigation.PushAsync(new SettingsPage());
        }

        private async void NavigationBar_BestellenTapped(object sender, EventArgs e)
        {
            if (Connectivity.NetworkAccess.HasFlag(NetworkAccess.Internet))
                await Navigation.PushAsync(new OrderTypePage(_authService));
        }

        private async void NavigationBar_AktionenTapped(object sender, EventArgs e)
        {
            if (Connectivity.NetworkAccess.HasFlag(NetworkAccess.Internet))
                await Navigation.PushAsync(new AktionenPage(_authService));
        }

        private async void NavigationBar_PunkteTapped(object sender, EventArgs e)
        {
            if (Connectivity.NetworkAccess.HasFlag(NetworkAccess.Internet))
                await Navigation.PushAsync(new MyPointsPage(_authService));
        }


        #region INotifyPropertyChanged

        public new event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            Debug.WriteLine($"[ChooseFiliale] OnPropertyChanged: '{propertyName}' je promenjeno");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
