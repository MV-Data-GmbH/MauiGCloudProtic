using System.ComponentModel; // Omogućava implementaciju INotifyPropertyChanged za obaveštavanje UI-a o promenama u svojstvima
using System.Runtime.CompilerServices; // Omogućava korišćenje [CallerMemberName] atributa za jednostavnije obaveštavanje o promenama
using System.Collections.ObjectModel; // Omogućava upotrebu ObservableCollection koja automatski obaveštava UI o promenama u kolekcijama
using Microsoft.Maui.Controls; // MAUI kontrole i tipovi (npr. ImageSource)

namespace GCloudPhone
{
    // Klasa koja predstavlja pogled (view) dodatnih priloga (side dishes) i implementira INotifyPropertyChanged za binding u UI-u
    public class SidedishesView : INotifyPropertyChanged
    {
        private bool _isSelected; // Privatno polje za označavanje selekcije
        private int _quantity; // Privatno polje za količinu

        public int Number { get; set; } // Jedinstveni identifikator priloga
        public string Description1 { get; set; } // Opis priloga
        public decimal PriceAmount { get; set; } // Cena priloga
        public string Picturestring { get; set; } // Slika priloga kao string (najverovatnije Base64)
        public int? Categories { get; set; } // ID kategorije, opcioni
        public int PageNumber { get; set; } // Broj stranice (npr. za paginaciju)
        public decimal VAT { get; set; } // PDV (Value Added Tax)
        public int Group { get; set; } // Grupa kojoj pripada prilog
                                       // NOVO: Svojstva za cene prema različitim metodama dostave
        public decimal PriceDelivery { get; set; }
        public decimal PricePickUp { get; set; }
        public decimal PriceDineIn { get; set; }
        public decimal PriceParking { get; set; }

