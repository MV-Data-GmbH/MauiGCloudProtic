using CommunityToolkit.Mvvm.DependencyInjection;
using System.Linq.Expressions;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic;
using GCloudPhone.Views.Templates;

namespace GCloudPhone.Views.Shop.OrderProccess;

public partial class ProductDetails : ContentPage
{
    private int _productNumber;
    private ProductsView _product;
    private int _quantity = 1;
    private decimal _price = 0;
    private List<SelectedSideDish> _selectedSideDishes = new List<SelectedSideDish>();
    private int _currentPage = 1;
    private int _currentGroup = 1;
    private int _totalPages = 5;

    public ObservableCollection<SidedishesView> ProductSideDishes { get; set; } = new ObservableCollection<SidedishesView>();
    public ObservableCollection<SidedishesView> CurrentPageSideDishes { get; set; } = new ObservableCollection<SidedishesView>();

    public ICommand SelectSideDishCommand { get; }
    public ICommand IncreaseQuantityCommand { get; }
    public ICommand DecreaseQuantityCommand { get; }
    public ICommand PreviousPageCommand { get; }
    public ICommand NextPageCommand { get; }

    public bool CanNavigatePrevious => _currentPage > 1;
    public bool CanNavigateNext => _currentPage < _totalPages;
    public bool ShowSideDishesLabel => ProductSideDishes.Any();
    public bool IsFirstPage => _currentPage == ProductSideDishes.Select(sd => sd.PageNumber).Distinct().OrderBy(p => p).FirstOrDefault();
    public bool IsLastPage => _currentPage == ProductSideDishes.Select(sd => sd.PageNumber).Distinct().OrderBy(p => p).LastOrDefault();
    private static List<CollectionView> collectionViews = null;

    public ProductDetails(ProductsView product)
    {
        InitializeComponent();
        _productNumber = product.Number;
        _product = product;

        SelectSideDishCommand = new Command<SidedishesView>(SelectSideDish);
        IncreaseQuantityCommand = new Command<SidedishesView>(IncreaseQuantity);
        DecreaseQuantityCommand = new Command<SidedishesView>(DecreaseQuantity);

        collectionViews = new List<CollectionView> {
          SideDishesCollectionView1,
          SideDishesCollectionView2,
          SideDishesCollectionView3,
          SideDishesCollectionView4};

        LoadProductDetailsAsync();
        Cart.Instance.ItemCountChanged += OnItemCountChanged;

        // Kombiniertes ViewModel setzen
        BindingContext = new CombinedViewModel(this, App.SignalR);
    }

    public void OnItemCountChanged()
    {
        var cartTemplate = Application.Current.MainPage.FindByName<CartTemplate>("CartTemplate");
        cartTemplate?.UpdateItemCount();
    }

    private async void LoadProductDetailsAsync()
    {
        if (_product != null)
        {
            ProductNameLabel.Text = _product.Description1;
            _price = _product.PriceAmount;
            UpdateAddToCartButton();
            await LoadSideDishesAsync(_product.Number);
        }
    }

    private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
    {
        string result = await DisplayPromptAsync("Zusätzliche Informationen zum Produkt", "Geben Sie zusätzliche Informationen oder Wünsche für Ihre Bestellung ein:", "OK", "Abbrechen", placeholder: "Ihre Nachricht", maxLength: 250, keyboard: Keyboard.Text);
        if (!string.IsNullOrEmpty(result))
        {
            //currentOrderItem.ItemNote = result;
        }
    }

