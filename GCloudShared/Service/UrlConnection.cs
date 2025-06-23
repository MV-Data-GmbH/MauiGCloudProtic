using GCloud.Shared;


namespace GCloudShared.Service
{
    public  class UrlConnection
    {
        #region AuthService

        public static string LoginUrl { get => BaseUrlContainer.BaseUri.ToString()+ "api/HomeApi/Login"; }
        public static string RegisterUrl { get => BaseUrlContainer.BaseUri.ToString() + "api/HomeApi/Register"; }
        public static string IsUsernameAvailableUrl { get => BaseUrlContainer.BaseUri.ToString() + "api/HomeApi/Available/"; }
        public static string IsEmailAvailableUrl { get => BaseUrlContainer.BaseUri.ToString() + "api/HomeApi/IsEmailAvailable/"; }
        public static string IsInvitationCodeAvailableUrl { get => BaseUrlContainer.BaseUri.ToString() + "api/HomeApi/IsInvitationCodeAvailable/"; }
        public static string InvitationCodeSenderIdUrl { get => BaseUrlContainer.BaseUri.ToString() + "api/HomeApi/InvitationCodeSenderId"; }
        public static string GetSpecialProductsByCompanyNameUrl { get => BaseUrlContainer.BaseUri.ToString() + "api/HomeApi/GetSpecialProductsByCompanyName"; }
        public static string GetQRCodeUrl { get => BaseUrlContainer.BaseUri.ToString() + "api/UsersApi/GetQrCode"; }
        public static string ChangePasswordUrl { get => BaseUrlContainer.BaseUri.ToString() + "api/HomeApi/ChangePassword"; }
        public static string Logout { get => BaseUrlContainer.BaseUri.ToString() + "api/HomeApi/Logout?deviceId="; }
        public static string ResetPasswordUrl { get => BaseUrlContainer.BaseUri.ToString() + "api/HomeApi/ResetPassword"; }
        public static string GetTotalPointsByUserIDUrl { get => BaseUrlContainer.BaseUri.ToString() + "api/HomeApi/GetTotalPointsByUserID"; }
        public static string ResendActivationEmailUrl { get => BaseUrlContainer.BaseUri.ToString() + "api/HomeApi/ResendActivationEmail"; }
        public static string CheckIfUserCanBuySpecialProduct { get => BaseUrlContainer.BaseUri.ToString() + "api/HomeApi/CheckIfUserCanBuySpecialProduct"; }
        public static string DeleteUser { get => BaseUrlContainer.BaseUri.ToString() + "api/HomeApi/DeleteUser"; }
        public static string GetAllUsers { get => BaseUrlContainer.BaseUri.ToString() + "api/HomeApi/GetAllUsers"; }

        //preimenovati posle promene
        public static string GetPointsAfterPurchase { get => BaseUrlContainer.BaseUri.ToString() + "api/HomeApi/UpdateUserTotalPointsAsync"; }
        public static string TransferPointsToUser { get => BaseUrlContainer.BaseUri.ToString() + "api/HomeApi/TransferPointsProcedura"; }
        public static string DecreasePoints { get => BaseUrlContainer.BaseUri.ToString() + "api/HomeApi/DecreasePointsProcedura"; }
        public static string BuyPoints { get => BaseUrlContainer.BaseUri.ToString() + "/api/HomeApi/BuyPointsProcedura"; }
        public static string AddAddress { get => BaseUrlContainer.BaseUri.ToString() + "/api/HomeApi/AddAddress"; }
        public static string GetAddressByUserId { get => BaseUrlContainer.BaseUri.ToString() + "api/HomeApi/GetAddressByUserID"; }
        public static string UpdateAddress { get => BaseUrlContainer.BaseUri.ToString() + "/api/HomeApi/UpdateAddress"; }
        public static string DeleteAddress { get => BaseUrlContainer.BaseUri.ToString() + "/api/HomeApi/DeleteAddress"; }

        public static string CheckNotifications { get => BaseUrlContainer.BaseUri + "api/HomeApi/CheckMobileNotifications"; }
        #endregion