        // Svojstvo za količinu; pri promeni, obaveštava UI o promeni
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                }
            }
        }

        // Svojstvo koje označava da li je prilog selektovan
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // Metoda koja obaveštava UI da se promenilo određeno svojstvo
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Klasa koja predstavlja selektovani prilog (side dish) sa osnovnim informacijama
    public class SelectedSideDish
    {
        public int Number { get; set; } // ID priloga
        public int PageNumber { get; set; } // Broj stranice
        public int Group { get; set; } // Grupa kojoj pripada
    }

    // Klasa koja predstavlja prikaz proizvoda sa svojim svojstvima i podrškom za promene (binding)
    public class ProductsView : INotifyPropertyChanged
    {
        public int Number { get; set; } // Jedinstveni identifikator proizvoda
        public string Description1 { get; set; } // Primarni opis proizvoda
        public string Description2 { get; set; } // Sekundarni opis proizvoda
        public int? Categories { get; set; } // ID kategorije, opcioni
        public string Picturestring { get; set; } // Slika proizvoda kao string (Base64)
        public decimal PriceAmount { get; set; } // Cena proizvoda
        public ImageSource Image { get; set; } // Slika proizvoda kao ImageSource
        public decimal VAT { get; set; } // PDV za proizvod

        private int _quantity; // Privatno polje za količinu proizvoda

        // Svojstvo za količinu; pri promeni, obaveštava UI i sakriva kontrole za količinu ako je količina 0
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    // Ako je količina 0, sakriva se prikaz kontrola za količinu
                    if (_quantity == 0)
                    {
                        ShowQuantityControls = false;
                    }
                    OnPropertyChanged();
                }
            }
        }

        private bool _isSelected; // Privatno polje za selekciju

        // Svojstvo koje označava da li je proizvod selektovan
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isHighlighted; // Privatno polje za označavanje isticanja (highlight)

        // Svojstvo koje označava da li je proizvod istaknut (highlighted)
        public bool IsHighlighted
        {
            get => _isHighlighted;
            set
            {
                if (_isHighlighted != value)
                {
                    _isHighlighted = value;
                    OnPropertyChanged();
                }
            }
        }

        // Privatno polje koje označava da li je proizvod deo kupona
        public bool _isCoupon;
        public bool IsCoupon
        {
            get => _isCoupon;
            set
            {
                if (_isCoupon != value)
                {
                    _isCoupon = value;
                    OnPropertyChanged();
                }
            }
        }

        private decimal _couponValue; // Privatno polje za vrednost kupona

        // Svojstvo za vrednost kupona; obaveštava UI pri promeni
        public decimal CouponValue
        {
            get => _couponValue;
            set
            {
                if (_couponValue != value)
                {
                    _couponValue = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _showQuantityControls; // Privatno polje koje određuje vidljivost kontrola za količinu

        // Svojstvo koje određuje da li da se prikazuju kontrole za količinu
        public bool ShowQuantityControls
        {
            get => _showQuantityControls;
            set
            {
                if (_showQuantityControls != value)
                {
                    _showQuantityControls = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // Metoda koja obaveštava UI o promeni svojstva
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Klasa koja predstavlja prikaz kategorije, uključujući sliku i mogućnost promene veličine (scale)
    public class CategoriesView : INotifyPropertyChanged
    {
        public int Number { get; set; } // ID kategorije
        public string Description1 { get; set; } // Naziv ili opis kategorije
        public string Picturestring { get; set; } // Slika kategorije kao string (Base64)
        public ImageSource Image { get; set; } // Slika kategorije kao ImageSource

        private double _scale = 1.0; // Privatno polje za skaliranje (po defaultu 1.0)

        // Svojstvo koje predstavlja skaliranje slike ili prikaza kategorije
        public double Scale
        {
            get => _scale;
            set
            {
                if (_scale != value)
                {
                    _scale = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isSelected; // Privatno polje koje označava selekciju kategorije

        // Svojstvo koje označava da li je kategorija selektovana
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // Metoda za obaveštavanje UI-a o promenama svojstava
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Klasa koja predstavlja stavku porudžbine i omogućava binding podataka za UI
    public class OrderItemViewModel : INotifyPropertyChanged
    {
        private int? _quantity; // Privatno polje za količinu, nullable

        public int Idc { get; set; } // Interni ID stavke
        public string OrderID { get; set; } // ID porudžbine kojoj pripada stavka
        public int ProductID { get; set; } // ID proizvoda
        public string ProductDescription1 { get; set; } // Prvi opis proizvoda
        public string ProductDescription2 { get; set; } // Drugi opis proizvoda
        public decimal? Amount { get; set; } // Cena stavke, nullable

        public bool IsCoupon { get; set; } // Označava da li je stavka deo kupona
        public decimal CouponValue { get; set; } // Vrednost kupona

        public string ItemNote { get; set; } // Beleška ili napomena za stavku

        // Količina stavke; pri promeni, obaveštava UI
        public int? Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                }
            }
        }
        public decimal? VAT { get; set; } // PDV za stavku, nullable
        public string Reference { get; set; } // Referenca (npr. kod proizvoda)
        public ImageSource ImageSource { get; set; } // Slika proizvoda

        // Kolekcija priloga koji su vezani za ovu stavku porudžbine
        public ObservableCollection<OrderItemViewModel> SideDishes { get; set; } = new ObservableCollection<OrderItemViewModel>();

        private bool _showImage; // Privatno polje koje određuje vidljivost slike

        // Svojstvo koje određuje da li se slika prikazuje
        public bool ShowImage
        {
            get => _showImage;
            set
            {
                _showImage = value;
                OnPropertyChanged();
            }
        }

        private Thickness _margin; // Privatno polje za margine (razmak) u UI prikazu

        // Svojstvo koje predstavlja marginu; omogućava dinamičko podešavanje razmaka
        public Thickness Margin
        {
            get => _margin;
            set
            {
                if (_margin != value)
                {
                    _margin = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // Metoda koja obaveštava UI o promeni svojstva
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Konstruktor, podešava početnu marginu na osnovu konfiguracije
        public OrderItemViewModel()
        {
            Margin = Config.ProductPictureInBasket == "Yes" ? new Thickness(20, 0, 0, 0) : new Thickness(0, 0, 0, 0);
        }

        // Metoda koja konvertuje OrderItemViewModel u OrderItems model (za čuvanje u bazi)
        public OrderItems ToOrderItem()
        {
            return new OrderItems
            {
                Idc = this.Idc,
                OrderID = this.OrderID,
                ProductID = this.ProductID,
                ProductDescription1 = this.ProductDescription1,
                ProductDescription2 = this.ProductDescription2,
                Amount = this.Amount,
                Quantity = this.Quantity,
                VAT = this.VAT,
                Reference = this.Reference,
                IsCoupon = this.IsCoupon,      // Uključuje zastavicu kupona
                CouponValue = this.CouponValue   // Uključuje vrednost kupona
            };
        }
    }

    // ViewModel koji predstavlja celu porudžbinu zajedno sa stavkama i informacijama za dostavu i plaćanje
    public class OrderViewModel : INotifyPropertyChanged
    {
        private string _deliveryAddress; // Privatno polje za adresu dostave
        private string _deliveryContact; // Privatno polje za kontakt osobu za dostavu
        private string _deliveryTime; // Privatno polje za vreme dostave
        private string _deliveryNotes; // Privatno polje za beleške vezane za dostavu
        private string _paymentMethod; // Privatno polje za način plaćanja
        private decimal _totalPrice; // Privatno polje za ukupnu cenu porudžbine
        private decimal _itemsTotal; // Privatno polje za ukupnu cenu stavki
        private decimal _deliveryFee; // Privatno polje za cenu dostave
        private decimal _vat; // Privatno polje za ukupni PDV
        private decimal? _tip; // Privatno polje za napojnicu, nullable
        private DateTime _selectedDate = DateTime.Now; // Privatno polje za odabrani datum (default je sada)
        private DateTime _minimumDate; // Privatno polje za minimalni dozvoljeni datum

        public event PropertyChangedEventHandler PropertyChanged;

        // Kolekcija stavki porudžbine
        public ObservableCollection<OrderItemViewModel> CartItems { get; set; }

        // Svojstvo za adresu dostave
        public string DeliveryAddress
        {
            get => _deliveryAddress;
            set
            {
                if (_deliveryAddress != value)
                {
                    _deliveryAddress = value;
                    OnPropertyChanged();
                }
            }
        }

        // Svojstvo za kontakt dostave
        public string DeliveryContact
        {
            get => _deliveryContact;
            set
            {
                if (_deliveryContact != value)
                {
                    _deliveryContact = value;
                    OnPropertyChanged();
                }
            }
        }

        // Svojstvo za vreme dostave
        public string DeliveryTime
        {
            get => _deliveryTime;
            set
            {
                if (_deliveryTime != value)
                {
                    _deliveryTime = value;
                    OnPropertyChanged();
                }
            }
        }

        // Svojstvo za beleške o dostavi
        public string DeliveryNotes
        {
            get => _deliveryNotes;
            set
            {
                if (_deliveryNotes != value)
                {
                    _deliveryNotes = value;
                    OnPropertyChanged();
                }
            }
        }

        // Svojstvo za način plaćanja
        public string PaymentMethod
        {
            get => _paymentMethod;
            set
            {
                if (_paymentMethod != value)
                {
                    _paymentMethod = value;
                    OnPropertyChanged();
                }
            }
        }

        // Svojstvo za ukupnu cenu porudžbine
        public decimal TotalPrice
        {
            get => _totalPrice;
            set
            {
                if (_totalPrice != value)
                {
                    _totalPrice = value;
                    OnPropertyChanged();
                }
            }
        }

        // Svojstvo za ukupnu cenu stavki
        public decimal ItemsTotal
        {
            get => _itemsTotal;
            set
            {
                if (_itemsTotal != value)
                {
                    _itemsTotal = value;
                    OnPropertyChanged();
                }
            }
        }

        // Svojstvo za cenu dostave
        public decimal DeliveryFee
        {
            get => _deliveryFee;
            set
            {
                if (_deliveryFee != value)
                {
                    _deliveryFee = value;
                    OnPropertyChanged();
                }
            }
        }

        // Svojstvo za ukupni PDV
        public decimal VAT
        {
            get => _vat;
            set
            {
                if (_vat != value)
                {
                    _vat = value;
                    OnPropertyChanged();
                }
            }
        }

        // Svojstvo za napojnicu, nullable
        public decimal? Tip
        {
            get => _tip;
            set
            {
                if (_tip != value)
                {
                    _tip = value;
                    OnPropertyChanged();
                }
            }
        }

        // Svojstvo za odabrani datum dostave
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate != value)
                {
                    _selectedDate = value;
                    OnPropertyChanged();
                }
            }
        }

        // Svojstvo za minimalni dozvoljeni datum
        public DateTime MinimumDate
        {
            get => _minimumDate;
            set
            {
                if (_minimumDate != value)
                {
                    _minimumDate = value;
                    OnPropertyChanged();
                }
            }
        }

        // Konstruktor inicijalizuje kolekciju stavki porudžbine
        public OrderViewModel()
        {
            CartItems = new ObservableCollection<OrderItemViewModel>();
        }

        // Metoda za obaveštavanje UI-a o promenama svojstava
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // ViewModel za korpu koji sadrži logiku za proračunavanje cena, PDV-a i ukupnog iznosa
    public class CartViewModel : INotifyPropertyChanged
    {
        private decimal itemsTotal; // Privatno polje za ukupnu cenu stavki
        private decimal deliveryFee = 1.00m; // Privatno polje za cenu dostave (podrazumevano 1.00)
        private decimal vat; // Privatno polje za PDV

        // Kolekcija stavki u korpi
        public ObservableCollection<OrderItemViewModel> CartItems { get; set; }

        // Svojstvo za ukupnu cenu stavki; pri promeni, ažurira PDV i ukupnu cenu
        public decimal ItemsTotal
        {
            get => itemsTotal;
            set
            {
                if (itemsTotal != value)
                {
                    itemsTotal = value;
                    OnPropertyChanged();
                    UpdateVAT();
                    UpdateTotal();
                }
            }
        }

        // Svojstvo za cenu dostave; pri promeni, ažurira ukupnu cenu
        public decimal DeliveryFee
        {
            get => deliveryFee;
            set
            {
                if (deliveryFee != value)
                {
                    deliveryFee = value;
                    OnPropertyChanged();
                    UpdateTotal();
                }
            }
        }

        // Svojstvo za PDV
        public decimal VAT
        {
            get => vat;
            set
            {
                if (vat != value)
                {
                    vat = value;
                    OnPropertyChanged();
                }
            }
        }

        private decimal total; // Privatno polje za ukupnu cenu

        // Svojstvo za ukupnu cenu korpe; postavljeno kao privatno setovanje, jer se proračunava interno
        public decimal Total
        {
            get => total;
            private set
            {
                if (total != value)
                {
                    total = value;
                    OnPropertyChanged();
                }
            }
        }

        // Konstruktor inicijalizuje kolekciju stavki i preračunava cenu kada se kolekcija promeni
        public CartViewModel()
        {
            CartItems = new ObservableCollection<OrderItemViewModel>();
            CartItems.CollectionChanged += (sender, e) => CalculateItemsTotal();
            // Primer dodavanja stavke u korpu (može se ukloniti u produkciji)
            CartItems.Add(new OrderItemViewModel { Quantity = 1, Amount = 1.00m });
        }

        // Metoda koja preračunava ukupnu cenu stavki u korpi
        private void CalculateItemsTotal()
        {
            ItemsTotal = CartItems.Sum(item => (item.Quantity ?? 0) * (item.Amount ?? 0));
        }

        // Metoda koja preračunava PDV kao 19% od ukupne cene stavki
        private void UpdateVAT()
        {
            VAT = ItemsTotal * 0.19m;
        }

        // Metoda koja preračunava ukupnu cenu (stavke + dostava + PDV)
        private void UpdateTotal()
        {
            Total = ItemsTotal + DeliveryFee + VAT;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        // Metoda za obaveštavanje UI-a o promenama svojstava
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // ViewModel koji predstavlja porudžbinu zajedno sa stavkama, slikom prvog proizvoda i vidljivošću stavki
    public class OrderWithItemsViewModel : INotifyPropertyChanged
    {
        public Orders Order { get; set; } // Podaci o porudžbini
        public List<OrderItems> OrderItems { get; set; } // Lista stavki porudžbine
        public string FirstProductImage { get; set; } // Slika prvog proizvoda kao string (Base64)
        public ImageSource FirstProductImageSource { get; set; } // Prikaz slike prvog proizvoda kao ImageSource

        private bool _isOrderItemsVisible; // Privatno polje koje određuje vidljivost stavki porudžbine

        // Svojstvo koje označava da li su stavke porudžbine vidljive; pri promeni, obaveštava UI i ažurira visinu i ikonicu ekspanzije
        public bool IsOrderItemsVisible
        {
            get => _isOrderItemsVisible;
            set
            {
                if (_isOrderItemsVisible != value)
                {
                    _isOrderItemsVisible = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(OrderItemsHeight));
                    OnPropertyChanged(nameof(ExpanderIcon));
                }
            }
        }

        private double _orderItemsHeight; // Privatno polje za visinu prikaza stavki

        // Svojstvo koje računa visinu prikaza stavki porudžbine
        public double OrderItemsHeight
        {
            get
            {
                double parentItemHeight = 80; // Primer visine glavne stavke (može se prilagoditi)
                if (IsOrderItemsVisible)
                {
                    return parentItemHeight + (OrderItems.Count * 40);
                }
                else
                {
                    return parentItemHeight;
                }
            }
            set
            {
                if (_orderItemsHeight != value)
                {
                    _orderItemsHeight = value;
                    OnPropertyChanged();
                }
            }
        }

        // Svojstvo koje vraća ikonicu ekspanzije – strelica gore ako su stavke vidljive, strelica dole inače
        public string ExpanderIcon => IsOrderItemsVisible ? "arrowup.png" : "arrowdown.png";

        public event PropertyChangedEventHandler PropertyChanged;

        // Metoda za obaveštavanje UI-a o promenama svojstava
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
