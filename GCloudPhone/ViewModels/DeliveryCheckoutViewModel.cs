using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GCloudPhone.ViewModels
{
    public class DeliveryCheckoutViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<string> AvailableTimeSlots { get; set; } = new ObservableCollection<string>();

        private string _selectedTimeSlot;
        public string SelectedTimeSlot
        {
            get => _selectedTimeSlot;
            set
            {
                _selectedTimeSlot = value;
                OnPropertyChanged(nameof(SelectedTimeSlot));
            }
        }
        private ObservableCollection<OrderItemViewModel> _cartItems;
        public ObservableCollection<OrderItemViewModel> CartItems
        {
            get => _cartItems;
            set
            {
                _cartItems = value;
                OnPropertyChanged(nameof(CartItems));
            }
        }


        private decimal _itemsTotal;
        public decimal ItemsTotal
        {
            get => _itemsTotal;
            set
            {
                _itemsTotal = value;
                OnPropertyChanged(nameof(ItemsTotal));
            }
        }

        private decimal _deliveryFee;
        public decimal DeliveryFee
        {
            get => _deliveryFee;
            set
            {
                _deliveryFee = value;
                OnPropertyChanged(nameof(DeliveryFee));
            }
        }

        private decimal _vat;
        public decimal VAT
        {
            get => _vat;
            set
            {
                _vat = value;
                OnPropertyChanged(nameof(VAT));
            }
        }

        private decimal? _tip;
        public decimal? Tip
        {
            get => _tip;
            set
            {
                _tip = value;
                OnPropertyChanged(nameof(Tip));
            }
        }

        private decimal _totalPrice;
        public decimal TotalPrice
        {
            get => _totalPrice;
            set
            {
                _totalPrice = value;
                OnPropertyChanged(nameof(TotalPrice));
            }
        }
        public ObservableCollection<Stores> Stores { get; set; } = new ObservableCollection<Stores>();
        private Stores _selectedStore;

        public Stores SelectedStore
        {
            get => _selectedStore;
            set
            {
                _selectedStore = value;
                OnPropertyChanged(nameof(SelectedStore));

                // Save the selected store ID to Preferences if it's a user-selected change
                if (_selectedStore != null && !isInitializing)
                {
                    Preferences.Set("SelectedStoreId", _selectedStore.Id.ToString());
                }
            }
        }

        private bool isInitializing = true;

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
                }
            }
        }
       

        public DeliveryCheckoutViewModel()
        {
            LoadStores();
        }

        private async void LoadStores()
        {
            try
            {
                var storesList = await SQL.GetAllStoresAsync();
                Stores.Clear();

                foreach (var store in storesList)
                {
                    Stores.Add(store);
                }

                // Set the default store based on Preferences
                string storedStoreId = Preferences.Get("SelectedStoreId", string.Empty);
                if (!string.IsNullOrEmpty(storedStoreId))
                {
                    SelectedStore = Stores.FirstOrDefault(store => store.Id.ToString() == storedStoreId);
                }

                isInitializing = false; // Finished initialization
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadStores error: {ex.Message}");
            }
        }



        //public event PropertyChangedEventHandler PropertyChanged;
        //protected virtual void OnPropertyChanged(string propertyName)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task LoadTimeSlotsAsync(DateTime selectedDate)
        {
            AvailableTimeSlots.Clear();

            var sqlInstance = new SQL();
            var storeId = Preferences.Get("SelectedStoreId", string.Empty);

            var storedHours = await sqlInstance.GetAllStoredOpeningHours();

            if (storedHours != null && storedHours.Count > 0)
            {
                var storeOpeningHours = storedHours.FirstOrDefault(sh => sh.StoreID == storeId);
                if (storeOpeningHours != null)
                {
                    TimeSpan startTime = TimeSpan.Parse(storeOpeningHours.OpenFrom, CultureInfo.InvariantCulture);
                    TimeSpan endTime = TimeSpan.Parse(storeOpeningHours.OpenTo, CultureInfo.InvariantCulture);
                    int timeSlotLength = storeOpeningHours.TimeSlotLength;

                    TimeSpan slotLength = TimeSpan.FromMinutes(timeSlotLength);

                    TimeSpan currentTime = DateTime.Now.TimeOfDay;

                    for (TimeSpan time = startTime; time < endTime; time += slotLength)
                    {
                        // Filter past times only for the current day
                        if (selectedDate.Date == DateTime.Now.Date && time <= currentTime)
                        {
                            continue;
                        }

                        AvailableTimeSlots.Add(time.ToString(@"hh\:mm"));
                    }

                    if (AvailableTimeSlots.Count == 0)
                    {
                        AvailableTimeSlots.Add("Keine verfügbaren Zeiten");
                    }
                }
                else
                {
                    AvailableTimeSlots.Add("Keine verfügbaren Zeiten");
                }
            }
            else
            {
                AvailableTimeSlots.Add("Keine verfügbaren Zeiten");
            }
        }

    }
}
