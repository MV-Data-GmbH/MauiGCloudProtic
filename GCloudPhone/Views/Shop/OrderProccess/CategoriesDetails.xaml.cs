using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using GCloud.Shared.Dto.Domain;
using GCloudPhone.Views.Shop.ShoppingCart;
using GCloudShared.Interface;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Shared;
using SkiaSharp;

namespace GCloudPhone.Views.Shop.OrderProccess
{
    public partial class CategoriesDetails : ContentPage
    {
        public ObservableCollection<ProductsView> ProductsCollection { get; set; }
        public ObservableCollection<CategoriesView> CategoriesCollection { get; set; }
        private int _categoryNumber;
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }

        private IAuthService _authService;
        private UserPointsService _userPointsService;

        private int _usedPoints
        {
            get => Preferences.Get("UsedPoints", 0);
            set => Preferences.Set("UsedPoints", value);
        }

        // Polja za Multiplikations-Popup
        private TaskCompletionSource<int> _multiplicationTcs;
        private int _popupQuantity = 1;

        public CategoriesDetails(int categoryNumber)
        {
            InitializeComponent();
            _categoryNumber = categoryNumber;
            ProductsCollection = new ObservableCollection<ProductsView>();
            _authService = new AuthService();
            _userPointsService = new UserPointsService();
            ProductsCollectionView.ItemsSource = ProductsCollection;

            IncreaseQuantityCommand = new Command<ProductsView>(IncreaseQuantity);
            DecreaseQuantityCommand = new Command<ProductsView>(DecreaseQuantity);
            BindingContext = this;

            if (Config.CategoryDisplay == "Compact")
            {
               
                CategoriesCollection = new ObservableCollection<CategoriesView>();
               
                if (_categoryNumber == 9999)
                {
                    LoadCategoriesAsync(true);
                }
                else
                {
                    LoadCategoriesAsync(false);
                }
            }
            else
            {
               
                LoadProductsAsync();
            }
        }

        // Osveži stanje kada se stranica ponovo pojavi – quantity controls se ne prikazuju
        protected override void OnAppearing()
        {
            base.OnAppearing();
            MessagingCenter.Subscribe<GCloudPhone.Views.Shop.ShoppingCart.ShoppingCart, OrderItemViewModel>(this, "CartUpdated", (sender, updatedItem) =>
            {
                var product = ProductsCollection.FirstOrDefault(p => p.Number == updatedItem.ProductID);
                if (product != null)
                {
                    product.Quantity = updatedItem.Quantity ?? 0;
                }
            });
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<GCloudPhone.Views.Shop.ShoppingCart.ShoppingCart, OrderItemViewModel>(this, "CartUpdated");
        }

        private async void LoadProductsAsync()
        {
            try
            {
                var products = await SQL.GetProductsByCategoryIdAsync(_categoryNumber);
                ProductsCollection.Clear();
                foreach (var product in products)
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
                        Description1 = product.Description1,
                        PriceAmount = product.PriceAmount,
                        Categories = product.Categories,
                        Picturestring = product.Picturestring,
                        Image = imageSource,
                        VAT = product.VAT,
                        // Inicijalno se ne prikazuju quantity controls
                        ShowQuantityControls = false,
                        Quantity = 0
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadProductsAsync error: {ex.Message}");
                await DisplayAlert("Fehler", "Failed to load products. Please try again later.", "OK");
            }
        }

