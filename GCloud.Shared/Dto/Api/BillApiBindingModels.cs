
using GCloud.Shared.Dto.Domain;


namespace GCloud.Shared.Dto.Api
{
    public class GetBillsResponseModel : List<Bill_Out_Dto>
    {
        public GetBillsResponseModel (IEnumerable<Bill_Out_Dto> collection) : base(collection) { }
    }

    public class BillAddRequestModel
    {
        public Guid CashRegisterId { get; set; }
        public string StoreApiToken { get; set; }
        /// <summary>
        /// Das ist die User-ID von dem FoodJet Benutzer, sofern der Enduser überhaupt FoodJet benutzt. 
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// Das ist die Anonyme User-Id, falls die E-Bill App verwendet wird. Ist auch die UserId gesetzt, wird immer die User-ID bevorzugt.
        /// </summary>
        public Guid? AnonymousUserId { get; set; }
        public List<Invoice> Invoices { get; set; } = new List<Invoice>();
    }

    public class BillAddRequestModelV2
    {
        public Guid CashRegisterId { get; set; }
        public string StoreApiToken { get; set; }
        /// <summary>
        /// Das ist die User-ID von dem FoodJet Benutzer, sofern der Enduser überhaupt FoodJet benutzt. 
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// Das ist die Anonyme User-Id, falls die E-Bill App verwendet wird. Ist auch die UserId gesetzt, wird immer die User-ID bevorzugt.
        /// </summary>
        public decimal Amount { get; set; }
       
    }

    public class AnonymousUserNewRequestModel
    {
        public string FirebaseToken { get; set; }
        public Guid? AnonymousUserId { get; set; }
    }

    public class AnonymousUserNewResponseModel
    {
        public Guid AnonymousUserId { get; set; }
    }
}