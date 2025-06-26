using GCloudShared.Repository;
using GCloudShared.Shared;
using System.Diagnostics;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using GCloudPhone.Services;
using GCloudShared.Service;
using GCloudPhone.ViewModels;
using GCloudPhone.Views.Shop.OrderProccess;

namespace GCloudPhone.Views.Shop.Checkout
{
    public partial class InStoreCheckout : ContentPage
    {
        private InStoreCheckoutViewModel viewModel;
        private OrderViewModel currentOrder;

        private readonly UserPointsService _userPointsService;
        private readonly PaymentService _paymentService;
        private readonly OrderCommunicationService _orderCommunicationService;

        // Order note se postavlja pri pokretanju obrade narudžbine.
        private string _orderNote;

        // Zastavica koja sprečava višestruke pozive metode ProcessOrder
        private bool _isProcessingOrder = false;

        // Zastavica koja osigurava da se narudžbina obrađuje samo jednom
        private bool _orderProcessed = false;

        Button selectedButton;

        public InStoreCheckout(decimal totalPrice, decimal? TotalVAT, ObservableCollection<OrderItemViewModel> cartItems, string orderNote)
        {
            InitializeComponent();
            _paymentService = new PaymentService();
            viewModel = new InStoreCheckoutViewModel
            {
                ItemsTotal = totalPrice,
                TotalPrice = totalPrice,
                VAT = TotalVAT.HasValue ? TotalVAT.Value : 0m,
                CartItems = cartItems
            };
            BindingContext = viewModel;

            currentOrder = new OrderViewModel();
            _userPointsService = new UserPointsService();
            _orderCommunicationService = new OrderCommunicationService();
            PaymentMethodPicker.SelectedIndex = 0;

            // Računanje ukupnog iznosa, koristeći eksplicitnu zamenu null vrednosti (?? 0m)
            currentOrder.ItemsTotal = Cart.Instance.GetItems().Sum(item => (item.Amount ?? 0m) * (item.Quantity ?? 0));
            currentOrder.VAT = TotalVAT ?? 0m;
            currentOrder.Tip = 0; // inicijalno, ako nije postavljeno
            currentOrder.TotalPrice = currentOrder.ItemsTotal + (currentOrder.Tip ?? 0m);

#if ANDROID
            // Privremeno rešenje za poznati problem (#16737) na Android platformi
            Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("AppendPickerHandler", (handler, view) =>
            {
                Microsoft.Maui.Handlers.PickerHandler.MapTitleColor(handler, view);
            });
#endif

            // Postavljamo order note
            _orderNote = orderNote;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Ako je ukupna cena 0€, sakrivamo izbor načina plaćanja i opcije za napojnicu
            if (viewModel.TotalPrice == 0)
            {
                PaymentMethodContainer.IsVisible = false;
                TipContainer.IsVisible = false;
            }
            else
            {
                PaymentMethodContainer.IsVisible = true;
                TipContainer.IsVisible = true;
                SetPaymentMethods();
            }
        }

        private void SetPaymentMethods()
        {
            PaymentMethodPicker.Items.Clear();
#if IOS
            PaymentMethodPicker.Items.Add("Apple Pay");
#endif
#if ANDROID
            PaymentMethodPicker.Items.Add("Google Pay");
#endif
            PaymentMethodPicker.Items.Add("Kreditkarte");
            // Ako želite dodati i opciju "Bar bezahlen", otkomentarišite sledeću liniju:
            // PaymentMethodPicker.Items.Add("Bar bezahlen");
        }

        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            string result = await DisplayPromptAsync("Zusätzliche Informationen",
                                                       "Geben Sie zusätzliche Informationen oder Wünsche für Ihre Bestellung ein:",
                                                       "OK", "Abbrechen",
                                                       placeholder: "Ihre Nachricht",
                                                       maxLength: 250,
                                                       keyboard: Keyboard.Text);
            if (!string.IsNullOrEmpty(result))
            {
                currentOrder.DeliveryNotes = result;
            }
        }

