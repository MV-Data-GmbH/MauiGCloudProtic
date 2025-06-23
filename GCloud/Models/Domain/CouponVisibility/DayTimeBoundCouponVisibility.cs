using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GCloud.Models.Domain
{
    public class DayTimeBoundCouponVisibility : AbstractCouponVisibility
    {
        public TimeSpan? MondayFrom { get; set; }
        public TimeSpan? MondayTo { get; set; }
        public TimeSpan? TuesdayFrom { get; set; }
        public TimeSpan? TuesdayTo { get; set; }
        public TimeSpan? WednesdayFrom { get; set; }
        public TimeSpan? WednesdayTo { get; set; }
        public TimeSpan? ThursdayFrom { get; set; }
        public TimeSpan? ThursdayTo { get; set; }
        public TimeSpan? FridayFrom { get; set; }
        public TimeSpan? FridayTo { get; set; }
        public TimeSpan? SaturdayFrom { get; set; }
        public TimeSpan? SaturdayTo { get; set; }
        public TimeSpan? SundayFrom { get; set; }
        public TimeSpan? SundayTo { get; set; }

        public bool IsValid()
        {
            var currentDay = DateTime.Now.DayOfWeek;

            var dayProperties = GetType().GetProperties().Where(x => x.Name.StartsWith(currentDay.ToString())).ToList();

            var fromProperty = dayProperties.FirstOrDefault(x => x.Name.EndsWith("From"));
            var toProperty = dayProperties.FirstOrDefault(x => x.Name.EndsWith("To"));

            var fromValue = fromProperty?.GetValue(this) as TimeSpan?;
            var toValue = toProperty?.GetValue(this) as TimeSpan?;

            if (!fromValue.HasValue && !toValue.HasValue)
            {
                return false;
            }

            var now = DateTime.Now.TimeOfDay;
            return !fromValue.HasValue && toValue.Value >= now ||
                   !toValue.HasValue && fromValue.Value <= now ||
                   fromValue.HasValue && fromValue.Value <= now && toValue.HasValue && toValue.Value >= now;
        }

        public override bool IsValid(string userId)
        {
            return IsValid();
        }

        public override string GetHumanReadableName()
        {
            return "Tageszeit";
        }
    }
}