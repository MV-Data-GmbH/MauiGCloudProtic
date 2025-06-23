using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Shared;

namespace GCloudPhone.Views.Settings.MyAccount;

public partial class AddAddressPage : ContentPage
{
    private readonly Addresses _address;
    public string City { get; set; }
    public string AddressLine1 { get; set; } 
    public string AddressLine2 { get; set; } 
    public string Zip { get; set; }

    // Properties to keep track of selected address type
    public string SelectedAddressType { get; set; }
    public Color HomeFrameColor { get; set; } = Colors.Gray;
    public Color WorkFrameColor { get; set; } = Colors.Gray;
    public Color OtherFrameColor { get; set; } = Colors.Gray;

    public Command<string> SelectAddressTypeCommand { get; }

    public AddAddressPage(Addresses address = null)
    {
        InitializeComponent();

        _address = address;

        SelectAddressTypeCommand = new Command<string>(OnSelectAddressType);

        if (_address != null)
        {
            Title = "Adresse bearbeiten"; // Change title for editing
            City = _address.City;
            AddressLine1 = _address.AddressLine1;
            AddressLine2 = _address.AddressLine2;
            Zip = _address.Zip;
            SelectedAddressType = _address.AddressType;
            SetAddressTypeColors();
        }
        else
        {
            Title = "Neue Adresse hinzufügen";
        }

        BindingContext = this;
    }

    private void SetAddressTypeColors()
    {
        // Set colors based on the selected address type for editing
        HomeFrameColor = SelectedAddressType == "Home" ? (Color)Application.Current.Resources["SelectedColor"] : (Color)Application.Current.Resources["UnselectedColor"];
        WorkFrameColor = SelectedAddressType == "Work" ? (Color)Application.Current.Resources["SelectedColor"] : (Color)Application.Current.Resources["UnselectedColor"];
        OtherFrameColor = SelectedAddressType == "Other" ? (Color)Application.Current.Resources["SelectedColor"] : (Color)Application.Current.Resources["UnselectedColor"];

        OnPropertyChanged(nameof(HomeFrameColor));
        OnPropertyChanged(nameof(WorkFrameColor));
        OnPropertyChanged(nameof(OtherFrameColor));
    }

   
    private void OnSelectAddressType(string addressType)
    {
        SelectedAddressType = addressType;
        SetAddressTypeColors();
    }


    private async void OnSaveButtonClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(City) || string.IsNullOrWhiteSpace(AddressLine1) || string.IsNullOrWhiteSpace(Zip))
        {
            await DisplayAlert("Validierungsfehler", "Bitte füllen Sie alle erforderlichen Felder aus.", "OK");
            return;
        }

        UserRepository ur = new UserRepository(DbBootstraper.Connection);
        var user = ur.GetCurrentUser();

        var address = _address ?? new Addresses
        {
            UserID = user.UserId,
            RemoteID = _address?.RemoteID ?? Guid.NewGuid().ToString(), // Use existing ID if editing
            Name = user.FirstName,
            SurName = user.LastName,
            Country = "Austria",
            Email = user.Email,
            IsDefault = 0
        };

        // Update properties with the new/edited values
        address.City = City;
        address.AddressLine1 = AddressLine1;
        address.AddressLine2 = AddressLine2;
        address.Zip = Zip;
        address.AddressType = SelectedAddressType;

        UserAddressService userAddressService = new UserAddressService();

        string result;
        if (_address != null)
        {
            result = await userAddressService.UpdateAddress(address);
        }
        else
        {
            //await SQL.SaveItemAsync(address);
            result = await userAddressService.AddUserAddress(address);
        }

        if (result.Contains("Address Added.") || result.Contains("Address Updated."))
        {
            await DisplayAlert("Erfolg", "Die Adresse wurde erfolgreich gespeichert.", "OK");
            await Navigation.PopAsync(); 
        }
        else
        {
            await DisplayAlert("Fehler", "Beim Speichern der Adresse ist ein Problem aufgetreten. Bitte versuchen Sie es erneut.", "OK");
        }
    }
}