        private void OnPaymentMethodChanged(object sender, EventArgs e)
        {
            if (sender is Picker picker && picker.SelectedItem != null)
            {
                currentOrder.PaymentMethod = picker.SelectedItem.ToString();
            }
        }

        private void OnTipButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                var tipAmount = Convert.ToDecimal(button.Text.Replace("€", ""));
                if (selectedButton == button)
                {
                    button.Style = (Style)Resources["UnselectedButtonStyle"];
                    viewModel.Tip = 0;
                    selectedButton = null;
                }
                else
                {
                    if (selectedButton != null)
                    {
                        selectedButton.Style = (Style)Resources["UnselectedButtonStyle"];
                    }
                    selectedButton = button;
                    selectedButton.Style = (Style)Resources["SelectedButtonStyle"];
                    viewModel.Tip = tipAmount;
                }
                UpdateTotalPrice();
            }
        }

        private async void OnCustomTipButtonClicked(object sender, EventArgs e)
        {
            var popup = new TipPopup();
            var result = await this.ShowPopupAsync(popup);
            if (result is decimal customTipAmount)
            {
                currentOrder.Tip = customTipAmount;
                selectedButton = null;
                UpdateTotalPrice();
            }
        }

        private void OnSwipedRight(object sender, SwipedEventArgs e)
        {
            Navigation.PopAsync();
        }

        private async void ConfirmButtonClicked(object sender, EventArgs e)
        {
            // Sprečavamo višestruke klikove koristeći zastavicu _isProcessingOrder
            if (_isProcessingOrder)
                return;

            _isProcessingOrder = true;

            try
            {
                // Ako je ukupna cena 0 €, direktno obrađujemo porudžbinu
                if (viewModel.TotalPrice == 0)
                {
                    string newOrderId = Guid.NewGuid().ToString();
                    await ProcessOrder(newOrderId);
                    return;
                }

                // Provera dostupnosti kase
                string cashRegisterName = await _orderCommunicationService.GetCashRegisterNameAsync();
                if (Config.PaymentWithoutDataTransfer == "No")
                {
                    if (string.IsNullOrEmpty(cashRegisterName) || !App.SignalR.OnlineUsers.Contains(cashRegisterName))
                    {
                        await DisplayAlert("Fehler", "Die Kasse ist offline.", "OK");
                        return;
                    }
                }

                // Generišemo novi OrderID pre pokretanja platnog procesa
                string newOrderIdForPayment = Guid.NewGuid().ToString();
                bool paymentSuccess = await _paymentService.ProcessPaymentAsync(currentOrder.PaymentMethod, currentOrder.TotalPrice, this, newOrderIdForPayment);
                if (paymentSuccess)
                {
                    await ProcessOrder(newOrderIdForPayment);
                }
                else
                {

                }
            }
            finally
            {
                _isProcessingOrder = false;
            }
        }

        public async Task ProcessOrderExternally()
        {
            // Ako eksterno pozivate ProcessOrder, generišite novi OrderID
            string newOrderId = Guid.NewGuid().ToString();
            await ProcessOrder(newOrderId);
        }

        private async Task ProcessOrder(string orderId)
        {
            // Ako je porudžbina već obrađena, odmah izađite
            if (_orderProcessed)
                return;

            _orderProcessed = true;

            // Kreiranje i čuvanje porudžbine u bazi
            var ur = new UserRepository(DbBootstraper.Connection);
            var user = ur.GetCurrentUser();
            string deliveryContact = $"{user.FirstName} {user.LastName}";

            var storeId = Preferences.Get("SelectedStoreId", string.Empty);
            var storeName = Preferences.Get("SelectedStoreName", "Unbekannte Filiale");
            var storeAddress = Preferences.Get("SelectedStoreAddress", "InStore");

            string plztxt = (App.OrderType == "Parking") ? "Parking" : "InStore";
            var order = new Orders
            {
                OrderID = orderId,
                UserID = user.UserId,
                StoreID = storeId,
                OrderDate = DateTime.Now,
                DeliveryDate = DateTime.Now.Date,
                DeliveryTime = DateTime.Now.ToString("HH:mm"),
                DeliveryAddress = storeAddress,
                DeliveryCity = storeName,
                DeliveryZip = plztxt,
                DeliveryCountry = "Austria",
                DeliveryPhone = "123456789",
                DeliveryEmail = user.Email,
                DeliveryContact = deliveryContact,
                DeliveryNotes = _orderNote,
                PaymentMethod = currentOrder.PaymentMethod,
                PaymentStatus = "Pending",
                OrderStatus = "Processing",
                TotalAmount = currentOrder.TotalPrice,
                TotalVAT = currentOrder.VAT,
                OrderType = App.OrderType,
                Reference = "",
                Tip = currentOrder.Tip
            };

            await SQL.SaveItemAsync(order);

            var orderItems = new List<OrderItems>();
            foreach (var item in Cart.Instance.Items)
            {
                var orderItem = new OrderItems
                {
                    OrderID = orderId,
                    ProductID = item.ProductID,
                    ProductDescription1 = item.ProductDescription1,
                    ProductDescription2 = item.ProductDescription2,
                    Amount = item.Amount ?? 0m,
                    // Uklonjen je sufiks 'm' jer je Quantity int? pa sada koristimo int literal 0
                    Quantity = item.Quantity ?? 0,
                    VAT = item.VAT ?? 0m,
                    Reference = item.Reference,
                    ItemNote = item.ItemNote
                };

                await SQL.SaveItemAsync(orderItem);
                orderItems.Add(orderItem);
            }

            // Slanje porudžbine u kasu
            bool orderSent = await _orderCommunicationService.SendOrderToServerAsync(order, orderItems);
            if (!orderSent)
            {
                await DisplayAlert("Fehler", "Die Bestellung konnte nicht an die Kasse gesendet werden. Bitte versuchen Sie es später erneut.", "OK");
                return;
            }

            // Ažuriranje bodova ako su korišćeni
            int usedPoints = Preferences.Get("UsedPoints", 0);
            if (usedPoints > 0)
            {
                string resultMessage = await _userPointsService.DecreasePointsFromUser(user.UserId, usedPoints);
                if (resultMessage.Contains("Points decreased"))
                {
                    Preferences.Remove("UsedPoints");
                }
                else
                {
                    await DisplayAlert("Fehler", "Beim Abziehen der Punkte ist ein Fehler aufgetreten.", "OK");
                    return;
                }
            }

            Cart.Instance.ClearCart();

            var orderWithItemsViewModel = new OrderWithItemsViewModel
            {
                Order = order,
                OrderItems = orderItems
            };

            int pointsEarned = Convert.ToInt32(currentOrder.TotalPrice);
            var updatePointsResult = await _userPointsService.UpdateUserPointsAfterPurchase(pointsEarned, storeId, user.UserId);
            if (pointsEarned > 0 && updatePointsResult is string message && message.Contains("User points updated successfully"))
            {
                await DisplayAlert("Glückwunsch", $"Mit diesem Kauf haben Sie {pointsEarned} Punkte erhalten.", "OK");
            }
            else
            {
                Debug.WriteLine("Fehler beim Aktualisieren der Punkte: " + (updatePointsResult?.ToString() ?? "Unbekannter Fehler"));
            }

            // Nakon obrade porudžbine, otvara se stranica sa detaljima porudžbine.
            await Navigation.PushAsync(new OrderDetailsPage(orderWithItemsViewModel));
        }

        private void UpdateTotalPrice()
        {
            viewModel.ItemsTotal = Cart.Instance.GetItems().Sum(item => item.Amount * item.Quantity ?? 0);
            viewModel.TotalPrice = viewModel.ItemsTotal + (viewModel.Tip ?? 0m);
        }
    }
}
