// PickupCheckoutViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
          // za Preferences
using GCloudShared.Shared;            // za Stores model
using GCloudShared.Repository;
using Preferences = Microsoft.Maui.Storage.Preferences;       

namespace GCloudPhone.ViewModels
{
    public class PickupCheckoutViewModel : INotifyPropertyChanged
    {
        // Lista slobodnih termina
        public ObservableCollection<string> AvailableTimeSlots { get; }
            = new ObservableCollection<string>();

        // Lista filijala
        public ObservableCollection<Stores> Stores { get; }
            = new ObservableCollection<Stores>();

        // Stavke u korpi (ako ti treba u VM-u)
        public ObservableCollection<OrderItemViewModel> CartItems { get; set; }

        // Trenutno izabrana filijala
        private Stores _selectedStore;
        private bool isInitializing = true;
        public Stores SelectedStore
        {
            get => _selectedStore;
            set
            {
                if (_selectedStore != value)
                {
                    _selectedStore = value;
                    OnPropertyChanged();
                    if (!isInitializing && _selectedStore != null)
                    {
                        // Snimi u Preferences i odmah reload termini
                        Preferences.Set("SelectedStoreId", _selectedStore.Id.ToString());
                        _ = LoadTimeSlotsAsync(SelectedDate);
                    }
                }
            }
        }

        // Datum za koji biramo termin
        private DateTime _selectedDate = DateTime.Now;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate != value)
                {
                    _selectedDate = value;
                    OnPropertyChanged();
                    _ = LoadTimeSlotsAsync(_selectedDate);
                }
            }
        }

        // Najraniji datum (za DatePicker)
        public DateTime MinimumDate { get; } = DateTime.Now;

        // Izabrani termin
        private string _selectedTimeSlot;
        public string SelectedTimeSlot
        {
            get => _selectedTimeSlot;
            set
            {
                if (_selectedTimeSlot != value)
                {
                    _selectedTimeSlot = value;
                    OnPropertyChanged();
                }
            }
        }

        // Ukupno stavki, PDV, bakšiš i konačna cena
        private decimal _itemsTotal;
        public decimal ItemsTotal
        {
            get => _itemsTotal;
            set { if (_itemsTotal != value) { _itemsTotal = value; OnPropertyChanged(); } }
        }

        private decimal _vat;
        public decimal VAT
        {
            get => _vat;
            set { if (_vat != value) { _vat = value; OnPropertyChanged(); } }
        }

        private decimal? _tip;
        public decimal? Tip
        {
            get => _tip;
            set { if (_tip != value) { _tip = value; OnPropertyChanged(); } }
        }

        private decimal _totalPrice;
        public decimal TotalPrice
        {
            get => _totalPrice;
            set { if (_totalPrice != value) { _totalPrice = value; OnPropertyChanged(); } }
        }

        // Konstruktor: učitaj filijale
        public PickupCheckoutViewModel()
        {
            LoadStores();
        }

        // Učitaj sve filijale iz baze i iz Preferences
        private async void LoadStores()
        {
            try
            {
                var storesList = await SQL.GetAllStoresAsync();
                Stores.Clear();
                foreach (var store in storesList)
                    Stores.Add(store);

                var storedId = Preferences.Get("SelectedStoreId", string.Empty);
                if (!string.IsNullOrEmpty(storedId))
                    SelectedStore = Stores.FirstOrDefault(s => s.Id.ToString() == storedId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadStores error: {ex}");
            }
            finally
            {
                isInitializing = false;
                // Nakon inicijalizacije, odmah učitaj termine za izabrani datum
                _ = LoadTimeSlotsAsync(SelectedDate);
            }
        }

        // Učitaj sve vremenske slotove za datu filijalu i datum
        public async Task LoadTimeSlotsAsync(DateTime selectedDate)
        {
            AvailableTimeSlots.Clear();
            try
            {
                var sql = new SQL();
                var storeId = Preferences.Get("SelectedStoreId", string.Empty);
                var storedHours = await sql.GetAllStoredOpeningHours();

                if (storedHours?.Any() == true)
                {
                    var hours = storedHours.FirstOrDefault(h => h.StoreID == storeId);
                    if (hours != null)
                    {
                        var start = TimeSpan.Parse(hours.OpenFrom, CultureInfo.InvariantCulture);
                        var end = TimeSpan.Parse(hours.OpenTo, CultureInfo.InvariantCulture);
                        var slotDuration = TimeSpan.FromMinutes(hours.TimeSlotLength);
                        var now = DateTime.Now.TimeOfDay;

                        for (var t = start; t < end; t += slotDuration)
                        {
                            // filtriraj prošle termine ako je danas
                            if (selectedDate.Date == DateTime.Now.Date && t <= now)
                                continue;
                            AvailableTimeSlots.Add(t.ToString(@"hh\:mm"));
                        }
                    }
                }

                if (!AvailableTimeSlots.Any())
                    AvailableTimeSlots.Add("Keine verfügbaren Zeiten");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadTimeSlotsAsync error: {ex}");
                AvailableTimeSlots.Clear();
                AvailableTimeSlots.Add("Keine verfügbaren Zeiten");
            }
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
