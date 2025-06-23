using GCloud.Shared.Dto.Domain;
using System.Collections.ObjectModel;


namespace GCloudPhone.Domain
{
    public class BillShowClass
    {
        public string Date { get; set; }
        public ObservableCollection<Bill_Out_Dto> Bills { get; set; }
    }
}
