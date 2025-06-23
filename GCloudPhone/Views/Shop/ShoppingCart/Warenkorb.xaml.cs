using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Maui.Core;
using GCloudPhone.Services;
using GCloudShared.Interface;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Shared;
using CommunityToolkit.Maui.Alerts;
using System.Linq;
using System.Collections.Generic;
using System;
using GCloudPhone.Views.Shop.OrderProccess;
using GCloudPhone.Views.Shop.Checkout;

namespace GCloudPhone.Views.Shop.ShoppingCart
{
    public partial class Warenkorb : ContentPage, INotifyPropertyChanged
    {
        private decimal _totalPrice;
        private decimal? _totalVat;
        public new event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<OrderItemViewModel> CartItems { get; set; }
        public ObservableCollection<ProductsView> ProductsCollection { get; set; }

        public decimal TotalPrice
        {
            get => _totalPrice;
            set
            {
                _totalPrice = value;
                OnPropertyChanged();
            }
        }
        public decimal? TotalVat
        {
            get => _totalVat;
            set
            {
                _totalVat = value;
                OnPropertyChanged();
            }
        }

        private string _orderNote;
        public string OrderNote
        {
            get => _orderNote;
            set
            {
                if (_orderNote != value)
                {
                    _orderNote = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand DeleteItemCommand { get; }
        public ICommand ToggleRecommendationSelectionCommand { get; }

        private IAuthService _authService;
        private UserPointsService _userPointsService;

        public Warenkorb()
        {
            InitializeComponent();
            _authService = new AuthService();
            _userPointsService = new UserPointsService();
            LoadCartItems();

            ProductsCollection = new ObservableCollection<ProductsView>();
            RecommendationsCollectionView.ItemsSource = ProductsCollection;

            IncreaseQuantityCommand = new Command<OrderItemViewModel>(IncreaseQuantity);
            DecreaseQuantityCommand = new Command<OrderItemViewModel>(DecreaseQuantity);
            DeleteItemCommand = new Command<OrderItemViewModel>(DeleteItem);
            ToggleRecommendationSelectionCommand = new Command<ProductsView>(ToggleRecommendationSelection);

            LoadProductsAsync();
        }

        private async void LoadCartItems()
        {
            var cartItems = Cart.Instance.Items;

            if (cartItems == null || !cartItems.Any())
            {
                await DisplayAlert("Hinweis", "Es gibt keine Artikel im Warenkorb.", "OK");
                return;
            }

            var cartItemViewModels = new List<OrderItemViewModel>();
            bool showImages = Config.ProductPictureInBasket == "Yes";

            foreach (var item in cartItems)
            {
                // Debugging: Check each item
                Console.WriteLine($"Item: {item.ProductID}, Reference: {item.Reference}");

                if (!string.IsNullOrEmpty(item.Reference))
                    continue;

                var product = await SQL.GetItemAsync<Products>(p => p.Number == item.ProductID);

                ImageSource imageSource = null;

                if (showImages)
                {
                    if (product != null && product.Pictures.HasValue)
                    {
                        var picture = await SQL.GetItemAsync<Pictures>(p => p.Number == product.Pictures.Value);
                        if (picture != null && !string.IsNullOrEmpty(picture.Picturestring))
                        {
                            byte[] imageBytes = Convert.FromBase64String(picture.Picturestring);
                            imageSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                        }
                        else
                        {
                            imageSource = ImageSource.FromFile("food.png");
                        }
                    }
                    else
                    {
                        imageSource = ImageSource.FromFile("food.png");
                    }
                }

                bool isCoupon = false;
                decimal couponValue = 0;

                var coupon = await SQL.GetItemAsync<Coupons>(c => c.ArticleNumber == item.ProductID);
                if (coupon != null)
                {
                    isCoupon = true;
                    couponValue = coupon.Value;
                }

                var mainItemViewModel = new OrderItemViewModel
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

                // Find and add side dishes
                var sideDishes = cartItems.Where(sd => sd.Reference == item.Idc.ToString());
                foreach (var sideDish in sideDishes)
                {
                    var sideDishImageSource = showImages ? ImageSource.FromFile("food.png") : null;

                    var sideDishViewModel = new OrderItemViewModel
                    {
                        Idc = sideDish.Idc,
                        OrderID = sideDish.OrderID,
                        ProductID = sideDish.ProductID,
                        ProductDescription1 = sideDish.ProductDescription1?.Trim(),
                        ProductDescription2 = sideDish.ProductDescription2,
                        Amount = sideDish.Amount,
                        Quantity = sideDish.Quantity,
                        VAT = sideDish.VAT,
                        Reference = sideDish.Reference,
                        ImageSource = sideDishImageSource,
                        ShowImage = showImages,
                        IsCoupon = false,
                        CouponValue = 0
                    };

                    mainItemViewModel.SideDishes.Add(sideDishViewModel);
                }

                cartItemViewModels.Add(mainItemViewModel);
            }

            Console.WriteLine($"Cart items count: {cartItemViewModels.Count}");

            CartItems = new ObservableCollection<OrderItemViewModel>(cartItemViewModels);
            CartItems.CollectionChanged += CartItems_CollectionChanged;
            CartCollectionView.ItemsSource = CartItems;
            BindingContext = this;
            UpdateTotalPrice();
        }

        private void CartItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateTotalPrice();
        }

        private void UpdateTotalPrice()
        {
            decimal totalPrice = 0;

            foreach (var item in CartItems)
            {
                totalPrice += (item.Amount ?? 0) * (item.Quantity ?? 0);

                foreach (var sideDish in item.SideDishes)
                {
                    totalPrice += (sideDish.Amount ?? 0) * (sideDish.Quantity ?? 0);
                }
            }

            TotalPrice = totalPrice;
            UpdateTotalVAT();
        }

        private void UpdateTotalVAT()
        {
            decimal? totalVat = 0;

            foreach (var item in CartItems)
            {
                totalVat += (item.Amount ?? 0) * (item.Quantity ?? 0) * (item.VAT ?? 0) / (100 + (item.VAT ?? 0));

                foreach (var sideDish in item.SideDishes)
                {
                    totalVat += (sideDish.Amount ?? 0) * (sideDish.Quantity ?? 0) * (sideDish.VAT ?? 0) / (100 + (sideDish.VAT ?? 0));
                }
            }

            TotalVat = totalVat;
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            string result = await DisplayPromptAsync(
                     "Zusätzliche Informationen",
                     "Geben Sie zusätzliche Informationen oder Wünsche für Ihre Bestellung ein:",
                     "OK",
                     "Abbrechen",
                     placeholder: "Ihre Nachricht",
                     maxLength: 250,
                     keyboard: Keyboard.Text);

            if (!string.IsNullOrEmpty(result))
            {
                OrderNote = result;
            }
        }

        private async void IncreaseQuantity(OrderItemViewModel item)
        {
            if (!item.Quantity.HasValue)
                return;

            if (item.IsCoupon)
            {
                UserRepository ur = new UserRepository(DbBootstraper.Connection);
                var user = ur.GetCurrentUser();

                string pointsString = await _authService.GetTotalPointsByUserID(user.UserId);
                if (!string.IsNullOrEmpty(pointsString))
                {
                    pointsString = pointsString.Trim().Replace("\"", "");
                }

                int availablePoints = int.TryParse(pointsString, out int parsedPoints) ? parsedPoints : 0;
                int usedPoints = Preferences.Get("UsedPoints", 0);
                int pointsToDeduct = (int)item.CouponValue;
                int totalPointsRequired = usedPoints + pointsToDeduct;

                if (availablePoints >= totalPointsRequired)
                {
                    item.Quantity++;
                    usedPoints += pointsToDeduct;
                    Preferences.Set("UsedPoints", usedPoints);

                    Cart.Instance.UpdateItemQuantity(item.ProductID, item.Quantity.Value);
                    UpdateTotalPrice();

                    int remainingPoints = availablePoints - usedPoints;

                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                    string toastMessage = $"Verbleibende Punkte: {remainingPoints}";
                    ToastDuration duration = ToastDuration.Short;
                    double fontSize = 14;
                    var toast = Toast.Make(toastMessage, duration, fontSize);
                    toast.Show(cancellationTokenSource.Token);
                }
                else
                {
                    await DisplayAlert("Fehler", "Nicht genügend Punkte für diesen Coupon.", "OK");
                }
            }
            else
            {
                item.Quantity++;
                Cart.Instance.UpdateItemQuantity(item.ProductID, item.Quantity.Value);
                UpdateTotalPrice();
            }
            // Pošalji poruku da se promenila količina (za update na CategoriesDetails)
            MessagingCenter.Send(this, "CartUpdated", item);
        }

        private async void DecreaseQuantity(OrderItemViewModel item)
        {
            if (item.Quantity <= 1)
            {
                DeleteItem(item);
                return;
            }

            if (item.IsCoupon)
            {
                UserRepository ur = new UserRepository(DbBootstraper.Connection);
                var user = ur.GetCurrentUser();

                string pointsString = await _authService.GetTotalPointsByUserID(user.UserId);

                if (!string.IsNullOrEmpty(pointsString))
                {
                    pointsString = pointsString.Trim().Replace("\"", "");
                }

                int availablePoints = int.TryParse(pointsString, out int userPoints) ? userPoints : 0;
                int usedPoints = Preferences.Get("UsedPoints", 0);
                int pointsToRefund = (int)item.CouponValue;

                usedPoints -= pointsToRefund;
                if (usedPoints < 0) usedPoints = 0;
                Preferences.Set("UsedPoints", usedPoints);

                item.Quantity--;
                Cart.Instance.UpdateItemQuantity(item.ProductID, item.Quantity.Value);
                UpdateTotalPrice();

                int updatedAvailablePoints = availablePoints - usedPoints;

                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                string toastMessage = $"Verfügbare Punkte: {updatedAvailablePoints}";
                ToastDuration duration = ToastDuration.Short;
                double fontSize = 14;
                var toast = Toast.Make(toastMessage, duration, fontSize);
                await toast.Show(cancellationTokenSource.Token);
            }
            else
            {
                item.Quantity--;
                Cart.Instance.UpdateItemQuantity(item.ProductID, item.Quantity.Value);
                UpdateTotalPrice();
            }
            // Pošalji poruku da se promenila količina
            MessagingCenter.Send(this, "CartUpdated", item);
        }

        private void DeleteItem(OrderItemViewModel itemViewModel)
        {
            if (itemViewModel.IsCoupon)
            {
                int usedPoints = Preferences.Get("UsedPoints", 0);
                usedPoints -= (int)itemViewModel.CouponValue * (itemViewModel.Quantity ?? 1);
                if (usedPoints < 0) usedPoints = 0;
                Preferences.Set("UsedPoints", usedPoints);
            }

            var itemToDelete = new OrderItemViewModel
            {
                Idc = itemViewModel.Idc,
                OrderID = itemViewModel.OrderID,
                ProductID = itemViewModel.ProductID,
                ProductDescription1 = itemViewModel.ProductDescription1,
                ProductDescription2 = itemViewModel.ProductDescription2,
                Amount = itemViewModel.Amount,
                Quantity = itemViewModel.Quantity,
                VAT = itemViewModel.VAT,
                Reference = itemViewModel.Reference,
            };

            if (string.IsNullOrEmpty(itemToDelete.Reference))
            {
                var relatedSideDishes = Cart.Instance.Items
                    .Where(i => i.Reference == itemToDelete.Idc.ToString())
                    .ToList();

                foreach (var sideDish in relatedSideDishes)
                {
                    Cart.Instance.RemoveItem(sideDish);
                    var sideDishViewModel = CartItems.FirstOrDefault(vm => vm.Idc == sideDish.Idc);
                    if (sideDishViewModel != null)
                    {
                        CartItems.Remove(sideDishViewModel);
                    }
                }
            }

            Cart.Instance.RemoveItem(itemToDelete);
            CartItems.Remove(itemViewModel);
            LoadCartItems();
            UpdateTotalPrice();

            // Pošalji poruku da je artikal obrisan (opciono)
            MessagingCenter.Send(this, "CartUpdated", itemViewModel);
        }

        private async void ContinueToCheckoutClicked(object sender, EventArgs e)
        {
            switch (App.OrderType)
            {
                case "DineIn":
                    await Navigation.PushAsync(new InStoreCheckout(TotalPrice, TotalVat, CartItems, OrderNote));
                    break;
                case "Parking":
                    await Navigation.PushAsync(new InStoreCheckout(TotalPrice, TotalVat, CartItems, OrderNote));
                    break;
                case "Delivery":
                    await Navigation.PushAsync(new DeliveryCheckout(TotalPrice, TotalVat, CartItems, OrderNote));
                    break;
                case "PickUp":
                    await Navigation.PushAsync(new PickupCheckout(TotalPrice, TotalVat, CartItems, OrderNote));
                    break;
            }
        }

        private async void LoadProductsAsync()
        {
            try
            {
                var cartItems = Cart.Instance.Items;

                if (cartItems == null || !cartItems.Any())
                {
                    await DisplayAlert("Hinweis", "Keine Artikel im Warenkorb.", "OK");
                    return;
                }

                var productIdsInCart = cartItems.Select(item => item.ProductID).ToList();

                if (productIdsInCart.Count > 0)
                {
                    var recommendedProducts = await SQL.GetRecommendationsAsync(productIdsInCart);

                    ProductsCollection.Clear();
                    foreach (var product in recommendedProducts)
                    {
                        ImageSource imageSource;

                        if (!string.IsNullOrEmpty(product.Picturestring))
                        {
                            byte[] imageBytes = Convert.FromBase64String(product.Picturestring);
                            imageSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                        }
                        else
                        {
                            imageSource = ImageSource.FromFile("food.png");
                        }

                        ProductsCollection.Add(new ProductsView
                        {
                            Number = product.Number,
                            Description1 = product.Description1.Trim(),
                            Description2 = product.Description2,
                            Categories = product.Categories,
                            Picturestring = product.Picturestring,
                            PriceAmount = product.PriceAmount,
                            Image = imageSource,
                            VAT = product.VAT
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fehler", "Failed to load product recommendations. " + ex.Message, "OK");
            }
        }

        private void ToggleRecommendationSelection(ProductsView recommendation)
        {
            var existingItem = CartItems.FirstOrDefault(i => i.ProductID == recommendation.Number && i.ProductDescription2 == "Recommended item");

            if (existingItem != null)
            {
                existingItem.Quantity++;
                Cart.Instance.UpdateItemQuantity(existingItem.ProductID, existingItem.Quantity.Value);
            }
            else
            {
                var newOrderItem = new OrderItemViewModel
                {
                    ProductID = recommendation.Number,
                    ProductDescription1 = recommendation.Description1.Trim(),
                    ProductDescription2 = "Recommended item",
                    Amount = recommendation.PriceAmount,
                    Quantity = 1,
                    VAT = recommendation.VAT,
                };

                Cart.Instance.AddItem(newOrderItem);
            }

            LoadCartItems();
            UpdateTotalPrice();

            // Pošalji poruku da su se preporučeni artikli ažurirali
            MessagingCenter.Send(this, "CartUpdated", recommendation);
        }

        protected new void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void imgHome_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MainPage());
        }

        private async void CancelOrder_Tapped(object sender, TappedEventArgs e)
        {
            bool confirmCancel = await DisplayAlert("Bestellung stornieren", "Sind Sie sicher, dass Sie Ihre Bestellung stornieren möchten?", "Ja", "Nein");

            if (confirmCancel)
            {
                Cart.Instance.ClearCart();
                await Navigation.PushAsync(new CategoriesPage());
            }
        }
    }
}
