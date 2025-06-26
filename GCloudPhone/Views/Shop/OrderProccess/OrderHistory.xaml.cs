using CommunityToolkit.Maui.Views;
using GCloudPhone;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using System.IO;
using System;
using System.Threading.Tasks;
using GCloudShared.Interface;
using GCloudPhone.Services;
using GCloudPhone.Views.Shop.OrderProcesses;

namespace GCloudPhone.Views.Shop.OrderProccess
{
    public partial class OrderHistory : ContentPage
    {
        private readonly IAuthService _authService;

        public ObservableCollection<OrderWithItemsViewModel> Orders { get; set; }
        public ICommand TapCommand { get; set; }
        public ICommand ToggleOrderItemsCommand { get; }

        // Konstruktor koji prima IAuthService kao parametar.
        public OrderHistory(IAuthService authService)
        {
            InitializeComponent();
            _authService = authService;

            Orders = new ObservableCollection<OrderWithItemsViewModel>();
            TapCommand = new Command<OrderWithItemsViewModel>(OnOrderTapped);
            ToggleOrderItemsCommand = new Command<OrderWithItemsViewModel>(OnToggleOrderItems);

            BindingContext = this;

            LoadOrders();
            OrdersCollectionView.ItemsSource = Orders;
        }

        private async void LoadOrders()
        {
            var processedOrders = await SQL.GetProcessedOrdersAsync();

            if (processedOrders.Count == 0)
            {
                NoOrders.IsVisible = true;
                return;
            }

            foreach (var order in processedOrders)
            {
                Orders.Add(order);
            }
        }


        private async void OnOrderTapped(OrderWithItemsViewModel selectedOrder)
        {
            // Prvo pitamo za ponavljanje narudžbine
            var repeatPopup = new RepeatOrderPopup();
            await this.ShowPopupAsync(repeatPopup);
            bool repeatOrder = await repeatPopup.TaskCompletionSource.Task;
            if (!repeatOrder)
                return;

            // Pitamo za način dostave
            var deliveryPopup = new DeliveryQuestionPopup();
            var deliveryMethod = await this.ShowPopupAsync(deliveryPopup) as string;
            if (string.IsNullOrEmpty(deliveryMethod))
            {
                await DisplayAlert("Fehler", "Es wurde keine Bestellart ausgewählt.", "OK");
                return;
            }

            // Čistimo korpu i dodajemo stavke iz stare narudžbine...
            Cart.Instance.ClearCart();
            foreach (var item in selectedOrder.OrderItems)
            {
                if (!item.IsCoupon)
                {
                    decimal newPrice = await GetPriceForDeliveryMethodAsync(item.ProductID, deliveryMethod);
                    var cartItem = new OrderItemViewModel
                    {
                        ProductID = item.ProductID,
                        ProductDescription1 = item.ProductDescription1,
                        ProductDescription2 = item.ProductDescription2,
                        Amount = newPrice,
                        Quantity = item.Quantity,
                        VAT = item.VAT,
                        Reference = item.Reference,
                        ItemNote = item.ItemNote
                    };
                    Cart.Instance.AddItem(cartItem);
                }
            }

            switch (deliveryMethod)
            {
                case "PickUp":
                    // Učitaj sve filijale
                    var stores = await SQL.GetAllStoresAsync();

                    if (stores.Count == 1)
                    {
                        // Ako postoji samo jedna, automatski je biramo
                        Preferences.Set("SelectedStoreId", stores[0].Id.ToString());
                    }
                    // Ako ih ima više, korisnik će sam da izabere u ChooseFiliale
                    await Navigation.PushAsync(new ChooseFiliale(_authService));
                    break;

                case "DineIn":
                    App.IsRepeatOrder = true;
                    await Navigation.PushAsync(new NFCReaderPage(new NfcService()));
                    break;

                case "Parking":
                    App.IsRepeatOrder = true;
                    var qrService = ((App)Application.Current)
                                        .Services.GetService(typeof(IQrScannerService))
                                        as IQrScannerService;
                    await Navigation.PushAsync(new BestellungInDerFiliale(_authService, qrService));
                    break;

                default:
                    await DisplayAlert("Fehler", "Ungültige Bestellart.", "OK");
                    break;
            }
        }


        /// <summary>
        /// Asinhrona metoda koja dohvaća cenu proizvoda za datu metodu dostave.
        /// Mapa (primer):
        ///     "Delivery" → PricesType = 5
        ///     "PickUp"  → PricesType = 6
        ///     "DineIn"  → PricesType = 1
        ///     "Parking" → PricesType = 4

        private async Task<decimal> GetPriceForDeliveryMethodAsync(int productId, string deliveryMethod)
        {
            int priceType = 0;
            switch (deliveryMethod)
            {
                case "Delivery":
                    priceType = 5;
                    break;
                case "PickUp":
                    priceType = 6;
                    break;
                case "DineIn":
                    priceType = 1;
                    break;
                case "Parking":
                    priceType = 4;
                    break;
                default:
                    priceType = 0;
                    break;
            }

            var priceRecord = await SQL.GetItemAsync<Prices>(p => p.Products == productId && p.PricesType == priceType);
            if (priceRecord != null)
                return priceRecord.Amount;
            return 0;
        }

        private void OnToggleOrderItems(OrderWithItemsViewModel selectedOrder)
        {
            foreach (var order in Orders)
            {
                if (order != selectedOrder)
                {
                    order.IsOrderItemsVisible = false;
                }
            }
            selectedOrder.IsOrderItemsVisible = !selectedOrder.IsOrderItemsVisible;
        }
    }
}
