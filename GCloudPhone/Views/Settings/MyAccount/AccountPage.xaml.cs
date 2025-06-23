using GCloudPhone.Views.Shop;
using GCloudShared.Interface;
using GCloudShared.Service;
using System;
using Microsoft.Maui.Controls;
using GCloudPhone.Views.Shop.Payments;
using GCloudPhone.Views.Shop.OrderProccess;

namespace GCloudPhone.Views.Settings.MyAccount
{
    public partial class AccountPage : ContentPage
    {
        public AccountPage()
        {
            InitializeComponent();
            string username = "Milica";
            UserLabel.Text = $"Willkommen {username}";
        }

        private async void OnAccountTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AccountDetails());
        }

        private async void OnCreditsTapped(object sender, EventArgs e)
        {
            // Implementirajte logiku za Credits ako je potrebno
        }

        private async void OnAddressesTapped(object sender, EventArgs e)
        {
            // Implementirajte logiku za Addresses ako je potrebno
        }

        private async void OnPaymentsTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Payments());
        }

        private async void OnOrderHistoryTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new OrderHistory(new AuthService()));
        }
    }
}
