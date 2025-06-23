using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GCloud.Models.Domain
{
    public class MinimumVisitCouponVisibility : AbstractCouponVisibility
    {
        /// <summary>
        /// Legt fest, wie oft ein Kunde mindestens mit der Karte bezahlt haben muss
        /// </summary>
        public int MinimumVisits { get; set; }

        public int DateRange { get; set; }

        public override bool IsValid(string userId)
        {
            return Coupon?.AssignedStores?.SelectMany(store => store.InterestedUsers)
                       .FirstOrDefault(x => x.Id == userId)
                      ?.TurnoverJournals.Count(x => x.CreditDateTime >= DateTime.Now.Date.AddDays(DateRange * -1)) >= MinimumVisits;
        }

        public override string GetHumanReadableName()
        {
            return "Besuchszahl";
        }
    }
}