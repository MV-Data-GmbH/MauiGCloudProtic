using GCloud.Shared.Dto.Domain;
using GCloudPhone.Views.Shop;
using GCloudShared.Interface;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Shared;
using Newtonsoft.Json;

namespace GCloudPhone.Views.Aktionen;

public partial class AktionenCouponDetails : ContentPage
{
    private IUserCouponService _userCouponService;
    private IAuthService _authService;


    public AktionenCouponDetails(Guid Id, IAuthService authService)
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



    private async void GetData(string guidString)
    {
        CouponDto coupon = await _userCouponService.GetUserCoupon(guidString) as CouponDto;
        if (coupon is CouponDto)
        {

            ValidFromLabel.Text = coupon.ValidFrom?.ToString("dd-MM-yyyy") ?? "Unbegrenzt";

            ValidToLabel.Text = coupon.ValidTo?.ToString("dd-MM-yyyy") ?? "Unbegrenzt";
            ReedemableLabel.Text = coupon.RedeemsLeft?.ToString();

            if (coupon.CouponType == CouponTypeDto.Percent)
            {
                PointsLabel.Text = coupon.Value.ToString("#0") + " %";

            }
            if (coupon.CouponType == CouponTypeDto.Value)
            {
                PointsLabel.Text = coupon.Value.ToString() + " €";
            }

        }
        else
        {
            await DisplayAlert("Fehler", "Es ist ein Fehler aufgetreten!", "OK");
        }
    }

    private async void OnBackButtonClicked(object sender, System.EventArgs e)
    {
        await Navigation.PopAsync();
    }


  


}