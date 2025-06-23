using System;
using System.ComponentModel.DataAnnotations;

namespace GCloud.Models.Domain
{
    public class DateBoundCouponVisibility : AbstractCouponVisibility, IDateTimeBounded
    {
        [Display(Name = "Gültig Von")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? ValidFrom { get; set; }
        [Display(Name = "Gültig Bis")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? ValidTo { get; set; }

        public bool IsValid()
        {
            var today = DateTime.Now.Date;
            return (!ValidFrom.HasValue && !ValidTo.HasValue) ||
                   (!ValidFrom.HasValue && ValidTo.HasValue && ValidTo.Value >= today) ||
                   (!ValidTo.HasValue && ValidFrom.HasValue && ValidFrom.Value <= today) ||
                   (ValidFrom.HasValue && ValidFrom.Value <= today && ValidTo.HasValue && ValidTo.Value >= today);
        }

        public override bool IsValid(string userId)
        {
            return IsValid();
        }

        public override string GetHumanReadableName()
        {
            return "Datum-Zeitraum";
        }

        public bool IsDateTimeNowOrUpcoming()
        {
            var today = DateTime.Now.Date;
            return (!ValidFrom.HasValue && !ValidTo.HasValue) ||
                   (!ValidFrom.HasValue && ValidTo.HasValue && ValidTo.Value >= today) ||
                   (!ValidTo.HasValue && ValidFrom.HasValue) ||
                   (ValidTo.Value >= today);
        }

        public bool IsDateTimeUpcoming()
        {
            var today = DateTime.Now.Date;
            return ValidFrom.HasValue && ValidFrom.Value > today;
        }

        public bool IsDateTimeExpired()
        {
            var today = DateTime.Now.Date;
            return ValidTo.HasValue && ValidTo.Value < today;
        }

        public DateTime? GetValidFrom(string userId)
        {
            return ValidFrom;
        }

        public DateTime? GetValidTo(string userId)
        {
            return ValidTo;
        }
    }
}