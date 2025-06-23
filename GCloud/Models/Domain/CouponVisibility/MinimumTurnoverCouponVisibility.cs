using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GCloud.Models.Domain
{
    public class MinimumTurnoverCouponVisibility : AbstractCouponVisibility
    {
        public decimal MinimumTurnover { get; set; }
        /// <summary>
        /// Gibt den Zeitraum an, für den der Umsatz gesammelt wurde. zB heißt ein Wert von 10, dass in den letzten 10 Tagen der Umsatz gesammelt werden musste.
        /// </summary>
        public int DateRange { get; set; }

        public override bool IsValid(string userId)
        {
            return Coupon?.AssignedStores.SelectMany(store => store.InterestedUsers).FirstOrDefault(x => x.Id == userId)?.TurnoverJournals
                  ?.Where(x => x.StoreId == Coupon?.AssignedStores.First().Id)
                   .OrderByDescending(x => x.CreditDateTime)
                   .TakeWhile(x => x.CreditDateTime.Date >= DateTime.Now.Date.AddDays(DateRange * -1))
                   .Aggregate(0m, (x, y) => x + y.TurnoverChange) > MinimumTurnover;
        }

        public override string GetHumanReadableName()
        {
            return "Mindestumsatz";
        }
    }
}