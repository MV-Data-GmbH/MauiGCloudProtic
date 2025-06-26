// ShoppingCart.xaml.cs
using System;
using GCloudPhone.ViewModels;
using GCloudPhone.Views.Shop.Checkout;
using GCloudPhone.Views.Shop.OrderProccess;
using GCloudShared.Shared;
using Microsoft.Maui.Controls;

namespace GCloudPhone.Views.Shop.ShoppingCart
{
    public partial class ShoppingCart : ContentPage
    {
        private readonly ShoppingCartViewModel viewModel;

        public ShoppingCart()
        {
            InitializeComponent();
            viewModel = new ShoppingCartViewModel();
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await viewModel.LoadCartItemsAsync();
            await viewModel.LoadProductsAsync();
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
                viewModel.OrderNote = result;
            }
        }

        private async void ContinueToCheckoutClicked(object sender, EventArgs e)
        {
            var page = new CheckoutPage
            {
                Mode = App.OrderType,
                OrderNote = Uri.EscapeDataString(viewModel.OrderNote ?? string.Empty)
            };

            // više ne postavljamo Preferences ovde—
            // CheckoutPage će prilikom učitavanja čitati:
            // var mode = Preferences.Get("CheckoutMode", "");
            // var note = Preferences.Get("CheckoutNote", "");

            await Navigation.PushAsync(page);
        }

        private async void imgHome_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MainPage());
        }

        private async void CancelOrder_Tapped(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert(
                "Bestellung stornieren",
                "Sind Sie sicher, dass Sie Ihre Bestellung stornieren möchten?",
                "Ja", "Nein");

            if (confirm)
            {
                Cart.Instance.ClearCart();
                await Navigation.PushAsync(new CategoriesPage());
            }
        }
    }
}