    private async Task LoadSideDishesAsync(int productNumber)
    {
        try
        {
            var sideDishes = await SQL.GetSidedishesByProductIdAsync(productNumber);
            if (!sideDishes.Any())
            {
                PreviousPageButton.IsVisible = false;
                NextPageButton.IsVisible = false;
                return;
            }

            ProductSideDishes.Clear();

            foreach (var sideDish in sideDishes)
            {
                ProductSideDishes.Add(new SidedishesView
                {
                    Number = sideDish.Number,
                    Description1 = sideDish.Description1,
                    PriceAmount = sideDish.PriceAmount,
                    Picturestring = sideDish.Picturestring,
                    PageNumber = sideDish.PageNumber,
                    Group = sideDish.Group == 6 ? 1 : sideDish.Group,
                    Quantity = 1,
                    VAT = sideDish.VAT
                });
            }

            _selectedSideDishes.Clear();
            _totalPages = ProductSideDishes.Select(sd => sd.PageNumber).Distinct().Count();
            _currentPage = ProductSideDishes.Select(sd => sd.PageNumber).Distinct().OrderBy(p => p).First();
            LoadCurrentPageSideDishes();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in LoadSideDishesAsync: {ex.Message}");
        }
    }

    private void LoadCurrentPageSideDishes()
    {
        // Hole alle Gerichte der aktuellen Seite
        var currentPageSideDishes = ProductSideDishes
            .Where(sd => sd.PageNumber == _currentPage)
            .ToList();

        // Prüfe, ob es Gerichte auf der aktuellen Seite gibt
        if (currentPageSideDishes.Any())
        {
            // Setze `_currentGroup` basierend auf der Gruppe der ersten Beilage auf der aktuellen Seite
            _currentGroup = currentPageSideDishes.First().Group;
        }

        // Bereinige die aktuelle Seite für die CollectionView
        CurrentPageSideDishes.Clear();
        foreach (var sideDish in currentPageSideDishes)
        {
            CurrentPageSideDishes.Add(sideDish);
        }

        // Blende alle CollectionViews aus
        foreach (var collectionView in collectionViews)
        {
            collectionView.IsVisible = false;
        }

        // Wähle die CollectionView basierend auf `_currentGroup`
        if (_currentGroup >= 1 && _currentGroup <= collectionViews.Count)
        {
            var collectionView = collectionViews[_currentGroup - 1];
            collectionView.ItemsSource = CurrentPageSideDishes;
            collectionView.IsVisible = true;
        }

      
        UpdateAddToCartButton();
    }


  


    private void SelectSideDish(SidedishesView sideDish)
    {
        if (sideDish.Group == 1 || sideDish.Group == 6)
        {
            // Deselect all previously selected side dishes in PageNumber 1
            foreach (var selectedSideDish in _selectedSideDishes.Where(sd => sd.Group == 1).ToList())
            {
                var sideDishToDeselect = ProductSideDishes.FirstOrDefault(sd => sd.Number == selectedSideDish.Number && sd.PageNumber == sideDish.PageNumber && sd.Group == selectedSideDish.Group);
                if (sideDishToDeselect != null)
                {
                    sideDishToDeselect.IsSelected = false;
                }
                _selectedSideDishes.Remove(selectedSideDish);
            }
        }

        sideDish.IsSelected = !sideDish.IsSelected;

        if (sideDish.IsSelected)
        {
            _selectedSideDishes.Add(new SelectedSideDish { Number = sideDish.Number, PageNumber = sideDish.PageNumber, Group=sideDish.Group });
        }
        else
        {
            var itemToRemove = _selectedSideDishes.FirstOrDefault(sd => sd.Number == sideDish.Number && sd.PageNumber == sideDish.PageNumber && sd.Group == sideDish.Group);
            if (itemToRemove != null)
            {
                _selectedSideDishes.Remove(itemToRemove);
            }
        }

        UpdateAddToCartButton();
    }

    private void IncreaseQuantity(SidedishesView sideDish)
    {
        sideDish.Quantity++;
        UpdateAddToCartButton();
    }

    private void DecreaseQuantity(SidedishesView sideDish)
    {
        if (sideDish.Quantity > 1)
        {
            sideDish.Quantity--;
            UpdateAddToCartButton();
        }
    }

    private decimal GetTotal()
    {
        decimal totalPrice = _price * _quantity;

        foreach (var selectedSideDish in _selectedSideDishes)
        {
            var sideDish = ProductSideDishes.FirstOrDefault(sd => sd.Number == selectedSideDish.Number && sd.PageNumber == selectedSideDish.PageNumber);
            if (sideDish != null)
            {
                totalPrice += sideDish.PriceAmount * sideDish.Quantity;
            }
        }

        return totalPrice;
    }

