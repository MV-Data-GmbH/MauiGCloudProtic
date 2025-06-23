using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace GCloudPhone.Views.Shop
{
    public partial class DeliveryQuestionPopup : Popup
    {
        public DeliveryQuestionPopup()
        {
            InitializeComponent();
            InitializeRadioButtons();
        }

        private void InitializeRadioButtons()
        {
            // Dugme za dostavu (ako je potrebno)
            if (Config.ShowDelivery == "Yes")
            {
                var deliveryButton = new Button
                {
                    Text = "Lieferung",
                    Style = (Style)Application.Current.Resources["UnselectedButtonStyle"],
                    CommandParameter = "Delivery"
                };
                deliveryButton.Clicked += OnOptionButtonClicked;
                RadioButtonContainer.Children.Add(deliveryButton);
            }

            // Dugme za preuzimanje – odgovara Abholung.
            if (Config.ShowPickup == "Yes")
            {
                var pickupButton = new Button
                {
                    Text = "Abholung",
                    Style = (Style)Application.Current.Resources["UnselectedButtonStyle"],
                    CommandParameter = "PickUp"
                };
                pickupButton.Clicked += OnOptionButtonClicked;
                RadioButtonContainer.Children.Add(pickupButton);
            }

            // Dugme za "In der Filiale" – odgovara ponovljenom naručivanju u filijali.
            if (Config.ShowFastOrder == "Yes")
            {
                var dineInButton = new Button
                {
                    Text = "In der Filiale",
                    Style = (Style)Application.Current.Resources["UnselectedButtonStyle"],
                    CommandParameter = "DineIn"
                };
                dineInButton.Clicked += OnOptionButtonClicked;
                RadioButtonContainer.Children.Add(dineInButton);
            }

            // Dugme za parking – odgovara Auf dem Parkplatz.
            if (Config.ShowParking == "Yes")
            {
                var parkingButton = new Button
                {
                    Text = "Auf dem Parkplatz",
                    Style = (Style)Application.Current.Resources["UnselectedButtonStyle"],
                    CommandParameter = "Parking"
                };
                parkingButton.Clicked += OnOptionButtonClicked;
                RadioButtonContainer.Children.Add(parkingButton);
            }
        }

        private void OnOptionButtonClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            string selectedOrderType = button.CommandParameter.ToString();
            App.OrderType = selectedOrderType;
            // Odmah zatvori popup i vrati izabranu metodu dostave.
            Close(selectedOrderType);
        }
    }
}