        private async void LoadCouponProductsAsync()
        {
            try
            {
                ProductsCollection.Clear();
                if (StartUpDataImport.coupons == null || !StartUpDataImport.coupons.Any())
                {
                    await DisplayAlert("Fehler", "Keine Coupons verfügbar.", "OK");
                    return;
                }
                UserRepository ur = new UserRepository(DbBootstraper.Connection);
                var user = ur.GetCurrentUser();
                string pointsString = await _authService.GetTotalPointsByUserID(user.UserId);
                pointsString = pointsString.Replace("\"", "");
                int pointsValue = int.TryParse(pointsString, out int p) ? p : 0;

                foreach (var product in StartUpDataImport.coupons)
                {
                    if (product.CouponType == 4 && product.Value < pointsValue)
                    {
                        ImageSource imageSource;
                        if (!string.IsNullOrEmpty(product.IconBase64))
                        {
                            byte[] imageBytes = Convert.FromBase64String(product.IconBase64);
                            imageSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                        }
                        else
                        {
                            imageSource = ImageSource.FromFile("food.png");
                        }
                        ProductsCollection.Add(new ProductsView
                        {
                            Number = product.ArticleNumber ?? 0,
                            Description1 = product.Name,
                            PriceAmount = 0,
                            Categories = 9999,
                            Picturestring = product.IconBase64,
                            Image = imageSource,
                            VAT = 0,
                            IsCoupon = true,
                            CouponValue = Math.Floor(product.Value),
                            ShowQuantityControls = false,
                            Quantity = 0
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadCouponProductsAsync error: {ex.Message}");
                await DisplayAlert("Fehler", "Failed to load coupons. Please try again later.", "OK");
            }
        }

        private async void LoadCategoriesAsync(bool isCoupon)
        {
            try
            {
                var categories = await SQL.GetAllCategoriesAsync();
                foreach (var category in categories)
                {
                    ImageSource imageSource;
                    if (!string.IsNullOrEmpty(category.Picturestring))
                    {
                        byte[] imageBytes = Convert.FromBase64String(category.Picturestring);
                        imageSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                    }
                    else
                    {
                        imageSource = ImageSource.FromFile("food.png");
                    }
                    CategoriesCollection.Add(new CategoriesView
                    {
                        Number = category.Number,
                        Description1 = category.Description1.Trim(),
                        Image = imageSource,
                        IsSelected = false
                    });
                }

                if (isCoupon)
                {
                    LoadCouponProductsAsync();
                }
                else
                {
                    var selectedCategory = CategoriesCollection.FirstOrDefault(c => c.Number == _categoryNumber);
                    if (selectedCategory != null)
                    {
                        selectedCategory.IsSelected = true;
                      
                        var index = CategoriesCollection.IndexOf(selectedCategory);
                        if (index >= 0)
                        {
                            await Task.Delay(100);
                         
                            LoadProductsAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadCategoriesAsync error: {ex.Message}");
                await DisplayAlert("Error", "Failed to load categories. Please try again later.", "OK");
            }
        }

        private void IncreaseQuantity(ProductsView product)
        {
            product.Quantity++;

            var existingItem = Cart.Instance.Items.FirstOrDefault(i => i.ProductID == product.Number && !i.IsCoupon);
            if (existingItem != null)
            {
                existingItem.Quantity = product.Quantity;
                Cart.Instance.UpdateItemQuantity(existingItem.ProductID, existingItem.Quantity.Value);
            }
            else
            {
                var orderItem = new OrderItemViewModel
                {
                    ProductID = product.Number,
                    ProductDescription1 = product.Description1,
                    Amount = product.PriceAmount,
                    Quantity = product.Quantity,
                    VAT = 0,
                    IsCoupon = false,
                    CouponValue = 0
                };
                Cart.Instance.AddItem(orderItem);
            }

            if (Config.ShowPopupForSelect == "Yes")
            {
                ShowNotification(product.Description1, $" ({product.Quantity}x im Warenkorb)");
            }
        }

        private void DecreaseQuantity(ProductsView product)
        {
            if (product.Quantity > 1)
            {
                product.Quantity--;

                var existingItem = Cart.Instance.Items.FirstOrDefault(i => i.ProductID == product.Number && !i.IsCoupon);
                if (existingItem != null)
                {
                    existingItem.Quantity = product.Quantity;
                    Cart.Instance.UpdateItemQuantity(existingItem.ProductID, existingItem.Quantity.Value);
                }
            }
            else if (product.Quantity == 1) // Ako se briše poslednja količina
            {
                product.Quantity = 0;

                var existingItem = Cart.Instance.Items.FirstOrDefault(i => i.ProductID == product.Number && !i.IsCoupon);
                if (existingItem != null)
                {
                    Cart.Instance.RemoveItem(existingItem);
                }
            }

            if (Config.ShowPopupForSelect == "Yes")
            {
                ShowNotification(product.Description1, " aus dem Warenkorb entfernt");
            }
        }

        private async void OnSwipedRight(object sender, SwipedEventArgs e)
        {
            Navigation.PopAsync();
        }

        private async void OnItemTapped(object sender, EventArgs e)
        {
            // Ako je MultiplicationPopup vidljiv, ne reaguj
            if (MultiplicationPopupPanel.IsVisible)
                return;

            var grid = sender as Grid;
            var product = grid?.BindingContext as ProductsView;
            if (grid == null || product == null)
                return;

            // Animacija za vizuelni feedback
            Task.Run(async () =>
            {
                await grid.ColorTo(Colors.White, Colors.LightGreen, c => grid.BackgroundColor = c, 350);
                await Task.Delay(1000);
                await grid.ColorTo(Colors.LightGreen, Colors.White, c => grid.BackgroundColor = c, 350);
            });

            // Provera da li je kupon
            if (product.PriceAmount == 0 && product.IsCoupon)
            {
                UserRepository ur = new UserRepository(DbBootstraper.Connection);
                var user = ur.GetCurrentUser();
                string pointsString = await _authService.GetTotalPointsByUserID(user.UserId);
                pointsString = pointsString.Replace("\"", "");
                int userPoints = int.TryParse(pointsString, out int parsedPoints) ? parsedPoints : 0;
                int requiredPoints = (int)product.CouponValue;

                var existingCoupon = Cart.Instance.Items.FirstOrDefault(i => i.ProductID == product.Number && i.IsCoupon);
                if (userPoints - _usedPoints >= requiredPoints)
                {
                    if (existingCoupon != null)
                    {
                        existingCoupon.Quantity++;
                    }
                    else
                    {
                        var orderItem = new OrderItemViewModel
                        {
                            ProductID = product.Number,
                            ProductDescription1 = product.Description1,
                            ProductDescription2 = product.Description2,
                            Amount = 0,
                            Quantity = 1,
                            VAT = 0,
                            IsCoupon = true,
                            CouponValue = Math.Floor(product.CouponValue)
                        };
                        Cart.Instance.AddItem(orderItem);
                    }

                    _usedPoints += requiredPoints;
                    int updatedPoints = userPoints - _usedPoints;
                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                    string toastMessage = $"Aktuelle Punkte: {updatedPoints}";
                    ToastDuration duration = ToastDuration.Short;
                    double fontSize = 14;
                    var toast = Toast.Make(toastMessage, duration, fontSize);
                    await toast.Show(cancellationTokenSource.Token);

                    if (Config.ShowPopupForSelect == "Yes")
                    {
                        ShowNotification(product.Description1, " zum Warenkorb hinzugefügt");
                    }
                }
                else
                {
                    await DisplayAlert("Fehler", "Nicht genügend Punkte für diesen Coupon.", "OK");
                }
                return;
            }

            // Provera za SideDishes
            var sideDishes = await SQL.GetSidedishesByProductIdAsync(product.Number);
            if (sideDishes.Any())
            {
                await Navigation.PushAsync(new ProductDetails(product));
                return;
            }

            // Ako se koristi MultiplicationPopup
            if (Config.ShowPopupForMultiplikation == "Yes")
            {
                int selectedQuantity = await ShowMultiplicationPopup(product);
                if (selectedQuantity > 0)
                {
                    // Provera da li je proizvod već dodat
                    var existingItem = Cart.Instance.Items.FirstOrDefault(i => i.ProductID == product.Number && !i.IsCoupon);
                    if (existingItem != null)
                    {
                        // Dodajemo novu količinu na postojeću umesto prepisivanja
                        existingItem.Quantity = (existingItem.Quantity ?? 0) + selectedQuantity;
                        // Ovde eksplicitno konvertujemo int? u int
                        product.Quantity = existingItem.Quantity.Value;
                        Cart.Instance.UpdateItemQuantity(existingItem.ProductID, existingItem.Quantity.Value);
                    }
                    else
                    {
                        product.Quantity = selectedQuantity;
                        var orderItem = new OrderItemViewModel
                        {
                            ProductID = product.Number,
                            ProductDescription1 = product.Description1,
                            ProductDescription2 = product.Description2,
                            Amount = product.PriceAmount,
                            Quantity = product.Quantity,
                            VAT = product.VAT,
                            IsCoupon = false,
                            CouponValue = 0
                        };
                        Cart.Instance.AddItem(orderItem);
                    }
                    if (Config.ShowPopupForSelect == "Yes")
                    {
                        ShowNotification(product.Description1, " zum Warenkorb hinzugefügt");
                    }
                }
                return;
            }
            else
            {
                // Ako proizvod nije prethodno dodat, dodaj ga i postavi količinu
                if (product.Quantity == 0)
                {
                    product.Quantity = 1;
                    var orderItem = new OrderItemViewModel
                    {
                        ProductID = product.Number,
                        ProductDescription1 = product.Description1,
                        ProductDescription2 = product.Description2,
                        Amount = product.PriceAmount,
                        Quantity = 1,
                        VAT = product.VAT,
                        IsCoupon = false,
                        CouponValue = 0
                    };
                    Cart.Instance.AddItem(orderItem);
                }
                else
                {
                    // Ako je proizvod već dodat, samo povećaj količinu u košarici
                    var existingItem = Cart.Instance.Items.FirstOrDefault(i => i.ProductID == product.Number && !i.IsCoupon);
                    if (existingItem != null)
                    {
                        existingItem.Quantity = (existingItem.Quantity ?? 0) + 1;
                        Cart.Instance.UpdateItemQuantity(existingItem.ProductID, existingItem.Quantity.Value);
                    }
                }

                if (Config.ShowPopupForSelect == "Yes")
                {
                    ShowNotification(product.Description1, " zum Warenkorb hinzugefügt");
                }
            }
        }

        private async Task<int> ShowMultiplicationPopup(ProductsView product)
        {
            // Resetujemo input količinu na 1 – 
            // bez obzira da li je proizvod već u korpi, 
            // ako je količina menjana iz korpe, popup će početi od 1.
            _popupQuantity = 1;

            PopupProductImage.Source = product.Image;
            PopupProductDescription.Text = product.Description1;
            PopupQuantityLabel.Text = _popupQuantity.ToString();
            PopupOverlay.IsVisible = true;
            MultiplicationPopupPanel.IsVisible = true;

            _multiplicationTcs = new TaskCompletionSource<int>();
            int result = await _multiplicationTcs.Task;
            MultiplicationPopupPanel.IsVisible = false;
            PopupOverlay.IsVisible = false;
            return result;
        }
        private void OnPopupPlusClicked(object sender, EventArgs e)
        {
            _popupQuantity++;
            PopupQuantityLabel.Text = _popupQuantity.ToString();
        }

        private void OnPopupMinusClicked(object sender, EventArgs e)
        {
            if (_popupQuantity > 1)
            {
                _popupQuantity--;
                PopupQuantityLabel.Text = _popupQuantity.ToString();
            }
        }

        private void OnPopupOkClicked(object sender, EventArgs e)
        {
            _multiplicationTcs?.SetResult(_popupQuantity);
        }

        private void OnPopupCancelClicked(object sender, EventArgs e)
        {
            _multiplicationTcs?.SetResult(0);
        }

        private async void ShowNotification(string description, string message)
        {
            NotificationLabel.Text = description.Trim();
            NotificationLabel2.Text = message;
            NotificationPanel.IsVisible = true;
            await Task.Run(async () =>
            {
                await Task.Delay(2000);
                Dispatcher.Dispatch(() =>
                {
                    NotificationPanel.IsVisible = false;
                });
            });
        }

        private void OnCategoriesPositionChanged(object sender, PositionChangedEventArgs e)
        {
            try
            {
                var carouselView = sender as CarouselView;
                if (carouselView == null)
                    return;
                var previousIndex = e.PreviousPosition;
                var currentIndex = e.CurrentPosition;
                if (previousIndex >= 0 && previousIndex < CategoriesCollection.Count)
                {
                    CategoriesCollection[previousIndex].IsSelected = false;
                }
                if (currentIndex >= 0 && currentIndex < CategoriesCollection.Count)
                {
                    CategoriesCollection[currentIndex].IsSelected = true;
                    var currentItem = CategoriesCollection[currentIndex];
                    _categoryNumber = currentItem.Number;
                    LoadProductsAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnCategoriesPositionChanged error: {ex.Message}");
            }
        }
    }
}
