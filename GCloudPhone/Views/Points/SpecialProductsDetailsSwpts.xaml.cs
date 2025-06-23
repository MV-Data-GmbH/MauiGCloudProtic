using GCloud.Shared.Dto.Domain;
using GCloudPhone.Services;
using GCloudPhone.Views.Aktionen;
using GCloudPhone.Views.Points;
using GCloudPhone.Views.Shop;
using GCloudPhone.Views.Shop.OrderProccess;
using GCloudShared.Interface;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Shared;
using Newtonsoft.Json;

namespace GCloudPhone.Views.Points;

public partial class SpecialProductsDetailsSwpts : ContentPage
{
    private IUserCouponService _userCouponService;
    private IAuthService _authService;

    public SpecialProductsDetailsSwpts(Guid Id, IAuthService authService)
    {
        InitializeComponent();

        _authService = authService;
        _userCouponService = new UserCouponService();

        string guidString = Id.ToString();

        GetData(guidString);

        UserRepository ur = new UserRepository(DbBootstraper.Connection);
        var user = ur.GetCurrentUser();


        var jsonValue = JsonConvert.SerializeObject(new
        {
            UserId = user.UserId,
            CouponId = Id.ToString()
        });

        CouponQRcode.Value = jsonValue;

      
    }

    


    //detalji kupona , GetUserCoupon,
    private async void GetData(string guidString)
    {
        CouponDto coupon = await _userCouponService.GetUserCoupon(guidString) as CouponDto;
        if (coupon is CouponDto)
        {
            ValidFromLabel.Text = coupon.ValidFrom?.ToString("dd-MM-yyyy") ?? "Unbegrenzt";
            ValidToLabel.Text = coupon.ValidTo?.ToString("dd-MM-yyyy") ?? "Unbegrenzt";
            ReedemableLabel.Text = coupon.RedeemsLeft?.ToString() ?? "Unbegrenzt";
            PointsLabel.Text = coupon.Value.ToString();

        }
        else
        {
            await DisplayAlert("Fehler", "Es ist ein Fehler aufgetreten!", "OK");
        }
    }
    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
    


    private void TapGestureRecognizer_Tapped_6(object sender, TappedEventArgs e)
    {
        var qrScannerService = ((App)Application.Current).Services.GetService(typeof(IQrScannerService)) as IQrScannerService;
        Navigation.PushAsync(new BestellungInDerFiliale(_authService, qrScannerService));

    }

}