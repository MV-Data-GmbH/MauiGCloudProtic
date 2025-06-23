using GCloudShared.Repository;
using GCloudShared.Shared;
using GCloudShared.WebShopDto;
using Newtonsoft.Json;

namespace GCloudPhone.Views;

public partial class OrderDetails : ContentPage
{
    private OrderKuechenDisplay _order;

    public OrderDetails(OrderKuechenDisplay order)
	{
		InitializeComponent();
        _order = order;
      
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        BindOrderDetails(_order);
        FetchAndCompareOrderDetails();

    }

    private void BindOrderDetails(OrderKuechenDisplay order)
    {
        OrderNumberLabel.Text = order.Id.ToString();

        // Manually convert UTC to local time by adding 4 hours
        DateTime localTime = order.Date.AddHours(4);

        DateLabel.Text = localTime.ToString("g");
        OrderCommentLabel.Text = order.OrderComment ?? "/";

        OrderItemsCollectionView.ItemsSource = order.orderItemKuechenDisplays;
    }

    private void FetchAndCompareOrderDetails()
    {
        try
        {
            ParametersRepository pr = new ParametersRepository(DbBootstraper.Connection);
            var data = pr.FindByParameter("FastOrder");
            if (data != null)
            {
                var decompresorder = CompressString.DecompressString(data.ParameterValue);
                var fetchedOrder = JsonConvert.DeserializeObject<OrderKuechenDisplay>(decompresorder);
         
                if (fetchedOrder.Id == _order.Id)
                {
                    //QrCodeImage.Value = data.ParameterValue;
                    //QrCodeImage.IsVisible = true;
                    //QRCodeLabel.IsVisible = true;
                }
                else
                {
                   
                }
            }
            else
            {
                
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to fetch or compare order details: " + ex.Message);
        }
    }
}