using System;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using GCloudPhone.Views.Shop.OrderProccess;
using GCloudPhone.Views.Shop.ShoppingCart;
using GCloudShared.Shared;
using Microsoft.Maui.Controls;

namespace GCloudPhone.Views.Templates
{
    public partial class CartTemplate : ContentView, INotifyPropertyChanged
    {
        private decimal totalPrice;
        private int itemCount;

        public CartTemplate()
        {
            InitializeComponent();
            BindingContext = this;

            // Inicijalno popuni
            UpdateTotalPrice();
            UpdateItemCount();

            // Kad god se menjaju stavke u korpi
            Cart.Instance.ItemCountChanged += OnItemCountChanged;

            // Ako OrderDetailsPage baci poruku (prilikom prvog otvaranja)
            MessagingCenter.Subscribe<OrderDetailsPage>(this, "OrderCompleted", sender =>
            {
                RefreshCart();
            });

            // **Novo**: presrećemo „back“ navigaciju iz OrderDetailsPage
            if (Application.Current.MainPage is NavigationPage navPage)
                navPage.Popped += OnNavPagePopped;
        }

        // Oslobađanje resursa (opciono)
        ~CartTemplate()
        {
            if (Application.Current.MainPage is NavigationPage navPage)
                navPage.Popped -= OnNavPagePopped;

            Cart.Instance.ItemCountChanged -= OnItemCountChanged;
            MessagingCenter.Unsubscribe<OrderDetailsPage>(this, "OrderCompleted");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int ItemCount
        {
            get => itemCount;
            set
            {
                if (itemCount != value)
                {
                    itemCount = value;
                    OnPropertyChanged(nameof(ItemCount));
                    OnPropertyChanged(nameof(HasMultipleItems));
                }
            }
        }

        public decimal TotalPrice
        {
            get => totalPrice;
            set
            {
                if (totalPrice != value)
                {
                    totalPrice = value;
                    OnPropertyChanged(nameof(TotalPrice));
                }
            }
        }

        // Uvek prikazujemo 0 ako je prazno
        public bool IsLabelVisible => true;

        public bool HasMultipleItems => ItemCount > 1;

        private async void OnCartTapped(object sender, EventArgs e)
        {
            if (ItemCount == 0)
            {
                var toast = Toast.Make("Der Warenkorb ist leer.", ToastDuration.Short, 14);
                await toast.Show();
                return;
            }
            await Navigation.PushAsync(new ShoppingCart());
        }

        private void OnItemCountChanged()
        {
            RefreshCart();
        }

        // Ovaj metod pozovite i iz svih hook-ova
        void RefreshCart()
        {
            UpdateTotalPrice();
            UpdateItemCount();
        }

        public void UpdateItemCount()
        {
            ItemCount = Cart.Instance.GetItems().Count;
        }

        public void UpdateTotalPrice()
        {
            TotalPrice = Cart.Instance.GetItems()
                .Sum(i => i.Amount.GetValueOrDefault() * i.Quantity.GetValueOrDefault());
        }

        private void OnNavPagePopped(object sender, NavigationEventArgs e)
        {
            // kad se pop-uje (t.j. korisnik klikne back) OrderDetailsPage
            if (e.Page is OrderDetailsPage)
                RefreshCart();
        }

        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
