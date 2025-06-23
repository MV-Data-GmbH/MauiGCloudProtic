using CommunityToolkit.Maui.Views;

namespace GCloudPhone.Views.Shop.OrderProccess;

public partial class OrderDetailsPage : ContentPage
{
    public OrderWithItemsViewModel OrderWithItems { get; set; }
    public OrderDetailsPage(OrderWithItemsViewModel orderWithItems)
    {
        InitializeComponent();
        OrderWithItems = orderWithItems;
        BindingContext = OrderWithItems;

        CustomBackButton.OverrideNavigation = true;
        CustomBackButton.BackButtonClicked += OnCustomBackButtonClicked;
    }
    private async void OnCustomBackButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MainPage());
        //var popup = new OrderTrackingPopup();
        //Application.Current.MainPage.ShowPopup(popup);
    }

    // Optionally, unsubscribe from event when the page is disappearing to avoid memory leaks
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        CustomBackButton.BackButtonClicked -= OnCustomBackButtonClicked;
    }

    private void OnSwipedRight(object sender, SwipedEventArgs e)
    {
        
        Navigation.PushAsync(new MainPage());
    }
}