using System;
using System.Collections.ObjectModel; // Omogućava upotrebu ObservableCollection koja automatski obaveštava UI o promenama u kolekciji
using System.Diagnostics;             // Omogućava upotrebu Debug klase za ispis poruka tokom razvoja
using System.IO;                      // Za MemoryStream
using System.Threading.Tasks;        // Za Task
using GCloudShared.Interface;         // Uključuje interfejse definisane u GCloudShared projektu (npr. IAuthService)
using Microsoft.Maui.Controls;       // Za ContentPage, ImageSource, itd.

namespace GCloudPhone.Views.Shop.OrderProccess
{
    public partial class CategoriesPage : ContentPage
    {
        private readonly IAuthService _authService;

        public ObservableCollection<CategoriesView> CategoryCollection { get; set; }
        public ObservableCollection<Stores> Stores { get; set; } = new ObservableCollection<Stores>();

        public CategoriesPage()
        {
            Debug.WriteLine("CategoriesPage ctor start");                                   // 1
            InitializeComponent();                                                           // 2
            Debug.WriteLine("InitializeComponent complete");                                 // 3

            CategoryCollection = new ObservableCollection<CategoriesView>();
            ListViewCollection.ItemsSource = CategoryCollection;
            Debug.WriteLine($"Postavljen ItemsSource, initial CategoryCollection.Count = {CategoryCollection.Count}"); // 4

            SetCategoryDisplay(Config.CategoryDisplay);
            Debug.WriteLine($"Prikaz kategorija podešen na: {Config.CategoryDisplay}");      // 5

            this.BindingContext = App.SignalR;
            Debug.WriteLine("BindingContext postavljen na App.SignalR");                      // 6

            // Obavezno proveri u XAML da li imaš Loaded="OnLoaded" na <ContentPage ... />
            Debug.WriteLine("CategoriesPage ctor end");                                     // 7
        }

        public CategoriesPage(IAuthService authService) : this()
        {
            Debug.WriteLine("CategoriesPage(IAuthService) ctor start");                     // 8
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            Debug.WriteLine("_authService dodeljen");                                        // 9
            Debug.WriteLine("CategoriesPage(IAuthService) ctor end");                       // 10
        }

        // Ako ti Loaded event nije pouzdan, možeš i override-ovati OnAppearing:
        protected override void OnAppearing()
        {
            base.OnAppearing();
            Debug.WriteLine("OnAppearing called");                                          // 11
        }

        private async void OnLoaded(object sender, EventArgs e)
        {
            Debug.WriteLine("OnLoaded start");                                              // 12

            Debug.WriteLine($"StartUpDataImport.coupons.Count = {StartUpDataImport.coupons.Count}"); // 13
            if (StartUpDataImport.coupons.Count > 0)
            {
                CouponTaste.IsVisible = true;
                Debug.WriteLine("CouponTaste vidljiv");                                     // 14
            }

            Debug.WriteLine($"Pre Clear, CategoryCollection.Count = {CategoryCollection.Count}"); // 15
            CategoryCollection.Clear();
            Debug.WriteLine($"Posle Clear, CategoryCollection.Count = {CategoryCollection.Count}"); // 16

            await LoadCategoriesAsync();

            Debug.WriteLine($"Posle LoadCategoriesAsync, CategoryCollection.Count = {CategoryCollection.Count}"); // 17
            Debug.WriteLine("OnLoaded end");                                                // 18
        }

        private async Task LoadCategoriesAsync()
        {
            Debug.WriteLine("LoadCategoriesAsync start");                                    // 19
            try
            {
                var categories = await SQL.GetAllCategoriesAsync();
                Debug.WriteLine($"SQL.GetAllCategoriesAsync vratio {categories?.Count ?? 0} stavki"); // 20

                if (categories == null || categories.Count == 0)
                {
                    Debug.WriteLine("Nema kategorija za prikaz!");                          // 21
                }

                foreach (var category in categories)
                {
                    Debug.WriteLine($"Obrada category.Number = {category.Number}");        // 22

                    ImageSource imageSource;
                    if (!string.IsNullOrEmpty(category.Picturestring))
                    {
                        Debug.WriteLine("Kreiranje ImageSource iz Base64");                // 23
                        byte[] imageBytes = Convert.FromBase64String(category.Picturestring);
                        imageSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                    }
                    else
                    {
                        Debug.WriteLine("Koristi default food.png");                       // 24
                        imageSource = ImageSource.FromFile("food.png");
                    }

                    CategoryCollection.Add(new CategoriesView
                    {
                        Number = category.Number,
                        Description1 = category.Description1,
                        Image = imageSource,
                        IsSelected = false
                    });
                    Debug.WriteLine($"Dodato u kolekciju: {category.Number}");             // 25
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadCategoriesAsync error: {ex}");                       // 26
                await DisplayAlert("Error", "Failed to load categories. Please try again later.", "OK");
            }
            Debug.WriteLine("LoadCategoriesAsync end");                                     // 27
        }

        private async void OnItemTapped(object sender, EventArgs e)
        {
            Debug.WriteLine("OnItemTapped start");                                          // 28
            if (sender is Grid grid && grid.BindingContext is CategoriesView category)
            {
                Debug.WriteLine($"Odabrana kategorija: {category.Number}");                // 29
                await Navigation.PushAsync(new CategoriesDetails(category.Number));
            }
            else
            {
                Debug.WriteLine("OnItemTapped: sender nije Grid ili BindingContext nije CategoriesView"); // 30
            }
            Debug.WriteLine("OnItemTapped end");                                            // 31
        }

        private void SetCategoryDisplay(string displayMode)
        {
            Debug.WriteLine($"SetCategoryDisplay pozvana sa '{displayMode}'");              // 32
            if (displayMode == "Grid")
            {
                ListViewCollection.IsVisible = false;
                Debug.WriteLine("ListViewCollection.IsVisible = false");                    // 33
            }
            else
            {
                ListViewCollection.IsVisible = true;
                Debug.WriteLine("ListViewCollection.IsVisible = true");                     // 34
            }
        }

        private async void OnCouponsTapped(object sender, TappedEventArgs e)
        {
            Debug.WriteLine("OnCouponsTapped");                                             // 35
            await Navigation.PushAsync(new CategoriesDetails(9999));
        }
    }
}
