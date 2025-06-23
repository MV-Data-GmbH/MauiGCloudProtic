

namespace GCloud.Shared.Dto.Api
{
    public class StoreCouponApiBindingModel
    {
        public class StoreCouponApiPutModel
        {
            public Guid CouponId { get; set; }
            public Guid UserGuid { get; set; }
            public string StoreApiToken { get; set; }
            public decimal CashValue { get; set; }
            /// <summary>
            /// Gibt an, wie viel Umsatz in dieser Rechnung gemacht wird. Wird immer dann gesetzt, wenn der Umsatz für diesen Kunden verbucht werden soll, aber kein Cashback gesetzt wurde.
            /// Ist notwendig, da der Umsatz sonst nur mit erstellen eines Cashbacks erhöht wird, aber beim einlösen eines Gutscheines kein Cashback vergeben wird.
            /// </summary>
            public decimal? InvoiceTurnover { get; set; }
            public DateTime RedeemDateTime { get; set; }
            public Guid CashRegisterId { get; set; }
        }
    }
}