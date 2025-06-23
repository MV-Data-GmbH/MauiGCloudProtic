using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCloudPhone.ViewModels
{
    public class InStoreCheckoutViewModel : INotifyPropertyChanged
    {
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

        public InStoreCheckoutViewModel()
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

                string storedStoreId = Preferences.Get("SelectedStoreId", string.Empty);
                if (!string.IsNullOrEmpty(storedStoreId))
                {
                    SelectedStore = Stores.FirstOrDefault(store => store.Id.ToString() == storedStoreId);
                }

                isInitializing = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadStores error: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
