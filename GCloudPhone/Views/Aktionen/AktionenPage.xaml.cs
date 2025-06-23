using GCloud.Shared.Dto.Domain;
using GCloudPhone.Views.Aktionen;
using GCloudPhone.Views.Points;
using GCloudPhone.Views.Settings.MyAccount;
using GCloudPhone.Views.Shop;
using GCloudPhone.Views.Shop.OrderProccess;
using GCloudPhone.Views.Templates;
using GCloudShared.Interface;
using GCloudShared.Repository;
using GCloudShared.Service;
using System.Windows.Input;

namespace GCloudPhone.Views.Aktionen
{
    public partial class AktionenPage : ContentPage
    {
        public List<CouponDto> Aktionen { get; set; } = new List<CouponDto>();
        public ICommand ItemTappedCommand { get; set; }
        private IAuthService _authService;
        private bool IsLogging;
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }

        public AktionenPage(IAuthService authService)
        {
            InitializeComponent();
            _authService = new AuthService(); // Možete koristiti authService direktno ako želite
            AktieLogged(authService);
            BindingContext = this;

            if (IsLogging)
            {
                ItemTappedCommand = new Command<Guid>(NavigateToAktionenDetailsPage);
                LoadAktionen();
            }

            // Pretplata na događaje NavigationBar-a
            navigationBar.HomeTapped += NavigationBar_HomeTapped;
            navigationBar.ProductTapped += NavigationBar_ProductTapped;
            navigationBar.BestellenTapped += NavigationBar_BestellenTapped;
            navigationBar.AktionenTapped += NavigationBar_AktionenTapped;
            navigationBar.PunkteTapped += NavigationBar_PunkteTapped;
        }

        private async void AktieLogged(IAuthService authService)
        {
            if (!authService.IsLogged())
            {
                IsLogging = false;
                await DisplayAlert("Fehler", "Sie müssen angemeldet sein, um fortzufahren!", "OK");
                await Navigation.PushAsync(new LoginPage(authService));
                return;
            }
            else
            {
                IsLogging = true;
            }
        }

        private async void LoadAktionen()
        {
            IsLoading = true;
            try
            {
                var userService = new UserCouponService();
                var storeService = new StoreService();

                List<StoreDto> stores = await storeService.GetStores() as List<StoreDto>;
                if (stores == null || stores.Count == 0)
                {
                    await DisplayAlert("Fehler", "Es ist ein Fehler aufgetreten!", "OK");
                    return;
                }
                var firstStore = stores[0];
                string storeFirstId = firstStore.Id.ToString();

                List<CouponDto> listOfCoupons1 = null;
                try
                {
                    var result = await userService.GetUserCouponsByStore(storeFirstId);
                    listOfCoupons1 = result as List<CouponDto>;

                    if (listOfCoupons1 == null)
                    {
                        throw new InvalidOperationException("Es wurde eine Liste von Gutscheinen erwartet, aber ein anderer Typ empfangen.");
                    }
                }
                catch (Exception)
                {
                    await DisplayAlert("Fehler", "Ein Fehler ist aufgetreten!", "OK");
                }

                var listOfCoupons = listOfCoupons1 as List<CouponDto>;
                List<CouponDto> listAktionen = new List<CouponDto>();

                double displayDensity = DeviceDisplay.Current.MainDisplayInfo.Density;
                double pH = (DeviceDisplay.Current.MainDisplayInfo.Height / displayDensity);
                double pictureWidth = (DeviceDisplay.Current.MainDisplayInfo.Width / displayDensity) * 0.46;

                if (listOfCoupons is List<CouponDto>)
                {
                    foreach (CouponDto coupon in listOfCoupons)
                    {
                        if (coupon.CouponType == CouponTypeDto.Percent)
                        {
                            coupon.CouponPoints = coupon.Value.ToString("#0") + " %";
                            coupon.ImageSource = Base64ToImageSource(coupon.IconBase64);
                            coupon.PictureWidth = pictureWidth;
                            listAktionen.Add(coupon);
                        }
                        if (coupon.CouponType == CouponTypeDto.Value)
                        {
                            coupon.CouponPoints = coupon.Value.ToString() + " €";
                            coupon.ImageSource = Base64ToImageSource(coupon.IconBase64);
                            coupon.PictureWidth = pictureWidth;
                            listAktionen.Add(coupon);
                        }
                    }

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (listAktionen.Count > 0)
                        {
                            AktionenList.IsVisible = true;
                            AktioneLabel.IsVisible = false;
                        }
                        else
                        {
                            AktionenList.IsVisible = false;
                            AktioneLabel.IsVisible = true;
                        }
                        AktionenList.ItemsSource = listAktionen;
                    });
                }
                else
                {
                    await DisplayAlert("Fehler", "Es ist ein Fehler aufgetreten!", "OK");
                    return;
                }
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    DisplayAlert("Fehler", ex.Message, "OK");
                });
            }
            finally
            {
                IsLoading = false;  // Deaktivira ActivityIndicator
            }
        }

        public static ImageSource Base64ToImageSource(string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
                return null;

            try
            {
                byte[] imageBytes = Convert.FromBase64String(base64String);
                return ImageSource.FromStream(() => new MemoryStream(imageBytes));
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Invalid base64 string: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting base64 to ImageSource: {ex.Message}");
                return null;
            }
        }

        private void NavigateToAktionenDetailsPage(Guid Id)
        {
            if (Id != Guid.Empty)
            {
                Navigation.PushAsync(new AktionenCouponDetails(Id, _authService));
            }
        }

        // NavigationBar event handler-i

        private async void NavigationBar_HomeTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MainPage());
        }

        private async void NavigationBar_ProductTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }

        private async void NavigationBar_BestellenTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new OrderTypePage(_authService));
        }

        private async void NavigationBar_AktionenTapped(object sender, EventArgs e)
        {
            // Već se nalazimo na Seiten Aktionen – ovde možete izvršiti osvežavanje ili ostaviti prazno.
        }

        private async void NavigationBar_PunkteTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MyPointsPage(_authService));
        }
    }
}
