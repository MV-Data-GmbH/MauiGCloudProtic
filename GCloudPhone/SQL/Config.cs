using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCloudPhone
{
    public static class Config
    {
    public static string CategoryDisplay { get; set; } = "Compact"; // List, Grid, Compact
    public static string ShowFastOrder { get; set; } = "Yes"; // Yes, No
    public static string ShowDelivery { get; set; } = "Yes"; // Yes, No
    public static string ShowPickup { get; set; } = "Yes"; // Yes, No
    public static string ShowParking { get; set; } = "No"; // Yes, No
    public static string ProductPictureInBasket { get; set; } = "No"; // Yes, No
    public static string ShowPointsQuestion { get; set; } = "No"; // Yes, No
    public static string ShowPopupForSelect { get; set; } = "No"; // Yes, No
    public static string PaymentWithoutDataTransfer { get; set; } = "No"; // Yes, No
    public static string ReaderType { get; set; } = "NFC"; // Options: NFC, QR
    public static string ShowPopupForMultiplikation { get; set; } = "Yes";


    }

    public static class ParameterLoader
    {
    public static void LoadParameters(List<Parameters> parameters)
    {
        foreach (var param in parameters)
        {
            switch (param.Parameter)
            {
                case "CategoryDisplay":
                    Config.CategoryDisplay = param.Value;
                    break;
                case "ShowFastOrder":
                    Config.ShowFastOrder = param.Value;
                    break;
                case "ShowDelivery":
                    Config.ShowDelivery = param.Value;
                    break;
                case "ShowPickup":
                    Config.ShowPickup = param.Value;
                    break;
                case "ShowParking":
                    Config.ShowParking = param.Value;
                    break;
                case "ProductPictureInBasket":
                    Config.ProductPictureInBasket = param.Value;
                    break;
                case "ShowPointsQuestion":
                   Config.ShowPointsQuestion = param.Value;
                    break;
                case "ShowPopupForSelect":
                    Config.ShowPopupForSelect = param.Value;
                    break;
                case "PaymentWithoutDataTransfer":
                    Config.PaymentWithoutDataTransfer = param.Value;
                    break;
                case "ReaderType":
                    Config.ReaderType = param.Value;
                    break;
               case "ShowPopupForMultiplikation":
                    Config.ShowPopupForMultiplikation = param.Value;
                    break;
               default:
                    break;
            }
        }
    }
}
}