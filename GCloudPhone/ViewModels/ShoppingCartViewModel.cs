// ShoppingCartViewModel.cs
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using GCloudPhone.Services;
using GCloudShared.Interface;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Shared;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GCloudPhone.ViewModels
{
    public class ShoppingCartViewModel : INotifyPropertyChanged
    {
        private decimal _totalPrice;
        private decimal? _totalVat;
        private string _orderNote;

        private readonly IAuthService _authService;
        private readonly UserPointsService _userPointsService;

        public ObservableCollection<OrderItemViewModel> CartItems { get; }
        public ObservableCollection<ProductsView> ProductsCollection { get; }

        public decimal TotalPrice
        {
            get => _totalPrice;
            set { _totalPrice = value; OnPropertyChanged(); }
        }

        public decimal? TotalVat
        {
            get => _totalVat;
            set { _totalVat = value; OnPropertyChanged(); }
        }

        public string OrderNote
        {
            get => _orderNote;
            set { if (_orderNote != value) { _orderNote = value; OnPropertyChanged(); } }
        }

        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand DeleteItemCommand { get; }
        public ICommand ToggleRecommendationSelectionCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ShoppingCartViewModel()
        {
            _authService = new AuthService();
            _userPointsService = new UserPointsService();

            CartItems = new ObservableCollection<OrderItemViewModel>();
            ProductsCollection = new ObservableCollection<ProductsView>();

            IncreaseQuantityCommand = new Command<OrderItemViewModel>(async item => await IncreaseQuantityAsync(item));
            DecreaseQuantityCommand = new Command<OrderItemViewModel>(async item => await DecreaseQuantityAsync(item));
            DeleteItemCommand = new Command<OrderItemViewModel>(item => DeleteItem(item));
            ToggleRecommendationSelectionCommand = new Command<ProductsView>(rec => ToggleRecommendationSelection(rec));

            CartItems.CollectionChanged += (s, e) => UpdateTotalPrice();
        }

        public async Task LoadCartItemsAsync()
        {
            try
            {
                if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                        await Application.Current.MainPage.DisplayAlert(
                            "Keine Verbindung",
                            "Es ist keine Internetverbindung verfügbar.",
                            "OK"));
                    return;
                }

                var cart = Cart.Instance.Items;
                if (cart == null || !cart.Any())
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                        await Application.Current.MainPage.DisplayAlert(
                            "Hinweis",
                            "Es gibt keine Artikel im Warenkorb.",
                            "OK"));
                    return;
                }

                var list = new List<OrderItemViewModel>();
                bool showImages = Config.ProductPictureInBasket == "Yes";

                foreach (var item in cart.Where(i => string.IsNullOrEmpty(i.Reference)))
                {
                    var product = await SQL.GetItemAsync<Products>(p => p.Number == item.ProductID);
                    ImageSource imageSource = null;
                    if (showImages)
                    {
                        if (product?.Pictures.HasValue == true)
                        {
                            var pic = await SQL.GetItemAsync<Pictures>(p => p.Number == product.Pictures.Value);
                            if (!string.IsNullOrEmpty(pic?.Picturestring))
                            {
                                var bytes = Convert.FromBase64String(pic.Picturestring);
                                imageSource = ImageSource.FromStream(() => new MemoryStream(bytes));
                            }
                            else imageSource = ImageSource.FromFile("food.png");
                        }
                        else imageSource = ImageSource.FromFile("food.png");
                    }

                    bool isCoupon = false;
                    decimal couponValue = 0;
                    var coupon = await SQL.GetItemAsync<Coupons>(c => c.ArticleNumber == item.ProductID);
                    if (coupon != null)
                    {
                        isCoupon = true;
                        couponValue = coupon.Value;
                    }

                    var vm = new OrderItemViewModel
                    {
                        Idc = item.Idc,
                        OrderID = item.OrderID,
                        ProductID = item.ProductID,
                        ProductDescription1 = item.ProductDescription1?.Trim(),
                        ProductDescription2 = item.ProductDescription2,
                        Amount = item.Amount,
                        Quantity = item.Quantity,
                        VAT = item.VAT,
                        Reference = item.Reference,
                        ImageSource = imageSource,
                        ShowImage = showImages,
                        IsCoupon = isCoupon,
                        CouponValue = couponValue,
                        SideDishes = new ObservableCollection<OrderItemViewModel>()
                    };

                    var sides = cart.Where(sd => sd.Reference == item.Idc.ToString());
                    foreach (var sd in sides)
                    {
                        var sideImage = showImages ? ImageSource.FromFile("food.png") : null;
                        vm.SideDishes.Add(new OrderItemViewModel
                        {
                            Idc = sd.Idc,
                            OrderID = sd.OrderID,
                            ProductID = sd.ProductID,
                            ProductDescription1 = sd.ProductDescription1?.Trim(),
                            ProductDescription2 = sd.ProductDescription2,
                            Amount = sd.Amount,
                            Quantity = sd.Quantity,
                            VAT = sd.VAT,
                            Reference = sd.Reference,
                            ImageSource = sideImage,
                            ShowImage = showImages,
                            IsCoupon = false,
                            CouponValue = 0
                        });
                    }

                    list.Add(vm);
                }

                CartItems.Clear();
                foreach (var x in list) CartItems.Add(x);
                UpdateTotalPrice();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ShoppingCartViewModel] Greška pri učitavanju korpe: {ex}");
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await Application.Current.MainPage.DisplayAlert(
                        "Fehler",
                        $"Fehler beim Laden des Warenkorbs:\n{ex.Message}",
                        "OK"));
            }
        }

        public async Task LoadProductsAsync()
        {
            try
            {
                var cart = Cart.Instance.Items;
                if (cart == null || !cart.Any())
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                        await Application.Current.MainPage.DisplayAlert(
                            "Hinweis",
                            "Keine Artikel im Warenkorb.",
                            "OK"));
                    return;
                }

                var ids = cart.Select(i => i.ProductID).ToList();
                if (ids.Count > 0)
                {
                    var recs = await SQL.GetRecommendationsAsync(ids);
                    ProductsCollection.Clear();
                    foreach (var p in recs)
                    {
                        ImageSource img = !string.IsNullOrEmpty(p.Picturestring)
                            ? ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(p.Picturestring)))
                            : ImageSource.FromFile("food.png");

                        ProductsCollection.Add(new ProductsView
                        {
                            Number = p.Number,
                            Description1 = p.Description1.Trim(),
                            Description2 = p.Description2,
                            Categories = p.Categories,
                            Picturestring = p.Picturestring,
                            PriceAmount = p.PriceAmount,
                            VAT = p.VAT,
                            Image = img
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ShoppingCartViewModel] Greška pri učitavanju preporuka: {ex}");
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await Application.Current.MainPage.DisplayAlert(
                        "Fehler",
                        $"Failed to load product recommendations:\n{ex.Message}",
                        "OK"));
            }
        }

        private void UpdateTotalPrice()
        {
            decimal sum = 0;
            foreach (var item in CartItems)
            {
                sum += (item.Amount ?? 0) * (item.Quantity ?? 0);
                foreach (var sd in item.SideDishes)
                    sum += (sd.Amount ?? 0) * (sd.Quantity ?? 0);
            }
            TotalPrice = sum;
            UpdateTotalVat();
        }

        private void UpdateTotalVat()
        {
            decimal? vatSum = 0;
            foreach (var item in CartItems)
            {
                vatSum += (item.Amount ?? 0) * (item.Quantity ?? 0) * (item.VAT ?? 0) / (100 + (item.VAT ?? 0));
                foreach (var sd in item.SideDishes)
                    vatSum += (sd.Amount ?? 0) * (sd.Quantity ?? 0) * (sd.VAT ?? 0) / (100 + (sd.VAT ?? 0));
            }
            TotalVat = vatSum;
        }

        private async Task IncreaseQuantityAsync(OrderItemViewModel item)
        {
            if (!item.Quantity.HasValue) return;

            if (item.IsCoupon)
            {
                var ur = new UserRepository(DbBootstraper.Connection);
                var user = ur.GetCurrentUser();
                string pts = await _authService.GetTotalPointsByUserID(user.UserId);
                if (!string.IsNullOrEmpty(pts)) pts = pts.Trim().Replace("\"", "");
                int avail = int.TryParse(pts, out var p) ? p : 0;
                int used = Preferences.Get("UsedPoints", 0);
                int need = (int)item.CouponValue;
                if (avail >= used + need)
                {
                    item.Quantity++;
                    used += need;
                    Preferences.Set("UsedPoints", used);

                    Cart.Instance.UpdateItemQuantity(item.ProductID, item.Quantity.Value);
                    UpdateTotalPrice();
                    int rem = avail - used;
                    var toast = Toast.Make($"Verbleibende Punkte: {rem}", ToastDuration.Short, 14);
                    toast.Show();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Fehler", "Nicht genügend Punkte für diesen Coupon.", "OK");
                }
            }
            else
            {
                item.Quantity++;
                Cart.Instance.UpdateItemQuantity(item.ProductID, item.Quantity.Value);
                UpdateTotalPrice();
            }

            MessagingCenter.Send(this, "CartUpdated", item);
        }

        private async Task DecreaseQuantityAsync(OrderItemViewModel item)
        {
            if (item.Quantity <= 1)
            {
                DeleteItem(item);
                return;
            }

            if (item.IsCoupon)
            {
                var ur = new UserRepository(DbBootstraper.Connection);
                var user = ur.GetCurrentUser();
                string pts = await _authService.GetTotalPointsByUserID(user.UserId);
                if (!string.IsNullOrEmpty(pts)) pts = pts.Trim().Replace("\"", "");
                int avail = int.TryParse(pts, out var p) ? p : 0;
                int used = Preferences.Get("UsedPoints", 0);
                used -= (int)item.CouponValue;
                if (used < 0) used = 0;
                Preferences.Set("UsedPoints", used);

                item.Quantity--;
                Cart.Instance.UpdateItemQuantity(item.ProductID, item.Quantity.Value);
                UpdateTotalPrice();

                int rem = avail - used;
                var toast = Toast.Make($"Verfügbare Punkte: {rem}", ToastDuration.Short, 14);
                toast.Show();
            }
            else
            {
                item.Quantity--;
                Cart.Instance.UpdateItemQuantity(item.ProductID, item.Quantity.Value);
                UpdateTotalPrice();
            }

            MessagingCenter.Send(this, "CartUpdated", item);
        }

        private void DeleteItem(OrderItemViewModel item)
        {
            if (item.IsCoupon)
            {
                int used = Preferences.Get("UsedPoints", 0);
                used -= (int)item.CouponValue * (item.Quantity ?? 1);
                if (used < 0) used = 0;
                Preferences.Set("UsedPoints", used);
            }

            var toDelete = new OrderItemViewModel
            {
                Idc = item.Idc,
                OrderID = item.OrderID,
                ProductID = item.ProductID,
                ProductDescription1 = item.ProductDescription1,
                ProductDescription2 = item.ProductDescription2,
                Amount = item.Amount,
                Quantity = item.Quantity,
                VAT = item.VAT,
                Reference = item.Reference
            };

            if (string.IsNullOrEmpty(toDelete.Reference))
            {
                var sides = Cart.Instance.Items.Where(i => i.Reference == toDelete.Idc.ToString()).ToList();
                foreach (var sd in sides)
                {
                    Cart.Instance.RemoveItem(sd);
                    var sdVm = CartItems.FirstOrDefault(vm => vm.Idc == sd.Idc);
                    if (sdVm != null)
                        CartItems.Remove(sdVm);
                }
            }

            Cart.Instance.RemoveItem(toDelete);
            CartItems.Remove(item);

            _ = LoadCartItemsAsync();
            UpdateTotalPrice();

            MessagingCenter.Send(this, "CartUpdated", item);
        }

        private void ToggleRecommendationSelection(ProductsView recommendation)
        {
            var existing = CartItems.FirstOrDefault(i =>
                i.ProductID == recommendation.Number &&
                i.ProductDescription2 == "Recommended item");

            if (existing != null)
            {
                existing.Quantity++;
                Cart.Instance.UpdateItemQuantity(existing.ProductID, existing.Quantity.Value);
            }
            else
            {
                Cart.Instance.AddItem(new OrderItemViewModel
                {
                    ProductID = recommendation.Number,
                    ProductDescription1 = recommendation.Description1.Trim(),
                    ProductDescription2 = "Recommended item",
                    Amount = recommendation.PriceAmount,
                    Quantity = 1,
                    VAT = recommendation.VAT
                });
            }

            _ = LoadCartItemsAsync();
            UpdateTotalPrice();

            MessagingCenter.Send(this, "CartUpdated", recommendation);
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
