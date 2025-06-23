using GCloudShared.Repository;
using GCloudShared.Shared;
using System.Diagnostics;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using GCloudPhone.Services;
using GCloudShared.Service;
using GCloudPhone.ViewModels;
using GCloudPhone.Views.Shop.OrderProccess;
using GCloudPhone.Views.Settings.MyAccount;

namespace GCloudPhone.Views.Shop.Checkout
{
    public partial class DeliveryCheckout : ContentPage
    {
        public ObservableCollection<string> AvailableTimeSlots { get; set; }
        public string SelectedDeliveryTime { get; set; }
        private DeliveryCheckoutViewModel viewModel;
        Button selectedButton;
        private Addresses _selectedAddress;
        private OrderViewModel currentOrder;

        private readonly UserPointsService _userPointsService;
        private readonly PaymentService _paymentService;
        private readonly OrderCommunicationService _orderCommunicationService;

        private string _orderId;
        private string _orderNote;

        public DateTime MinimumDate { get; } = DateTime.Now;
        public DateTime SelectedDate { get; set; } = DateTime.Now;

        public DeliveryCheckout(decimal totalPrice, decimal? TotalVAT, ObservableCollection<OrderItemViewModel> cartItems, string orderNote)
        {
            InitializeComponent();

            _paymentService = new PaymentService();
            _orderCommunicationService = new OrderCommunicationService();
            viewModel = new DeliveryCheckoutViewModel
            {
                ItemsTotal = totalPrice,
                TotalPrice = totalPrice,
                VAT = TotalVAT ?? 0,
                CartItems = cartItems
            };
            BindingContext = viewModel;

            currentOrder = new OrderViewModel { SelectedDate = DateTime.Now };
            AvailableTimeSlots = new ObservableCollection<string>();
            viewModel.SelectedDate = currentOrder.SelectedDate;

            _userPointsService = new UserPointsService();
            _selectedAddress = null;
            UpdateSelectedAddressDisplay();
            PaymentMethodPicker.SelectedIndex = 0;
            currentOrder.ItemsTotal = Cart.Instance.GetItems().Sum(item => item.Amount * item.Quantity ?? 0);
            currentOrder.TotalPrice = currentOrder.ItemsTotal + currentOrder.DeliveryFee + (currentOrder.Tip ?? 0);
            currentOrder.VAT = TotalVAT ?? 0;

           

#if ANDROID
            Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("AppendPickerHandler", (handler, view) =>
            {
                Microsoft.Maui.Handlers.PickerHandler.MapTitleColor(handler, view);
            });
#endif

            _orderId = Guid.NewGuid().ToString();
            _orderNote = orderNote;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            SetPaymentMethods();

            // If total is 0 EUR, hide PaymentMethod and Tip sections.
            if (viewModel.TotalPrice == 0)
            {
                PaymentMethodSection.IsVisible = false;
                TipSection.IsVisible = false;
            }
            else
            {
                PaymentMethodSection.IsVisible = true;
                TipSection.IsVisible = true;
            }
        }

