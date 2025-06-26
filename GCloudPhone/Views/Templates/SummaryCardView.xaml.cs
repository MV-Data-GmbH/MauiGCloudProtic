using Microsoft.Maui.Controls;

namespace GCloudPhone.Views.Templates
{
    public partial class SummaryCardView : ContentView
    {
        public SummaryCardView()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty ItemsTotalProperty =
            BindableProperty.Create(
                nameof(ItemsTotal),
                typeof(decimal),
                typeof(SummaryCardView),
                default(decimal));

        public decimal ItemsTotal
        {
            get => (decimal)GetValue(ItemsTotalProperty);
            set => SetValue(ItemsTotalProperty, value);
        }

        public static readonly BindableProperty DeliveryFeeProperty =
            BindableProperty.Create(
                nameof(DeliveryFee),
                typeof(decimal),
                typeof(SummaryCardView),
                default(decimal));

        public decimal DeliveryFee
        {
            get => (decimal)GetValue(DeliveryFeeProperty);
            set => SetValue(DeliveryFeeProperty, value);
        }

        public static readonly BindableProperty VATProperty =
            BindableProperty.Create(
                nameof(VAT),
                typeof(decimal),
                typeof(SummaryCardView),
                default(decimal));

        public decimal VAT
        {
            get => (decimal)GetValue(VATProperty);
            set => SetValue(VATProperty, value);
        }

        public static readonly BindableProperty TipProperty =
            BindableProperty.Create(
                nameof(Tip),
                typeof(decimal),
                typeof(SummaryCardView),
                default(decimal));

        public decimal Tip
        {
            get => (decimal)GetValue(TipProperty);
            set => SetValue(TipProperty, value);
        }

        public static readonly BindableProperty TotalPriceProperty =
            BindableProperty.Create(
                nameof(TotalPrice),
                typeof(decimal),
                typeof(SummaryCardView),
                default(decimal));

        public decimal TotalPrice
        {
            get => (decimal)GetValue(TotalPriceProperty);
            set => SetValue(TotalPriceProperty, value);
        }
    }
}
