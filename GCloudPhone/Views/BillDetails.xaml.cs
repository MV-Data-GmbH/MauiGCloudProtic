using CommunityToolkit.Mvvm.Input;
using GCloud.Shared.Dto.Domain;
using GCloudShared.Interface;
using GCloudShared.Service;
using iTextSharp.text;
using iTextSharp.text.pdf;
using QRCoder;
using GCloudPhone.Domain;

namespace GCloudPhone.Views;

public partial class BillDetails : ContentPage
{
	public IBillService BillService { get; set; }
    public IAuthService _authService { get; set; }
    private Guid _id;
    //ObservableCollection<TaxShowClass> taxes=new ObservableCollection<TaxShowClass>();
    public BillDetails(Guid Id, IAuthService authService)
	{
		InitializeComponent();
		BillService = new BillService();
        _authService = authService;
        _id = Id;
        ShowBill(Id);



    }
	private async void ShowBill(Guid id)
	{
        var result = await BillService.GetById(id);
        var invoice = result as Bill_Out_Dto;
        if (invoice != null)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            if (invoice.Invoice.JwsSignature != null)
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(invoice.Invoice.JwsSignature, QRCodeGenerator.ECCLevel.L);
                PngByteQRCode qRCode = new PngByteQRCode(qrCodeData);
                byte[] qrCodeBytes = qRCode.GetGraphic(50);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ImageSource qrImageSource = ImageSource.FromStream(() => new MemoryStream(qrCodeBytes));
                    QRCode.Source = qrImageSource;
                    QRCode.MaximumHeightRequest = 200;
                    QRCode.MaximumWidthRequest = 200;
                });
            }
            await Task.Delay(1000);
            String BillForPrint = "", Town = "", CompanyName = "", StreetAdreess = "", ZipCode = "";
            int numspaceLeft, numspaceRight = 0;
            string Dash = "------------------------------------------\n";
            int NumberOfCharacter = Dash.Length;
            var numberSpaceCharacterTown = NumberOfCharacter - invoice.Invoice.Biller.Address.Town.Length;
            numspaceLeft = (int)Math.Ceiling((decimal)(numberSpaceCharacterTown / 2));
            numspaceRight = (int)Math.Floor((decimal)(numberSpaceCharacterTown / 2));
            Town = invoice.Invoice.Biller.Address.Town.PadLeft(invoice.Invoice.Biller.Address.Town.Length + numspaceLeft).PadRight(invoice.Invoice.Biller.Address.Town.Length + numspaceRight);
            var numberSpaceCharacterComanyName = NumberOfCharacter - invoice.Invoice.Biller.ComanyName.Length;
            numspaceLeft = (int)Math.Ceiling((decimal)(numberSpaceCharacterComanyName / 2));
            numspaceRight = (int)Math.Floor((decimal)(numberSpaceCharacterComanyName / 2));
            CompanyName = invoice.Invoice.Biller.ComanyName.PadLeft(invoice.Invoice.Biller.ComanyName.Length + numspaceLeft).PadRight(invoice.Invoice.Biller.ComanyName.Length + numspaceRight);
            var numberSpaceCharacterStreet = NumberOfCharacter - invoice.Invoice.Biller.Address.Street.Length;
            numspaceLeft = (int)Math.Ceiling((decimal)(numberSpaceCharacterStreet / 2));
            numspaceRight = (int)Math.Floor((decimal)(numberSpaceCharacterStreet / 2));
            StreetAdreess = invoice.Invoice.Biller.Address.Street.PadLeft(invoice.Invoice.Biller.Address.Street.Length + numspaceLeft).PadRight(invoice.Invoice.Biller.Address.Street.Length + numspaceRight);
            var numberSpaceCharacterZip = NumberOfCharacter - (invoice.Invoice.Biller.Address.ZIP + "," + invoice.Invoice.Biller.Address.Town).Length;
            numspaceLeft = (int)Math.Ceiling((decimal)(numberSpaceCharacterZip / 2));
            numspaceRight = (int)Math.Floor((decimal)(numberSpaceCharacterZip / 2));
            ZipCode = $"{invoice.Invoice.Biller.Address.ZIP.ToString().PadLeft(invoice.Invoice.Biller.Address.ZIP.ToString().Length + numspaceLeft)},{invoice.Invoice.Biller.Address.Town.PadRight(invoice.Invoice.Biller.Address.Town.Length + numspaceRight)}";
            BillForPrint += $"{Town}\n{CompanyName}\n{StreetAdreess}\n{ZipCode}\n";
            BillForPrint += Dash;
            String   DatumText,   Time, Hours;
            DatumText = "Datum".PadLeft(NumberOfCharacter - 13);
            BillForPrint += "K-ID  Bonnr" + DatumText + "\n";
            var numberSpaceCharacterKid = NumberOfCharacter - invoice.Invoice.Biller.InvoiceRecipientsBillerID.ToString().Length;
            numspaceLeft = (int)Math.Ceiling((decimal)(numberSpaceCharacterKid / 2));
            numspaceRight = (int)Math.Floor((decimal)(numberSpaceCharacterKid / 2));
            Time = invoice.Invoice.InvoiceDate.ToString("dd.MM.yyyy").PadLeft(invoice.Invoice.InvoiceDate.ToString("dd.MM.yyyy").Length + 9);
            Hours = invoice.Invoice.InvoiceDate.ToString("HH:mm").PadLeft(invoice.Invoice.InvoiceDate.ToString("HH:mm").Length + 7);
            BillForPrint += $"{invoice.Invoice.Biller.InvoiceRecipientsBillerID}       {invoice.Invoice.InvoiceNumber}{Time}{Hours}\n";
            BillForPrint += Dash;
            String ArtikelDisplay, ArtikelValue, MwstDisplay, MwstValue, AnzDisplay, AnzValue, EzDisplay, EzValue, GPrDisplay, GprValue;
            ArtikelDisplay = "Artikel".PadRight(15);
            MwstDisplay = "MWSt".PadRight(8);
            AnzDisplay = "Anz".PadRight(6);
            EzDisplay = "EZ-Pr".PadRight(8);
            GPrDisplay = "G-Pr";
            BillForPrint += $"{ArtikelDisplay}{MwstDisplay}{AnzDisplay}{EzDisplay}{GPrDisplay}\n";
            BillForPrint += Dash;
            String ItemsDislay = "";
            foreach (var item in invoice.Invoice.Details.ItemList.ListLineItem)
            {
                if (item.Description.Length > 10)
                {
                    item.Description = item.Description.Substring(0, 10) + "..";
                    item.Description = item.Description.PadRight(15);
                }
                else
                {
                    item.Description = item.Description.PadRight(15);
                }
                ArtikelValue = item.Description;
                MwstValue = item.VATRate.ToString().Trim().PadRight(8);
                AnzValue = item.Quantity.Value.ToString().PadRight(6);
                EzValue = item.UnitPrice.ToString("#0.00").PadLeft(6);
                GprValue = item.LineItemAmount.ToString().PadLeft(7);
                ItemsDislay += $"{ArtikelValue}{MwstValue}{AnzValue}{EzValue}{GprValue}\n";
               
            }
            BillForPrint += ItemsDislay;
            BillForPrint += Dash;
            String Summe, SummeValue;
            Summe = "Summe";
            SummeValue = $"{invoice.Invoice.TotalGrossAmount.ToString("€ 0.00")}".PadLeft(NumberOfCharacter - 6);
            BillForPrint += $"{Summe}{SummeValue}\n";
            BillForPrint += Dash;
            String PaymentDisplay = "", CommentDisplay, PaymentValue;
            foreach (var item in invoice.Invoice.PaymentMethods)
            {
                CommentDisplay = item.Comment.Trim();
                PaymentValue = item.Amount.ToString("0.00").PadLeft(NumberOfCharacter - 4);
                PaymentDisplay += $"{CommentDisplay}{PaymentValue}\n";
            }
            BillForPrint += PaymentDisplay;
            BillForPrint += Dash;
            String  SaltzValue, NettodDisplay, NetoValue, MWStDisplay, MWStValue, BruttoDisplay, BruttoValue, TaxValue = "";
            NettodDisplay = "Netto".PadLeft(10);
            MWStDisplay = "MWSt".PadLeft(10);
            BruttoDisplay = "Brutto".PadLeft(17);
            BillForPrint += $"Saltz{NettodDisplay}{MWStDisplay}{BruttoDisplay}\n";
            foreach (var item in invoice.Invoice.Tax.VAT)
            {
                TaxShowClass tsc = new TaxShowClass();
                tsc.Salt = item.VATRate.ToString("#0") + " %";
                var netto = item.VATRate != 0 ? (item.Amount * 10) / (item.VATRate / 10) : 0;
                tsc.Netto = netto.ToString("0.00");
                tsc.MWST = item.Amount.ToString("0.00");
                tsc.Brutto = (netto + item.Amount).ToString("0.00");
                SaltzValue = tsc.Salt.PadRight(tsc.Salt.Length + (10 - tsc.Salt.Length));
                NetoValue = tsc.Netto.PadRight(tsc.Netto.Length + (11 - tsc.Netto.Length));
                MWStValue = tsc.MWST.PadRight(tsc.MWST.Length + (15 - tsc.MWST.Length));
                BruttoValue = tsc.Brutto.PadLeft(tsc.Brutto.Length + 1);
                TaxValue += $"{SaltzValue}{NetoValue}{MWStValue}{BruttoValue}\n";
            }
            BillForPrint += TaxValue;
            BillForPrint += Dash;
            Bill.Text = BillForPrint;
            
        }
    } 
    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        Navigation.PushAsync(new BillList(_authService));
    }
    private async void OnExportButtonClicked(object sender, EventArgs e)
    {
        var actionSheet = await DisplayActionSheet("Export", "Cancel", null, "Export as PDF", "Export as JPG");
        switch (actionSheet)
        {
            case "Export as PDF":
                OnExportPdfClicked();
                break;
            case "Export as JPG":
                OnExportJpgClicked();
                break;
            default:
                //
                break;
        }
    }
    [RelayCommand]
    async Task<bool> IsWriteStoragePermissionGranted()
    {
        //writeexternalstorage permission za android, photolibrary za ios
        var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
        if (status == PermissionStatus.Granted)
        {
            return true;
        }
        status = await Permissions.RequestAsync<Permissions.StorageWrite>();
        if (status == PermissionStatus.Granted)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private async void OnExportJpgClicked()
    {
        if (await IsWriteStoragePermissionGranted())
        {
            var file = await DoScreenshotAsync();
            ShareScreenshot(file);
        }
    }
    private async void OnExportPdfClicked()
    {
        if (await IsWriteStoragePermissionGranted())
        {
            var file = await DoScreenshotAsync();
            var pdfFile = ConvertJpgToPdf(file);
            SharePdf(pdfFile);
        }
    }
    
    private async Task<System.IO.FileInfo> DoScreenshotAsync()
    {
        string billGuid = _id.ToString();

        var screenshot = await Screenshot.CaptureAsync();

        var tempFolderPath = Path.GetTempPath();
        var fileName = $"Rechnung-{billGuid}.jpg";
        var filePath = Path.Combine(tempFolderPath, fileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            using (var stream = await screenshot.OpenReadAsync())
            {
                await stream.CopyToAsync(fileStream);
            }
        }

        return new System.IO.FileInfo(filePath);
    }

   

    private async void ShareScreenshot(System.IO.FileInfo file)
    {
        var filePath = file.FullName;
        var shareFileRequest = new Microsoft.Maui.ApplicationModel.DataTransfer.ShareFileRequest();
        shareFileRequest.File = new Microsoft.Maui.ApplicationModel.DataTransfer.ShareFile(filePath);
        await Microsoft.Maui.ApplicationModel.DataTransfer.Share.RequestAsync(shareFileRequest);
    }
    private static System.IO.FileInfo ConvertJpgToPdf(System.IO.FileInfo file)
    {
        var pdfFileName = "converted.pdf";
        var tempFolderPath = Path.GetTempPath();
        var pdfFilePath = Path.Combine(tempFolderPath, pdfFileName);

        using (var document = new Document())
        {
            using (var memoryStream = new MemoryStream())
            {
                var writer = PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                var image = iTextSharp.text.Image.GetInstance(file.FullName);

                // Adjust the image scale to fit the document page
                image.ScaleToFit(document.PageSize.Width, document.PageSize.Height);

                // Set the image alignment to center
                image.Alignment = iTextSharp.text.Image.ALIGN_CENTER;

                // Add the image to the document
                document.Add(image);

                document.Close();

                File.WriteAllBytes(pdfFilePath, memoryStream.ToArray());
            }
        }

        return new System.IO.FileInfo(pdfFilePath);
    }
    private async void SharePdf(System.IO.FileInfo file)
    {
        var filePath = file.FullName;
        var shareFile = new ShareFile(filePath, "application/pdf");
        await Share.RequestAsync(new ShareFileRequest(shareFile));
    }
}