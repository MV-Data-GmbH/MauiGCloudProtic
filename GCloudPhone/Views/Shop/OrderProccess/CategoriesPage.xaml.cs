using System.Collections.ObjectModel; // Omogućava upotrebu ObservableCollection koja automatski obaveštava UI o promenama u kolekciji
using GCloudShared.Interface; // Uključuje interfejse definisane u GCloudShared projektu (npr. IAuthService)
using System.Diagnostics; // Omogućava upotrebu Debug klase za ispis poruka tokom razvoja

namespace GCloudPhone.Views.Shop.OrderProccess
{
    // CategoriesPage predstavlja stranicu koja prikazuje kategorije proizvoda u Shop delu aplikacije
    public partial class CategoriesPage : ContentPage
    {
        // Privatni član koji čuva referencu na servis za autentifikaciju korisnika
        private IAuthService _authService;

        // Kolekcija koja sadrži podatke o kategorijama i koja se vezuje za UI (ListView, GridView, itd.)
        public ObservableCollection<CategoriesView> CategoryCollection { get; set; }

        // Kolekcija prodavnica koja se koristi na stranici; inicijalizovana praznom kolekcijom
        public ObservableCollection<Stores> Stores { get; set; } = new ObservableCollection<Stores>();

        // Podrazumevani konstruktor stranice
        public CategoriesPage()
        {
            InitializeComponent(); // Inicijalizuje komponente definisane u XAML fajlu

            // Inicijalizuje kolekciju kategorija
            CategoryCollection = new ObservableCollection<CategoriesView>();
            // Povezuje kolekciju kategorija sa ListView-om u UI, tako da se kategorije prikazuju
            ListViewCollection.ItemsSource = CategoryCollection;
            // Podesava način prikaza kategorija (npr. Grid ili List) na osnovu konfiguracije
            SetCategoryDisplay(Config.CategoryDisplay);
            // Poziv metode za učitavanje kategorija je ovde iskomentarisano
            //LoadCategoriesAsync();
            // Postavlja BindingContext stranice na App.SignalR 
            this.BindingContext = App.SignalR;
            // Postavlja tekst za label (FillialeLabel) na osnovu preferenci, sa podrazumevanom vrednošću "Unbekannte Filiale"
            
        }

        // Konstruktor koji prima IAuthService; poziva podrazumevani konstruktor i dodatno učitava kategorije
        public CategoriesPage(IAuthService authService) : this()
        {
            // Dodeljuje prosleđeni authService; baca ArgumentNullException ako je authService null
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            // Poziva asinhronu metodu za učitavanje kategorija iz baze
            //LoadCategoriesAsync();
        }

        // Metoda koja se poziva kada korisnik prevuče (swipe) desno na ekranu
        private void OnSwipedRight(object sender, SwipedEventArgs e)
        {
            // Vraća prethodnu stranicu u navigacionom stack-u
            Navigation.PopAsync();
        }

        // Metoda koja se poziva kada se stranica potpuno učita
        private async void OnLoaded(object sender, EventArgs e)
        {
           

            // Ako postoji barem jedan kupon u StartUpDataImport, prikazuje se UI element koji označava kupon
            if (StartUpDataImport.coupons.Count > 0)
            {
                CouponTaste.IsVisible = true;
            }

            // Briše sve prethodno učitane kategorije iz kolekcije
            CategoryCollection.Clear();
            // Učitava kategorije iz baze
            await LoadCategoriesAsync();
        }

        // Asinhrona metoda za učitavanje kategorija iz baze i njihovo dodavanje u kolekciju
        private async Task LoadCategoriesAsync()
        {
            try
            {
                // Poziva se metoda koja vraća sve vidljive kategorije iz baze
                var categories = await SQL.GetAllCategoriesAsync();

                // Iterira kroz svaku kategoriju
                foreach (var category in categories)
                {
                    ImageSource imageSource;
                    // Ako kategorija ima definisanu sliku, konvertuje Base64 string u ImageSource
                    if (!string.IsNullOrEmpty(category.Picturestring))
                    {
                        byte[] imageBytes = Convert.FromBase64String(category.Picturestring);
                        imageSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                    }
                    else
                    {
                        // Ako nema slike, koristi se podrazumevana slika "food.png"
                        imageSource = ImageSource.FromFile("food.png");
                    }

                    // Dodaje se nova instanca CategoriesView u kolekciju sa podacima o kategoriji
                    CategoryCollection.Add(new CategoriesView
                    {
                        Number = category.Number,
                        Description1 = category.Description1,
                        Image = imageSource,
                        IsSelected = false
                    });
                }
            }
            catch (Exception ex)
            {
                // U slučaju greške, ispisuje se poruka u debug prozoru
                Debug.WriteLine($"LoadCategoriesAsync error: {ex.Message}");
                // Prikazuje se dijalog o grešci korisniku
                await DisplayAlert("Error", "Failed to load categories. Please try again later.", "OK");
            }
        }

        // Metoda koja se poziva kada korisnik dodirne stavku kategorije
        private async void OnItemTapped(object sender, EventArgs e)
        {
            // Pretvara objekat koji je poslat (sender) u Grid
            var grid = sender as Grid;
            // Iz BindingContext-a Grid-a uzima se objekat kategorije (CategoriesView)
            var category = grid?.BindingContext as CategoriesView;
            if (category != null)
            {
                // Navigira se na stranicu sa detaljima kategorije, prosleđuje se broj kategorije
                await Navigation.PushAsync(new CategoriesDetails(category.Number));
            }
        }


        // Metoda koja podešava način prikaza kategorija (npr. lista ili grid) na osnovu prosleđenog parametra
        private void SetCategoryDisplay(string displayMode)
        {
            if (displayMode == "Grid")
            {
                // Ako je izabran grid prikaz, sakriva se ListView
                ListViewCollection.IsVisible = false;
               
            }
            else
            {
                // Inače, prikazuje se ListView
                ListViewCollection.IsVisible = true;
               
            }
        }

        // Metoda koja se poziva kada se dodirnu kuponi
        private async void OnCouponsTapped(object sender, TappedEventArgs e)
        {
            // Navigira se na stranicu sa detaljima kategorije sa ID-em 9999, što verovatno označava specijalnu kategoriju za kupone
            await Navigation.PushAsync(new CategoriesDetails(9999));
        }
    }
}
