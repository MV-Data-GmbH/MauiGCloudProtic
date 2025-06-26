using System;
using System.Linq;
using GCloudShared.Service;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;              // za Preferences
using GCloudPhone.Views.Shop;               // za MainPage
using GCloudPhone.Views.Shop.OrderProccess; // za OrderHistory, OrderTypePage
using GCloudPhone.Views.Shop.Checkout;      // za CheckoutPage
using GCloudPhone.Views.Settings.MyAccount;
using GCloudPhone.Views.Shop.ShoppingCart;  // za ShoppingCart
using GCloudPhone.Views.Points;             // za MyPointsPage
using GCloudShared.Shared;                  // za Cart.Instance

namespace GCloudPhone.Views.Templates
{
    public partial class BackButtonView : ContentView
    {
        public event EventHandler BackButtonClicked;
        public bool OverrideNavigation { get; set; } = false;

        public BackButtonView()
        {
            InitializeComponent();
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            BackButtonClicked?.Invoke(this, e);

            if (!OverrideNavigation && Navigation != null)
            {
                var currentPage = Navigation.NavigationStack.LastOrDefault();

                // Ako je trenutna stranica CheckoutPage, očisti korpu i obriši način dostave,
                // pa idi na MainPage
                if (currentPage is CheckoutPage)
                {
                    Cart.Instance.ClearCart();
                    Preferences.Remove("CheckoutMode");
                    Preferences.Remove("SelectedStoreId");
                    Preferences.Remove("UsedPoints");
                    await Navigation.PushAsync(new MainPage());
                    return;
                }

                // Ako je trenutna stranica OrderHistory i marker kaže da je došao sa OrderTypePage,
                // izbriši marker i vrati korisnika na OrderTypePage.
                string previousPage = Preferences.Get("PreviousPage", string.Empty);
                if (currentPage is OrderHistory && previousPage == "OrderTypePage")
                {
                    Preferences.Remove("PreviousPage");
                    await Navigation.PushAsync(new OrderTypePage(new AuthService()));
                    return;
                }

                // Ako je trenutna stranica ManageAddressesPage, idi na MainPage.
                if (currentPage is ManageAddressesPage)
                {
                    await Navigation.PushAsync(new MainPage());
                    return;
                }
                // Ako je trenutna stranica AddAddressPage, idi na ManageAddressesPage.
                else if (currentPage is AddAddressPage)
                {
                    await Navigation.PushAsync(new ManageAddressesPage());
                    return;
                }
                // Novo pravilo: ako je trenutna stranica WarenkorbPage (korpa), idi na CategoriesPage.
                else if (currentPage is ShoppingCart)
                {
                    await Navigation.PushAsync(new CategoriesPage());
                    return;
                }
                // *** DODATO ***: Ako je trenutna stranica SpecialProductListSWpts, idi na MyPointsPage
                else if (currentPage is SpecialProductListSWpts)
                {
                    await Navigation.PushAsync(new MyPointsPage(new AuthService()));
                    return;
                }

                // Ako je trenutna stranica CategoriesPage, pitaj da li nastaviti
                if (currentPage is CategoriesPage)
                {
                    if (Cart.Instance.Items.Any())
                    {
                        bool continueOrder = await Application.Current.MainPage.DisplayAlert(
                            "Aktuelle Bestellung",
                            "Möchten Sie Ihre aktuelle Bestellung fortsetzen?",
                            "Ja",
                            "Nein");

                        if (!continueOrder)
                        {
                            // 1) Očisti korpu
                            Cart.Instance.ClearCart();

                            // 2) Obriši sve relevantne Preferences
                            Preferences.Remove("SelectedStoreId");
                            Preferences.Remove("SelectedStoreName");
                            Preferences.Remove("SelectedStoreAddress");
                            Preferences.Remove("UsedPoints");
                            Preferences.Remove("CurrentOrderId");
                            Preferences.Remove("OrderNote");
                            Preferences.Remove("CheckoutMode");

                            // 3) Resetuj stanje na nivou aplikacije
                            App.OrderType = null;
                            App.SignalR.OnlineUsers.Clear();
                        }
                    }

                    await Navigation.PushAsync(new MainPage());
                }
                // Ako je Login, NotificationCenter ili Register, idi na MainPage
                else if (currentPage is LoginPage
                         || currentPage is NotificationCenterPage
                         || currentPage is RegisterPage)
                {
                    await Navigation.PushAsync(new MainPage());
                }
                else
                {
                    // Ostalo: ako je OrderTypePage, idi na MainPage, a inače na CategoriesPage
                    if (currentPage is OrderTypePage)
                        await Navigation.PushAsync(new MainPage());
                    else
                        await Navigation.PushAsync(new CategoriesPage());
                }
            }
        }
    }
}
