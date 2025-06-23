using System.ComponentModel;
using System.Linq;
using System.Threading;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using GCloudPhone.Views.Shop.ShoppingCart;

namespace GCloudPhone.Views.Templates
{
    public partial class CartTemplate : ContentView, INotifyPropertyChanged
    {
        private decimal totalPrice;
        private int itemCount;

        public int ItemCount
        {
            get => itemCount;
            set
            {
                if (itemCount != value)
                {
                    itemCount = value;
                    OnPropertyChanged(nameof(ItemCount));
                    OnPropertyChanged(nameof(IsLabelVisible)); 
                }
            }
        }

        public new event PropertyChangedEventHandler PropertyChanged;

        public decimal TotalPrice
        {
            get => totalPrice;
            set
            {
                if (totalPrice != value)
                {
                    totalPrice = value;
                    OnPropertyChanged(nameof(TotalPrice));
                    OnPropertyChanged(nameof(IsLabelVisible)); 
                }
            }
        }

        public bool IsLabelVisible => TotalPrice > 0;

        public CartTemplate()
        {
            InitializeComponent();
            BindingContext = this;
            UpdateTotalPrice();
            Cart.Instance.ItemCountChanged += OnItemCountChanged;
            UpdateItemCount();
        }

        private async void OnCartTapped(object sender, EventArgs e)
        {
            if (itemCount == 0) {
                string text = "Der Warenkorb ist leer.";
                ToastDuration duration = ToastDuration.Short;
                double fontSize = 14;

                var toast = Toast.Make(text, duration, fontSize);
                await toast.Show();

                return;
            }
            await Navigation.PushAsync(new Warenkorb());
        }

        public void UpdateTotalPrice()
        {
            TotalPrice = Cart.Instance.GetItems().Sum(item => item.Amount.GetValueOrDefault() * item.Quantity.GetValueOrDefault());
            OnPropertyChanged(nameof(IsLabelVisible));
        }

        private void OnItemCountChanged()
        {
            UpdateTotalPrice();
            UpdateItemCount();
        }

        public void UpdateItemCount()
        {
            ItemCount = Cart.Instance.GetItems().Count;
            OnPropertyChanged(nameof(IsLabelVisible));
        }

        protected new void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
