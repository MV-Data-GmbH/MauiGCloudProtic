using CommunityToolkit.Maui.Views;

namespace GCloudPhone.Views.Shop.Checkout;

public partial class TipPopup : Popup
{
    public TipPopup()
    {
        InitializeComponent();
    }

    private void OnConfirmClicked(object sender, EventArgs e)
    {
        if (decimal.TryParse(TipAmountEntry.Text, out var tipAmount))
        {
            Close(tipAmount);
        }
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Close(null); 
    }
}