        private async void OnCustomBackButtonClicked(object sender, EventArgs e)
        {
            bool userChoice = await DisplayAlert("Aktion auswählen",
                                                 "Möchten Sie Ihre Bestellung stornieren oder weiter einkaufen?",
                                                 "Bestellung stornieren",
                                                 "Weiter einkaufen");

            if (userChoice)
            {
                Cart.Instance.ClearCart();
                await Navigation.PushAsync(new MainPage());
            }
            else
            {
                await Navigation.PushAsync(new CategoriesPage());
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
           
        }

        private async void OnDateSelected(object sender, DateChangedEventArgs e)
        {
            if (e.NewDate < DateTime.Now.Date)
            {
                await DisplayAlert("Warnung", "Das gewählte Datum liegt in der Vergangenheit.", "OK");
                ((DatePicker)sender).Date = DateTime.Now;
                return;
            }
            currentOrder.SelectedDate = e.NewDate;
            viewModel.SelectedDate = e.NewDate;
            await viewModel.LoadTimeSlotsAsync(e.NewDate);
            Debug.WriteLine($"Selected Date Updated: {currentOrder.SelectedDate}");
        }

        public bool OnBackPressed()
        {
            ShowExitConfirmation();
            return true;
        }

        private async void ShowExitConfirmation()
        {
            bool userChoice = await DisplayAlert("Aktion auswählen",
                                                 "Möchten Sie Ihre Bestellung stornieren oder weiter einkaufen?",
                                                 "Bestellung stornieren",
                                                 "Weiter einkaufen");

            if (userChoice)
            {
                Cart.Instance.ClearCart();
                await Navigation.PushAsync(new MainPage());
            }
            else
            {
                await Navigation.PushAsync(new CategoriesPage());
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
            PaymentMethodPicker.SelectedIndex = 0;
        }

        private async Task LoadTimeSlotsAsync()
        {
            await viewModel.LoadTimeSlotsAsync(DateTime.Now);
        }

        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            string result = await DisplayPromptAsync("Zusätzliche Informationen",
                "Geben Sie zusätzliche Informationen oder Wünsche für Ihre Bestellung ein:",
                "OK", "Abbrechen", placeholder: "Ihre Nachricht", maxLength: 250, keyboard: Keyboard.Text);
            if (!string.IsNullOrEmpty(result))
            {
                currentOrder.DeliveryNotes = result;
            }
        }

        private void OnPaymentMethodChanged(object sender, EventArgs e)
        {
            var picker = sender as Picker;
            if (picker != null && picker.SelectedItem != null)
            {
                currentOrder.PaymentMethod = picker.SelectedItem.ToString();
            }
        }

        private void OnTipButtonClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
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

        private void OnAddressTapped(object sender, EventArgs e)
        {
            var popup = new AddressesBottomPopup(OnAddressSelected);
            this.ShowPopupAsync(popup);
        }

        private void OnAddressSelected(Addresses selectedAddress)
        {
            _selectedAddress = selectedAddress;
            UpdateSelectedAddressDisplay();
        }

        private void UpdateSelectedAddressDisplay()
        {
            if (_selectedAddress != null && !string.IsNullOrWhiteSpace(_selectedAddress.AddressLine1))
            {
                AddressLabel.Text = $"{_selectedAddress.AddressLine1}, {_selectedAddress.City}";
            }
            else
            {
                AddressLabel.Text = "Lieferadresse";
            }
        }

        private void OnSwipedRight(object sender, SwipedEventArgs e)
        {
            Navigation.PopAsync();
        }

        private async void ConfirmButtonClicked(object sender, EventArgs e)
        {
            string selectedTimeSlot = viewModel.SelectedTimeSlot;

            if (string.IsNullOrEmpty(selectedTimeSlot))
            {
                await DisplayAlert("Fehler", "Bitte wählen Sie eine Abholzeit aus.", "OK");
                return;
            }

            string cashRegisterName = await _orderCommunicationService.GetCashRegisterNameAsync();

            if (Config.PaymentWithoutDataTransfer == "No")
            {
                try
                {
                    if (string.IsNullOrEmpty(cashRegisterName) || !App.SignalR.OnlineUsers.Contains(cashRegisterName))
                    {
                        await DisplayAlert("Fehler", "Ziel offline", "OK");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error Target offline: {ex.Message}");
                }
            }

            // Ako je TotalPrice = 0, preskačemo plaćanje i idemo direktno na ProcessOrder
            if (viewModel.TotalPrice == 0)
            {
                await ProcessOrder(selectedTimeSlot);
                return;
            }

            bool paymentSuccess = await _paymentService.ProcessPaymentAsync(currentOrder.PaymentMethod, currentOrder.TotalPrice, this, _orderId);

            if (paymentSuccess)
            {
                await ProcessOrder(selectedTimeSlot);
            }
            else
            {
                await DisplayAlert("Zahlung fehlgeschlagen", "Ihre Zahlung war nicht erfolgreich. Bitte versuchen Sie es erneut.", "OK");
            }
        }

        public async Task ProcessOrderExternally()
        {
            string selectedTimeSlot = viewModel.SelectedTimeSlot;
            await ProcessOrder(selectedTimeSlot);
        }

        private async Task ProcessOrder(string selectedTimeSlot)
        {
            UserRepository ur = new UserRepository(DbBootstraper.Connection);
            var user = ur.GetCurrentUser();
            string firstName = user.FirstName;
            string lastName = user.LastName;
            string deliveryContact = $"{firstName} {lastName}";
            var storeId = Preferences.Get("SelectedStoreId", string.Empty);
            var selectedDate = currentOrder.SelectedDate.Date;
            if (selectedDate < DateTime.Now.Date)
            {
                await DisplayAlert("Warnung", "Das gewählte Datum liegt in der Vergangenheit.", "OK");
                return;
            }
            var order = new Orders
            {
                OrderID = _orderId,
                UserID = user.UserId,
                StoreID = storeId,
                OrderDate = DateTime.Now,
                DeliveryDate = selectedDate,
                DeliveryTime = selectedTimeSlot,
                DeliveryAddress = _selectedAddress.AddressLine1,
                DeliveryCity = _selectedAddress.City,
                DeliveryZip = _selectedAddress.Zip,
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
                    OrderID = _orderId,
                    ProductID = item.ProductID,
                    ProductDescription1 = item.ProductDescription1,
                    ProductDescription2 = item.ProductDescription2,
                    Amount = item.Amount,
                    Quantity = item.Quantity,
                    VAT = item.VAT,
                    Reference = item.Reference,
                    ItemNote = item.ItemNote
                };
                await SQL.SaveItemAsync(orderItem);
                orderItems.Add(orderItem);
            }
            bool orderSent = await _orderCommunicationService.SendOrderToServerAsync(order, orderItems);
            if (!orderSent)
            {
                await DisplayAlert("Fehler", "Die Bestellung konnte nicht an die Kasse gesendet werden. Bitte versuchen Sie es später erneut.", "OK");
                return;
            }
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
            int pointsEarned = (int)currentOrder.TotalPrice;
            var updatePointsResult = await _userPointsService.UpdateUserPointsAfterPurchase(pointsEarned, storeId, user.UserId);
            if (pointsEarned > 0 && updatePointsResult is string message && message.Contains("User points updated successfully"))
            {
                await DisplayAlert("Glückwunsch!", $"Mit diesem Kauf haben Sie {pointsEarned} Punkte erhalten.", "OK");
            }
            else
            {
                Debug.WriteLine("Error updating user points: " + (updatePointsResult?.ToString() ?? "Unknown error"));
            }
            await Navigation.PushAsync(new OrderDetailsPage(orderWithItemsViewModel));
        }

        private void UpdateTotalPrice()
        {
            viewModel.ItemsTotal = Cart.Instance.GetItems().Sum(item => item.Amount * item.Quantity ?? 0);
            viewModel.TotalPrice = viewModel.ItemsTotal + currentOrder.DeliveryFee + (viewModel.Tip ?? 0m);
        }

        private async void On_PageLoaded(object sender, EventArgs e)
        {
            await LoadTimeSlotsAsync();
        }
    }
}
