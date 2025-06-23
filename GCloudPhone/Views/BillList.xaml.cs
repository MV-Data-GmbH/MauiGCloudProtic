using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Views;
using GCloud.Shared.Dto.Domain;
using GCloud.Shared.Exceptions;
using GCloudPhone.Domain;
using GCloudPhone.Views.Settings.MyAccount;
using GCloudShared.Interface;
using GCloudShared.Service;
using System.Collections.ObjectModel;

namespace GCloudPhone.Views;
public partial class BillList : ContentPage
{
    private IAuthService _authService;
    private bool IsLoging;
    ObservableCollection<BillShowClass> bls = new ObservableCollection<BillShowClass>();
    List<Bill_Out_Dto> listBills = new List<Bill_Out_Dto>();
    public BillList(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
        BillLogged(authService);
        if (IsLoging)
        {
            GetBillList();
        }
        
    }
    private async void BillLogged(IAuthService authService)
    {
        if (!authService.IsLogged())
        {
            IsLoging = false;
            await DisplayAlert("Fehler", "Sie müssen angemeldet sein, um fortzufahren!", "OK");
            await Navigation.PushAsync(new LoginPage(authService));
            return;
        }
        else
        {
            IsLoging = true;
        }
    }
    private void OnBackButtonClicked(object sender, System.EventArgs e)
    {
        Navigation.PushAsync(new SettingsPage());
    }
   
    private async void GetBillList()
    {
        //var billservice = new BillService();
        //var result = await billservice.Get();
        //listBills = result as List<Bill_Out_Dto>;
        //if (listBills != null)
        //{
        //    if (listBills.Count > 0)
        //    {
        //        MainThread.BeginInvokeOnMainThread(() => { BillMsg.IsVisible = false; });
        //        ShowBillList(listBills);
        //    }
        //    else
        //    {
        //        MainThread.BeginInvokeOnMainThread(() => { BillMsg.IsVisible = true; });
        //    }
        //}
        //else
        //{
        //    var forbiden = result as HttpResponseMessage;
        //    if (forbiden != null)
        //    {
        //        if (forbiden.StatusCode == System.Net.HttpStatusCode.Forbidden)
        //        {
        //            await DisplayAlert("Fehler", "Sie müssen angemeldet sein, um fortzufahren!", "OK");
        //            await Navigation.PushAsync(new LoginPage(_authService));
        //            return;
        //        }
        //    }
        //    var error = result as ExceptionHandlerResult;
        //    if (error != null)
        //    {
        //        await DisplayAlert("Fehler", error.Message, "OK");
        //        return;
        //    }
        //    await DisplayAlert("Fehler", "Error an occured", "OK");
        //}
    }
    private void ShowBillList(List<Bill_Out_Dto> listbill)
    {
        //bls.Clear();
        //List<DateTime> ids = new List<DateTime>();
        //List<DateTime> ids1 = new List<DateTime>();
        //foreach (var item in listbill)
        //{
        //    ids.Add(item.Invoice.InvoiceDate);
        //}
        //foreach (var v in ids)
        //{
        //    if (!ids1.Contains(v.Date))
        //    {
        //        ids1.Add(v.Date);
        //    }
        //}
        //foreach (var v in ids1)
        //{
        //    BillShowClass bsc = new BillShowClass();
        //    bsc.Date = v.ToString("dd") + ". " + ConvertMonthInText(v) + " " + v.ToString("yyyy");
        //    bsc.Bills = listbill.Where(x => x.Invoice.InvoiceDate.ToString("dd/MM/yyyy") == v.Date.ToString("dd/MM/yyyy")).ToObservableCollection();
        //    bls.Add(bsc);
        //}
        //MainThread.BeginInvokeOnMainThread(() =>
        //{
        //    Rachung.ItemsSource = null;
        //    Rachung.ItemsSource = bls;
        //});
    }
    // Helper method to find a child element by name recursively
    private T FindChildByName<T>(Element element, string name) where T : Element
    {
        if (element is T tElement && element.StyleId == name)
            return tElement;
        if (element is IElementController elementController)
        {
            foreach (var child in elementController.LogicalChildren)
            {
                var foundElement = FindChildByName<T>(child, name);
                if (foundElement != null)
                    return foundElement;
            }
        }
        return null;
    }
    private async void btnShowPopupSort_Clicked(object sender, EventArgs e)
    {
        var result = await this.ShowPopupAsync(new PopupSortList());
        if (result != null)
        {
            if ((bool)result)
            {
                List<Bill_Out_Dto> temp = new List<Bill_Out_Dto>();
                var typeSort = PopupSortList.TypeSort;
                var sortAsc = PopupSortList.SortASC;
                if (typeSort == "Datum")
                {
                    if (sortAsc)
                    {
                        temp = listBills.OrderBy(x => x.Invoice.InvoiceDate).ToList();
                    }
                    else
                    {
                        temp = listBills.OrderByDescending(x => x.Invoice.InvoiceDate).ToList();
                    }
                }
                if (typeSort == "Rachung")
                {
                    if (sortAsc)
                    {
                        temp = listBills.OrderBy(x => x.Invoice.InvoiceNumber).ToList();
                    }
                    else
                    {
                        temp = listBills.OrderByDescending(x => x.Invoice.InvoiceNumber).ToList();
                    }
                }
                if (typeSort == "Preis")
                {
                    if (sortAsc)
                    {
                        temp = listBills.OrderBy(x => x.Invoice.TotalGrossAmount).ToList();
                    }
                    else
                    {
                        temp = listBills.OrderByDescending(x => x.Invoice.TotalGrossAmount).ToList();
                    }
                }
                if (typeSort == "Store")
                {
                    if (sortAsc)
                    {
                        temp = listBills.OrderBy(x => x.Invoice.Biller.InvoiceRecipientsBillerID).ToList();
                    }
                    else
                    {
                        temp = listBills.OrderByDescending(x => x.Invoice.Biller.InvoiceRecipientsBillerID).ToList();
                    }
                }
                ShowBillList(temp);
            }
        }
    }
    private static string ConvertMonthInText(DateTime date)
    {
        string month = "";
        switch (date.Month)
        {
            case 1:
                month = "Januar";
                break;
            case 2:
                month = "Februar";
                break;
            case 3:
                month = "März";
                break;
            case 4:
                month = "April";
                break;
            case 5:
                month = "Mai";
                break;
            case 6:
                month = "Juni";
                break;
            case 7:
                month = "Juli";
                break;
            case 8:
                month = "August";
                break;
            case 9:
                month = "September";
                break;
            case 10:
                month = "Oktober ";
                break;
            case 11:
                month = "November";
                break;
            case 12:
                month = "Dezember";
                break;
        }
        return month;
    }
    private void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        //var current = e.CurrentSelection;
        //var bill = current[0] as Bill_Out_Dto;
        //if (bill != null)
        //{
        //    var id = bill.Id;
        //    Navigation.PushAsync(new BillDetails(id, _authService));
        //}
    }
   
    private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        var searcquery = sender as SearchBar;
        if (searcquery != null)
        {
            var query = searcquery.Text;
            var searchResult = listBills.Where(c =>
                      c.Invoice.Biller.ComanyName.ToUpper().Contains(query.ToUpper())
                      || c.Invoice.InvoiceNumber.ToUpper().Contains(query.ToUpper())
                      || c.Invoice.InvoiceDate.ToString().ToUpper().Contains(query.ToUpper())
                  ).ToList();
            ShowBillList(searchResult);
        }
    }
    private void btnMore_Clicked(object sender, EventArgs e)
    {
        this.ShowPopupAsync(new PopupMore_Bill());
    }
}


