

namespace GCloud.Shared.Exceptions
{
    public enum ExceptionStatusCode
    {
        UserNotFound,
        StoreNotFound,
        ApiTokenInvalid,
        CouponNotFound,
        CredentialsInvalid,
        UserDisabled,
        EmailNotConfirmed,
        UsernameAlreadyTaken,
        GeneralRegistrationException,
        SendMailException,
        OldPasswordInvalid,
        CashbackNotFound,
        NoLastCashback,
        AlreadyRedeemed,
        ArgumentInvalid,
        CashRegisterNotInStore,
        AnonymousUserNotFound,
        AnonymousMobilePhoneNotFound,
        InvalidFirebaseToken,
        NoUserIdProvided
    }
}
