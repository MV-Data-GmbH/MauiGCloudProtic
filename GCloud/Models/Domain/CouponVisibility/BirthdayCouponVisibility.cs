using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace GCloud.Models.Domain
{
    public class BirthdayCouponVisibility : AbstractCouponVisibility, IDateTimeBounded
    {
        /// <summary>
        /// Gibt an wie viele Tage vor dem Geburtstag der Gutschein gültig ist
        /// </summary>
        public int ValidDaysBefore { get; set; }
        /// <summary>
        /// Gibt an wie viele Tage nach dem Geburtstag der Gutschein gültig ist
        /// </summary>
        public int ValidDaysAfter { get; set; }

        public override bool IsValid(string userId)
        {
            var birthday = Coupon.AssignedStores.SelectMany(store => store.InterestedUsers).FirstOrDefault(x => x.Id == userId)?.Birthday;
            if (!birthday.HasValue)
            {
                return false;
            }
            var today = DateTime.Now.Date;
            var birthdayThisYear = new DateTime(today.Year, birthday.Value.Month, birthday.Value.Day);
            return today >= birthdayThisYear.AddDays(ValidDaysBefore * -1) && today <= birthdayThisYear.AddDays(ValidDaysAfter);
        }

        public override string GetHumanReadableName()
        {
            return "Geburtstag";
        }

        public DateTime? GetValidFrom(string userId)
        {
            var birthday = Coupon.AssignedStores.SelectMany(store => store.InterestedUsers).FirstOrDefault(x => x.Id == userId)?.Birthday;
            if (!birthday.HasValue)
            {
                return null;
            }
            var birthdayThisYear = new DateTime(DateTime.Now.Year, birthday.Value.Month, birthday.Value.Day);

            return birthdayThisYear.AddDays(ValidDaysBefore * -1);
        }

        public DateTime? GetValidTo(string userId)
        {
            var birthday = Coupon.AssignedStores.SelectMany(store => store.InterestedUsers).FirstOrDefault(x => x.Id == userId)?.Birthday;
            if (!birthday.HasValue)
            {
                return null;
            }
            var birthdayThisYear = new DateTime(DateTime.Now.Year, birthday.Value.Month, birthday.Value.Day);
            return birthdayThisYear.AddDays(ValidDaysAfter);
        }
    }
}