    private void UpdateAddToCartButton()
    {
        decimal totalPrice = GetTotal();

        AddToCartButton.Text = !IsLastPage ? $"Ausgewählt: {totalPrice.ToString("C", new System.Globalization.CultureInfo("de-DE"))}" :
            $"In den Warenkorb: {totalPrice.ToString("C", new System.Globalization.CultureInfo("de-DE"))}";
    }

    private async void OnAddToCartButtonClicked(object sender, EventArgs e)
    {
        if (!IsLastPage)
        {
            NextPageButton_Clicked(null, null);
            return;
        }

        var orderItem = new OrderItemViewModel
        {
            ProductID = _product.Number,
            ProductDescription1 = _product.Description1,
            ProductDescription2 = _product.IsCoupon ? $"Coupon Value: {_product.CouponValue}" : null,
            Amount = _price,
            Quantity = _quantity,
            VAT = _product.VAT,
        };

        int productIdc = Cart.Instance.AddItem(orderItem);

        foreach (var selectedSideDish in _selectedSideDishes)
        {
            var sideDish = ProductSideDishes.FirstOrDefault(sd => sd.Number == selectedSideDish.Number && sd.PageNumber == selectedSideDish.PageNumber);
            if (sideDish != null)
            {
                var sideDishOrderItem = new OrderItemViewModel
                {
                    ProductID = sideDish.Number,
                    ProductDescription1 = sideDish.Description1,
                    Amount = sideDish.PriceAmount,
                    Quantity = sideDish.Quantity,
                    //Reference = _product.Number.ToString(),
                    Reference = productIdc.ToString(),
                    VAT = _product.VAT,
                };

                Cart.Instance.AddItem(sideDishOrderItem);
            }
        }

        await Navigation.PushAsync(new CategoriesPage());
    }

    private async void NextPageButton_Clicked(object sender, EventArgs e)
    {
        if (_currentGroup == 1 && !CurrentPageSideDishes.Any(sd => sd.IsSelected))
        {
            await DisplayAlert("Warnung", "Eine Entscheidung muss getroffen werden", "OK");
            return;
        }
        if(IsLastPage)
        {
            OnAddToCartButtonClicked(null, null);
            return;
        }
        var availablePages = ProductSideDishes.Select(sd => sd.PageNumber).Distinct().OrderBy(p => p).ToList();
        var nextPage = availablePages.FirstOrDefault(p => p > _currentPage);
        if (nextPage != 0)
        {
            _currentPage = nextPage;
            LoadCurrentPageSideDishes();
        }
    }

    private void PreviousPageButton_Clicked(object sender, EventArgs e)
    {
        var availablePages = ProductSideDishes.Select(sd => sd.PageNumber).Distinct().OrderBy(p => p).ToList();
        var previousPage = availablePages.LastOrDefault(p => p < _currentPage);
        if (previousPage != 0)
        {
            _currentPage = previousPage;
            LoadCurrentPageSideDishes();
        }
        else
        {
            OnBackButtonClicked(null, null);
        }
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        if (IsFirstPage)
        {
            bool answer = await DisplayAlert("Produkt löschen?", $"Produkt {ProductNameLabel.Text} löschen?", "Ja", "Nein");
            if (answer)
            {
                await Navigation.PopAsync();
            }
        }
        else
        {
            await Navigation.PopAsync();
        }
    }
}
public class CombinedViewModel : INotifyPropertyChanged
{
    public ProductDetails ProductDetailsContext { get; set; }
    public SignalRClient SignalRContext { get; set; } // Assuming App.SignalR is of type SignalRContext

    public CombinedViewModel(ProductDetails productDetailsContext, SignalRClient signalRContext)
    {
        ProductDetailsContext = productDetailsContext;
        SignalRContext = signalRContext;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
