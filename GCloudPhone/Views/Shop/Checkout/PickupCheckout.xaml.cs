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
    public partial class PickupCheckout : ContentPage
    {
        public ObservableCollection<string> AvailableTimeSlots { get; set; }
        public string SelectedPickupTime { get; set; }
        private PickupCheckoutViewModel viewModel;
        private OrderViewModel currentOrder;
        private readonly UserPointsService _userPointsService;
        private readonly PaymentService _paymentService;
        private readonly OrderCommunicationService _orderCommunicationService;
        private string _orderId;
        private string _orderNote;
        Button selectedButton;

        public DateTime MinimumDate { get; } = DateTime.Now;
        public DateTime SelectedDate { get; set; } = DateTime.Now;

        public PickupCheckout(decimal totalPrice, decimal? TotalVAT, ObservableCollection<OrderItemViewModel> cartItems, string orderNote)
        {
            InitializeComponent();

            _paymentService = new PaymentService();
            _orderCommunicationService = new OrderCommunicationService();
            _userPointsService = new UserPointsService();

            viewModel = new PickupCheckoutViewModel
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

            // Calculate totals (for pickup, no delivery fee is added)
            currentOrder.ItemsTotal = Cart.Instance.GetItems().Sum(item => item.Amount * (item.Quantity ?? 0)) ?? 0;
            currentOrder.TotalPrice = currentOrder.ItemsTotal + currentOrder.VAT + (currentOrder.Tip ?? 0);
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

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            SetPaymentMethods();
            

            // 3) Odmah učitaj time‐slotove za izabrani datum
            try
            {
                await viewModel.LoadTimeSlotsAsync(viewModel.SelectedDate);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OnAppearing] LoadTimeSlotsAsync failed: {ex}");
                await DisplayAlert("Fehler", "Zeitfenster konnten nicht geladen werden.", "OK");
            }

            // Hide payment and tip sections if total price is 0
            PaymentMethodSection.IsVisible = viewModel.TotalPrice > 0;
            TipSection.IsVisible = viewModel.TotalPrice > 0;

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
            if (sender is Picker picker && picker.SelectedItem != null)
            {
                currentOrder.PaymentMethod = picker.SelectedItem.ToString();
            }
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

        private void OnTipButtonClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            decimal tipAmount = Convert.ToDecimal(button.Text.Replace("€", ""));
            if (selectedButton == button)
            {
                button.Style = (Style)Resources["UnselectedButtonStyle"];
                viewModel.Tip = 0;
                currentOrder.Tip = 0; // Osigurava da se postavi na 0
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
                currentOrder.Tip = tipAmount;
            }
            UpdateTotalPrice();
        }

        private async void OnCustomTipButtonClicked(object sender, EventArgs e)
        {
            var popup = new TipPopup();
            var result = await this.ShowPopupAsync(popup);
            if (result is decimal customTipAmount)
            {
                viewModel.Tip = customTipAmount;
                currentOrder.Tip = customTipAmount; // Osigurava da se ažurira i u order modelu
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
            var btn = (Button)sender;
            btn.IsEnabled = false;

            // Posle 10 sekundi ponovo omogućimo dugme
            Device.StartTimer(TimeSpan.FromSeconds(10), () =>
            {
                btn.IsEnabled = true;
                return false; // timer se izvršava samo jednom
            });

            // ————————————————
            // Ostatak tvog koda bez izmena:
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
                Debug.WriteLine($"[Checkout] cashRegisterName = '{cashRegisterName}'");
                Debug.WriteLine($"[Checkout] OnlineUsers = [{string.Join(", ", App.SignalR.OnlineUsers)}]");
                await DisplayAlert("Zahlung fehlgeschlagen", "Ihre Zahlung war nicht erfolgreich. Bitte versuchen Sie es erneut.", "OK");
            }
        }


        // Privatna metoda za proračun bodova - 1 bod za svaki puni euro
        private int CalculatePoints(decimal totalPrice)
        {
            return (int)totalPrice;
        }

        private async Task ProcessOrder(string selectedTimeSlot)
        {
            // Dobavljanje trenutnog korisnika i ostalih parametara
            UserRepository ur = new UserRepository(DbBootstraper.Connection);
            var user = ur.GetCurrentUser();
            string firstName = user.FirstName;
            string lastName = user.LastName;
            string deliveryContact = $"{firstName} {lastName}";
            string storeId = Preferences.Get("SelectedStoreId", string.Empty);
            var selectedDate = currentOrder.SelectedDate.Date;

            // Dodatni parametri za detalje dostave
            var storeName = Preferences.Get("SelectedStoreName", "Unbekannte Filiale");
            var storeAddress = Preferences.Get("SelectedStoreAddress", string.Empty);

            // Provjera da li je izabrani datum u prošlosti
            if (selectedDate < DateTime.Now.Date)
            {
                await DisplayAlert("Warnung", "Das gewählte Datum liegt in der Vergangenheit.", "OK");
                return;
            }

            // Kreiranje narudžbine sa svim detaljima
            var order = new Orders
            {
                OrderID = _orderId,
                UserID = user.UserId,
                StoreID = storeId,
                OrderDate = DateTime.Now,
                DeliveryDate = selectedDate,
                DeliveryTime = selectedTimeSlot,
                DeliveryAddress = "Abholung " + storeAddress,
                DeliveryCity = "Abholung " + storeName,
                DeliveryZip = "Abholung",
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

            // Kreiranje liste stavki narudžbine iz stavki u korpi
            var orderItems = new List<OrderItems>();
            foreach (var item in Cart.Instance.GetItems())
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

            // Slanje narudžbine ka kasi
            bool orderSent = await _orderCommunicationService.SendOrderToServerAsync(order, orderItems);
            if (!orderSent)
            {
                await DisplayAlert("Fehler", "Die Bestellung konnte nicht an die Kasse gesendet werden. Bitte versuchen Sie es später erneut.", "OK");
                return;
            }

            // Obrada bodova: smanjenje iskorištenih bodova ako ih je korisnik koristio
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

            // Čišćenje korpe i priprema modela za prikaz detalja narudžbine
            Cart.Instance.ClearCart();
            var orderWithItemsViewModel = new OrderWithItemsViewModel
            {
                Order = order,
                OrderItems = orderItems
            };

            // Ažuriranje bodova nakon kupovine (obračun bodova se izvršava samo ovde)
            int pointsEarned = CalculatePoints(currentOrder.TotalPrice);
            var updatePointsResult = await _userPointsService.UpdateUserPointsAfterPurchase(pointsEarned, storeId, user.UserId);
            if (updatePointsResult is string message && message.Contains("User points updated successfully") && pointsEarned > 0)
            {
                await DisplayAlert("Glückwunsch!", $"Mit diesem Kauf haben Sie {pointsEarned} Punkte erhalten.", "OK");
            }
            else
            {
                Debug.WriteLine("Error updating user points: " + (updatePointsResult?.ToString() ?? "Unknown error"));
            }

            // Prelazak na stranicu sa detaljima narudžbine
            await Navigation.PushAsync(new OrderDetailsPage(orderWithItemsViewModel));
        }

        public async Task ProcessOrderExternally()
        {
            string selectedTimeSlot = viewModel.SelectedTimeSlot;
            if (string.IsNullOrEmpty(selectedTimeSlot))
            {
                return;
            }
            await ProcessOrder(selectedTimeSlot);
        }

        private void UpdateTotalPrice()
        {
            viewModel.ItemsTotal = Cart.Instance.GetItems().Sum(item => item.Amount * (item.Quantity ?? 0)) ?? 0;
            viewModel.TotalPrice = viewModel.ItemsTotal + viewModel.VAT + (viewModel.Tip ?? 0m);
            currentOrder.TotalPrice = viewModel.TotalPrice; // Osigurava da i order model bude ažuriran
        }

        private async void On_PageLoaded(object sender, EventArgs e)
        {
            await LoadTimeSlotsAsync();
        }
    }
}
