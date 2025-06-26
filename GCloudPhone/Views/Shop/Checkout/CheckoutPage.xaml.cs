using CommunityToolkit.Maui.Views;
using GCloudPhone.Models;
using GCloudPhone.Services;
using GCloudPhone.ViewModels;
using GCloudPhone.Views.Settings.MyAccount;
using GCloudPhone.Views.Shop.OrderProccess;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Shared;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Optional;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace GCloudPhone.Views.Shop.Checkout
{
    [QueryProperty(nameof(Mode), "mode")]
    [QueryProperty(nameof(OrderNote), "orderNote")]
    public partial class CheckoutPage : ContentPage
    {
        readonly PaymentService _paymentService = new();
        readonly OrderCommunicationService _orderCommunicationService = new();
        readonly UserPointsService _userPointsService = new();

        CheckoutViewModel viewModel;
        string _orderId;
        bool _orderProcessed;
        Addresses _selectedAddress;

        public CheckoutPage()
        {
            InitializeComponent();
            InitializeViewModel(CheckoutType.Pickup);
        }

        void InitializeViewModel(CheckoutType type)
        {
            viewModel = new CheckoutViewModel(type);

            if (viewModel.IsInStore)
            {
                viewModel.SelectedDate = DateTime.Now.Date;
                viewModel.SelectedTimeSlot = DateTime.Now.ToString("HH:mm");
            }

            BindingContext = viewModel;
            _orderId = Guid.NewGuid().ToString();
            Debug.WriteLine($"[CheckoutPage] Initialized with mode={type}, orderId={_orderId}");
        }

        public string Mode
        {
            set
            {
                try
                {
                    var modeString = Preferences.Get("CheckoutMode", value);
                    if (!Enum.TryParse<CheckoutType>(modeString, true, out var t))
                        t = CheckoutType.Pickup;
                    InitializeViewModel(t);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[CheckoutPage] Mode setter error: {ex}");
                }
            }
        }

        public string OrderNote
        {
            set
            {
                try
                {
                    if (viewModel != null)
                        viewModel.OrderNote = Uri.UnescapeDataString(value);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[CheckoutPage] OrderNote setter error: {ex}");
                }
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                Debug.WriteLine("[CheckoutPage] OnAppearing start");
                await viewModel.LoadStoresAsync();
                Debug.WriteLine("[CheckoutPage] Stores loaded");

                viewModel.PaymentMethods.Clear();
#if IOS
                viewModel.PaymentMethods.Add("Apple Pay");
#endif
#if ANDROID
                viewModel.PaymentMethods.Add("Google Pay");
#endif
                viewModel.PaymentMethods.Add("Kreditkarte");
                viewModel.PaymentMethods.Add("Bar");
                viewModel.PaymentMethod = viewModel.PaymentMethods.FirstOrDefault();
                Debug.WriteLine($"[CheckoutPage] PaymentMethods setup: {string.Join(", ", viewModel.PaymentMethods)}");

                if (viewModel.ShowDateTimeOptions)
                {
                    await viewModel.LoadTimeSlotsAsync(viewModel.SelectedDate);
                    Debug.WriteLine("[CheckoutPage] Time slots loaded");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckoutPage] OnAppearing error: {ex}");
                await DisplayAlert("Fehler", "Beim Laden der Daten ist ein Fehler aufgetreten.", "OK");
            }
            PaymentMethodSection.IsVisible = viewModel.TotalPrice > 0;
            TipSection.IsVisible = viewModel.TotalPrice > 0;
        }

        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("[CheckoutPage] Opening custom info prompt");
                var result = await DisplayPromptAsync(
                    "Zusätzliche Informationen",
                    "Geben Sie zusätzliche Informationen oder Wünsche für Ihre Bestellung ein:",
                    "OK", "Abbrechen",
                    placeholder: "Ihre Nachricht",
                    maxLength: 250,
                    keyboard: Keyboard.Text);

                if (!string.IsNullOrEmpty(result))
                {
                    viewModel.OrderNote = result;
                    Debug.WriteLine($"[CheckoutPage] OrderNote set to: {result}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckoutPage] TapGestureRecognizer_Tapped error: {ex}");
                await DisplayAlert("Fehler", "Ein Fehler ist aufgetreten.", "OK");
            }
        }

        void OnPaymentMethodChanged(object sender, EventArgs e)
        {
            try
            {
                if (sender is Picker p && p.SelectedItem != null)
                {
                    viewModel.PaymentMethod = p.SelectedItem.ToString();
                    Debug.WriteLine($"[CheckoutPage] PaymentMethod changed to: {viewModel.PaymentMethod}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckoutPage] OnPaymentMethodChanged error: {ex}");
            }
        }

        async void OnDateChanged(object sender, DateChangedEventArgs e)
        {
            try
            {
                if (e.NewDate < DateTime.Now.Date)
                {
                    Debug.WriteLine("[CheckoutPage] User selected past date");
                    await DisplayAlert("Warnung", "Datum in der Vergangenheit.", "OK");
                    (sender as DatePicker).Date = DateTime.Now;
                    return;
                }
                viewModel.SelectedDate = e.NewDate;
                Debug.WriteLine($"[CheckoutPage] SelectedDate set to: {viewModel.SelectedDate:d}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckoutPage] OnDateChanged error: {ex}");
                await DisplayAlert("Fehler", "Ein Fehler ist aufgetreten.", "OK");
            }
        }

        async void OnAddressTapped(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("[CheckoutPage] Opening address picker popup");
                var popup = new AddressesBottomPopup(addr =>
                {
                    _selectedAddress = addr;
                    AddressLabel.Text = $"{addr.AddressLine1}, {addr.Zip} {addr.City}";
                    Debug.WriteLine($"[CheckoutPage] Delivery address set to: {AddressLabel.Text}");
                });
                await this.ShowPopupAsync(popup);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckoutPage] OnAddressTapped error: {ex}");
                await DisplayAlert("Fehler", "Ein Fehler ist aufgetreten.", "OK");
            }
        }

        void OnTipButtonClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button btn)
                {
                    var amt = Convert.ToDecimal(btn.Text.Replace("€", ""));
                    foreach (var b in TipButtonsLayout.Children.OfType<Button>())
                        b.Style = (Style)Resources["UnselectedButtonStyle"];
                    btn.Style = (Style)Resources["SelectedButtonStyle"];
                    viewModel.Tip = amt;
                    Debug.WriteLine($"[CheckoutPage] Tip set to: {amt}€");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckoutPage] OnTipButtonClicked error: {ex}");
            }
        }

        async void OnCustomTipButtonClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("[CheckoutPage] Opening custom tip popup");
                var popup = new TipPopup();
                var res = await this.ShowPopupAsync(popup);
                if (res is decimal customAmt)
                {
                    viewModel.Tip = customAmt;
                    Debug.WriteLine($"[CheckoutPage] Custom tip set to: {customAmt}€");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckoutPage] OnCustomTipButtonClicked error: {ex}");
            }
        }

        async void ConfirmButtonClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("[CheckoutPage] ConfirmButtonClicked start");
                var missing = new List<string>();

                if (viewModel.ShowDateTimeOptions)
                {
                    if (viewModel.SelectedDate.Date < DateTime.Now.Date)
                        missing.Add("Datum");
                    if (string.IsNullOrEmpty(viewModel.SelectedTimeSlot))
                        missing.Add("Zeit");
                }

                if (viewModel.IsDelivery && _selectedAddress == null)
                    missing.Add("Adresse");

                if (string.IsNullOrEmpty(viewModel.PaymentMethod))
                    missing.Add("Zahlungsmethode");

                if (missing.Any())
                {
                    var msg = "Bitte wählen Sie noch: " + string.Join(", ", missing);
                    Debug.WriteLine($"[CheckoutPage] Missing fields: {msg}");
                    await DisplayAlert("Fehler", msg, "OK");
                    return;
                }

                if (viewModel.TotalPrice == 0m)
                {
                    Debug.WriteLine("[CheckoutPage] TotalPrice == 0, processing order without payment");
                    await ProcessOrder(viewModel.SelectedTimeSlot);
                    return;
                }

                Debug.WriteLine($"[CheckoutPage] Processing payment: {viewModel.PaymentMethod}, amount={viewModel.TotalPrice}€");
                var ok = await _paymentService.ProcessPaymentAsync(
                    viewModel.PaymentMethod,
                    viewModel.TotalPrice,
                    this,
                    _orderId);

                if (!ok)
                {
                    Debug.WriteLine("[CheckoutPage] Payment failed");
                    await DisplayAlert("Zahlung fehlgeschlagen", "Bitte erneut versuchen.", "OK");
                    return;
                }

                ConfirmButton.IsEnabled = false;
                await Task.Delay(5000);
                ConfirmButton.IsEnabled = true;

                Debug.WriteLine("[CheckoutPage] Payment successful, calling ProcessOrder");
                await ProcessOrder(viewModel.SelectedTimeSlot);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckoutPage] ConfirmButtonClicked error: {ex}");
                await DisplayAlert("Fehler", "Ein unerwarteter Fehler ist aufgetreten. Bitte erneut versuchen.", "OK");
            }
        }

        async Task ProcessOrder(string slot)
        {
            try
            {
                if (_orderProcessed)
                {
                    Debug.WriteLine("[CheckoutPage] Order already processed, skipping");
                    return;
                }
                _orderProcessed = true;
                Debug.WriteLine("[CheckoutPage] ProcessOrder start");

                var ur = new UserRepository(DbBootstraper.Connection);
                var user = ur.GetCurrentUser();
                var name = $"{user.FirstName} {user.LastName}";
                var storeId = Preferences.Get("SelectedStoreId", string.Empty);

                var deliveryDate = viewModel.ShowDateTimeOptions
                    ? viewModel.SelectedDate.Date
                    : DateTime.Now.Date;
                var deliveryTime = viewModel.ShowDateTimeOptions
                    ? slot
                    : string.Empty;

                string addrLine, city, zip;
                if (viewModel.IsDelivery)
                {
                    addrLine = _selectedAddress.AddressLine1;
                    city = _selectedAddress.City;
                    zip = _selectedAddress.Zip;
                }
                else
                {
                    var store = viewModel.SelectedStore;
                    
                    city = store.City;
                    
                }

                var order = new Orders
                {
                    OrderID = _orderId,
                    UserID = user.UserId,
                    StoreID = storeId,
                    OrderDate = DateTime.Now,
                    DeliveryDate = deliveryDate,
                    DeliveryTime = deliveryTime,
                    DeliveryCity = city,
                    DeliveryCountry = "Austria",
                    DeliveryEmail = user.Email,
                    DeliveryContact = name,
                    DeliveryNotes = viewModel.OrderNote,
                    PaymentMethod = viewModel.PaymentMethod,
                    PaymentStatus = "Pending",
                    OrderStatus = "Processing",
                    TotalAmount = viewModel.TotalPrice,
                    TotalVAT = viewModel.VAT,
                    OrderType = App.OrderType,
                    Reference = string.Empty,
                    Tip = viewModel.Tip ?? 0m
                };

                Debug.WriteLine("[CheckoutPage] Saving order to local DB");
                await SQL.SaveItemAsync(order);

                var items = new List<OrderItems>();
                foreach (var it in Cart.Instance.Items)
                {
                    var oi = new OrderItems
                    {
                        OrderID = _orderId,
                        ProductID = it.ProductID,
                        ProductDescription1 = it.ProductDescription1,
                        ProductDescription2 = it.ProductDescription2,
                        Amount = it.Amount,
                        Quantity = it.Quantity,
                        VAT = it.VAT,
                        Reference = it.Reference,
                        ItemNote = it.ItemNote
                    };
                    await SQL.SaveItemAsync(oi);
                    items.Add(oi);
                }
                Debug.WriteLine($"[CheckoutPage] {items.Count} order items saved");

                Debug.WriteLine("[CheckoutPage] Sending order to server");
                var sent = await _orderCommunicationService.SendOrderToServerAsync(order, items);
                if (!sent)
                {
                    Debug.WriteLine("[CheckoutPage] SendOrderToServerAsync failed");
                    await DisplayAlert("Fehler", "Die Bestellung konnte nicht gesendet werden. Bitte erneut versuchen.", "OK");
                    _orderProcessed = false;
                    return;
                }

                // obrada poena
                int usedPoints = Preferences.Get("UsedPoints", 0);
                if (usedPoints > 0)
                {
                    Debug.WriteLine($"[CheckoutPage] Decreasing used points: {usedPoints}");
                    var resp = await _userPointsService.DecreasePointsFromUser(user.UserId, usedPoints);
                    if (resp.Contains("Points decreased"))
                        Preferences.Remove("UsedPoints");
                    else
                    {
                        Debug.WriteLine("[CheckoutPage] Failed to decrease points");
                        await DisplayAlert("Fehler", "Beim Abziehen der Punkte ist ein Fehler aufgetreten.", "OK");
                        _orderProcessed = false;
                        return;
                    }
                }

                Debug.WriteLine("[CheckoutPage] Clearing local cart");
                // ne čistimo ovde—čišćenje je u OrderDetailsPage
                // Cart.Instance.ClearCart();

                int pointsEarned = (int)Math.Floor(viewModel.TotalPrice);
                Debug.WriteLine($"[CheckoutPage] Updating user points after purchase: earned={pointsEarned}");
                var upd = await _userPointsService.UpdateUserPointsAfterPurchase(pointsEarned, storeId, user.UserId);
                if (pointsEarned > 0 && upd.ToString().Contains("User points updated successfully"))
                {
                    Debug.WriteLine("[CheckoutPage] Points updated successfully");
                    await DisplayAlert("Glückwunsch!", $"Sie haben {pointsEarned} Punkte erhalten.", "OK");
                }

                var vmOrder = new OrderWithItemsViewModel
                {
                    Order = order,
                    OrderItems = items
                };

                Debug.WriteLine("[CheckoutPage] Navigating to OrderDetailsPage");
                var detailsPage = new OrderDetailsPage(vmOrder);
                await Navigation.PushAsync(detailsPage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckoutPage] ProcessOrder error: {ex}");
                await DisplayAlert("Fehler", "Ein Fehler beim Verarbeiten der Bestellung ist aufgetreten. Bitte erneut versuchen.", "OK");
                _orderProcessed = false;
            }
        }
    }
}