        #region BillService
        public static string GetUrl { get => BaseUrlContainer.BaseUri +"api/BillApi/Get"; }
        public static string GetByIdUrl { get => BaseUrlContainer.BaseUri+"api/BillApi/GetById"; }
        public static string CsvUrl { get => BaseUrlContainer.BaseUri+"api/BillApi/CSV"; }
        #endregion

        #region CashBackService
        public static string GetCashbacksForStoreUrl { get => BaseUrlContainer.BaseUri+"/api/Cashback"; }
        #endregion

        #region StartupService
        public static string GetBackGroundImagesUrl { get => BaseUrlContainer.BaseUri + "api/HomeApi/GetBackGroundImages"; }

        public static string LoadInitialDataUrl { get => BaseUrlContainer.BaseUri + "api/HomeApi/LoadInitialData"; }
        #endregion

        #region StoresService
        public static string GetStoreImageUrl { get => BaseUrlContainer.BaseUri + "Stores/LoadStoreImage/"; }
        public static string GetStoresUrl { get => BaseUrlContainer.BaseUri+"api/StoresApi"; }
        public static string UpdateStoreUrl { get => BaseUrlContainer.BaseUri + "api/StoresApi"; }

        #endregion

        #region UserCouponService
        public static string GetCouponImageUrl { get => BaseUrlContainer.BaseUri + "Coupons/LoadCouponImage/"; }
        public static string GetCouponQrCodeUrl { get => BaseUrlContainer.BaseUri + "api/UserCouponsApi/LoadCouponQrCode/"; }
        public static string GetManagerCouponsUrl { get => BaseUrlContainer.BaseUri + "api/UserCouponsApi/GetManagerCoupons"; }
        public static string GetUserCouponUrl { get => BaseUrlContainer.BaseUri + "api/UserCouponsApi/"; }
        public static string GetUserCouponsUrl { get => BaseUrlContainer.BaseUri + "api/UserCouponsApi?skipUserValidation="; }
        public static string GetUserCouponsByStoreUrl { get => $"{BaseUrlContainer.BaseUri}api/UserCouponsApi/store/"; }

        #endregion

        #region UserStoreService
        public static string AddToWatchListUrl { get => $"{BaseUrlContainer.BaseUri}api/UserStoresApi/"; }
        public static string DeleteFromWatchlist { get => $"{BaseUrlContainer.BaseUri}api/UserStoresApi/"; }
        public static string GetManagerStoresUrl { get => $"{BaseUrlContainer.BaseUri}api/UserStoresApi/GetManagerStores"; }
        public static string GetUserStoresUrl { get => $"{BaseUrlContainer.BaseUri}api/UserStoresApi/"; }
        #endregion

        #region WebShopService
        public static string CheckIfUserIsAlreadyRegistredInWebShopUrl { get => BaseUrlContainer.BaseUriWeb+"CustomerApi/CheckIfCustomerAlreadyRegistred"; }
        public static string RegisterWebUrl { get => BaseUrlContainer.BaseUriWeb + "CustomerApi/Register"; }
        public static string ResetPasswordInWebShopFromGcloudUrl { get => BaseUrlContainer.BaseUriWeb + "Customer/PasswordRecoveryFromGCloud"; }
        public static string SetWelcomeEmailToWebShopFromGcloudUrl { get => BaseUrlContainer.BaseUriWeb + "Customer/WelcomeMailFromGCloud"; }
        public static string DeleteUserWebshop { get => BaseUrlContainer.BaseUriWeb + "Customer/DeleteUserWebshop"; }

        public static string GetLastOrder { get => BaseUrlContainer.BaseUriWeb + "CustomerApi/GetLastOrder"; }
        public static string GetOrdersInLastHour { get => BaseUrlContainer.BaseUriWeb + "CustomerApi/GetOrdersInLastHour"; }

        #endregion

        public static bool CheckInternetConnection()
        {
            NetworkAccess accessType = Connectivity.Current.NetworkAccess;

            if (accessType == NetworkAccess.Internet)
            {
                return true;
                // Connection to internet is available
            }
            else
            {
                return false;
            }
        }
    }
}
