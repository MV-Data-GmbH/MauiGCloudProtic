using GCloud.Shared.Dto.Domain;
using GCloudPhone.Services;
using GCloudPhone.Views.Aktionen;
using GCloudPhone.Views.Points;
using GCloudPhone.Views.Settings.MyAccount;
using GCloudPhone.Views.Shop;
using GCloudPhone.Views.Shop.OrderProccess;
using GCloudShared.Interface;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Shared;
using System.Windows.Input;

namespace GCloudPhone.Views.Points;

public partial class SpecialProductListSWpts : ContentPage
{
    public ICommand ItemTappedCommand { get; set; }
    public List<CouponDto> Coupons { get; set; } = new List<CouponDto>();
    private IAuthService _authService;
    private readonly IQrScannerService _qrScannerService;


    List<CouponDto> listOfSpecialProducts = new List<CouponDto>();

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
    public SpecialProductListSWpts(IAuthService authService, IQrScannerService qrScannerService)
    {
        InitializeComponent();
        _authService = authService;
        SpecialProductLogged(authService);
        BindingContext = this;

        ItemTappedCommand = new Command<Guid>(NavigateToDetailsPage);

        LoadCoupons();

    }
    private async void SpecialProductLogged(IAuthService authService)
    {
        if (!authService.IsLogged())
        {
            await DisplayAlert("Fehler", "Sie müssen angemeldet sein, um fortzufahren!", "OK");
            await Navigation.PushAsync(new LoginPage(authService));
            return;
        }
    }


    private async void LoadCoupons()
    {
        //ucitava kupone iz prve radnje, poziva GetUserCouponsByStore
        IsLoading = true;
        try
        {
            var storeService = new StoreService();
            var userService = new UserCouponService();

            List<StoreDto> stores = await storeService.GetStores() as List<StoreDto>;
            var firstStore = new StoreDto();
            firstStore = stores[0];

            var storeFirstId = firstStore.Id.ToString();

            var listOfCoupons = (List<CouponDto>)await userService.GetUserCouponsByStore(storeFirstId);


            double DisplayDensity = DeviceDisplay.Current.MainDisplayInfo.Density;

            double pictureWidth;

            pictureWidth = (DeviceDisplay.Current.MainDisplayInfo.Width / DisplayDensity) * 0.46;



            UserRepository ur = new UserRepository(DbBootstraper.Connection);
            var user = ur.GetCurrentUser();
            var points = await _authService.GetTotalPointsByUserID(user.UserId);
            points = points.Replace("\"", "");
            int pointsValue = int.Parse(points);

            if (listOfCoupons is List<CouponDto>)
            {
                foreach (CouponDto coupon in listOfCoupons)
                {
                    if (coupon.CouponType == CouponTypeDto.SpecialProductPoints)
                    {
                        listOfSpecialProducts.Add(coupon);

                        if (pointsValue < coupon.Value)
                        {
                            coupon.CouponPoints = (coupon.Value - pointsValue).ToString("#0");
                            coupon.PointsText = "Punkte fehlen";
                            coupon.TextColor = Colors.Red;
                            coupon.ImageSource = Base64ToImageSource(coupon.IconBase64);
                            coupon.PictureWidth = pictureWidth;
                            coupon.BorderColor = Colors.Red;

                        }
                        else
                        {
                            coupon.CouponPoints = coupon.Value.ToString("#0");
                            coupon.PointsText = "Punkte";
                            coupon.TextColor = Colors.Green;
                            coupon.ImageSource = Base64ToImageSource(coupon.IconBase64);
                            coupon.PictureWidth = pictureWidth;
                            coupon.BorderColor = Color.FromRgba(255, 212, 1, 255);

                        }
                    }
                }

                if (listOfSpecialProducts.Count() > 0)
                {
                    CouponLabel.IsVisible = false;
                    GridCouponList.IsVisible = true;
                    CouponList.ItemsSource = listOfSpecialProducts;
                }
                else
                {
                    CouponLabel.IsVisible = true;
                    GridCouponList.IsVisible = false;
                    CouponList.ItemsSource = listOfSpecialProducts;
                }
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
            IsLoading = false;
        }

    }

    public static ImageSource Base64ToImageSource(string base64String)
    {
        // 1) Check if string is null or empty
        if (string.IsNullOrEmpty(base64String))
        {
            // Option A: Return null => no image
            return null;

            // Option B: Return a placeholder:
            // return ImageSource.FromFile("placeholder.png");
        }

        try
        {
            // 2) Convert from Base64
            byte[] imageBytes = Convert.FromBase64String(base64String);

            // 3) Return an ImageSource from the bytes
            return ImageSource.FromStream(() => new MemoryStream(imageBytes));
        }
        catch (FormatException ex)
        {
            // Usually means invalid Base64 data
            Console.WriteLine($"Invalid base64 string: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            // Some other I/O or memory error
            Console.WriteLine($"Error converting base64 to ImageSource: {ex.Message}");
            return null;
        }
    }




    private void NavigateToDetailsPage(Guid Id)
    {
        if (Id != Guid.Empty)
        {
            var coupon = listOfSpecialProducts.Where(x => x.Id == Id).FirstOrDefault();
            if (coupon != null)
            {
                if (coupon.TextColor == Colors.Green)
                {
                    Navigation.PushAsync(new SpecialProductsDetailsSwpts(Id, _authService));
                }


            }

        }
    }

    private void TapGestureRecognizer_Tapped_6(object sender, TappedEventArgs e)
    {

        Navigation.PushAsync(new BestellungInDerFiliale(_authService, _qrScannerService));
    }
}