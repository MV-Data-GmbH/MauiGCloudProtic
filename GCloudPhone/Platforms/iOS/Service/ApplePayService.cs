using System;
using System.Threading.Tasks;
using Foundation;
using PassKit;
using StoreKit;
using UIKit;

namespace GCloudPhone.Platforms.iOS.Service
{
    public class ApplePayService : PKPaymentAuthorizationViewControllerDelegate
    {
        private TaskCompletionSource<bool> paymentTaskCompletionSource;

        public async Task<bool> MakePayment(decimal totalPrice)
        {
            paymentTaskCompletionSource = new TaskCompletionSource<bool>();

            SKProductsRequest req = new SKProductsRequest(new NSSet());

            NSString[] paymentNetworks = new NSString[]
            {
                PKPaymentNetwork.Visa,
                PKPaymentNetwork.MasterCard,
                PKPaymentNetwork.Amex
            };

            var canMakePayment = PKPaymentAuthorizationViewController.CanMakePayments;

            PKPaymentRequest paymentRequest = new PKPaymentRequest
            {
                MerchantIdentifier = "merchant.com.companyname.gcloudshop",
                SupportedNetworks = paymentNetworks,
                MerchantCapabilities = PKMerchantCapability.ThreeDS,
                CountryCode = "AT",
                CurrencyCode = "EUR",
                PaymentSummaryItems = new PKPaymentSummaryItem[]
                {
                    new PKPaymentSummaryItem
                    {
                        Label = "Sample Purchase Item",
                        Amount = new NSDecimalNumber(totalPrice.ToString())
                    }
                }
            };

            var canMakePaymentsUsingNetworks = PKPaymentAuthorizationViewController.CanMakePaymentsUsingNetworks(paymentNetworks);

            if (canMakePaymentsUsingNetworks)
            {
                var controller = new PKPaymentAuthorizationViewController(paymentRequest)
                {
                    Delegate = this
                };

                var rootController = UIApplication.SharedApplication.Delegate.GetWindow().RootViewController;
                rootController.PresentViewController(controller, true, null);

                return await paymentTaskCompletionSource.Task;
            }
            else
            {
                return false;
            }
        }

        public override void DidAuthorizePayment(PKPaymentAuthorizationViewController controller, PKPayment payment,
              Action<PKPaymentAuthorizationStatus> completion)
        {
            completion(PKPaymentAuthorizationStatus.Success);
            paymentTaskCompletionSource.SetResult(true);
            controller.DismissViewController(true, null);
        }

        public override void PaymentAuthorizationViewControllerDidFinish(PKPaymentAuthorizationViewController controller)
        {
            if (!paymentTaskCompletionSource.Task.IsCompleted)
            {
                paymentTaskCompletionSource.SetResult(false);
            }
            controller.DismissViewController(true, null);
        }

        public override void WillAuthorizePayment(PKPaymentAuthorizationViewController controller)
        {
        }
    }
}
