using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using GCloud.Models;
using GCloud.Models.Domain;
using GCloud.Models.Domain.CouponUsageAction;
using GCloud.Models.Domain.CouponUsageRequirement;

namespace GCloud.Models.Domain
{
    public class Coupon : ISoftDeletable, IIdentifyable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [Display(Name = "Bezeichnung")]
        public string Name { get; set; }
        [Display(Name = "Kurze Beschreibung")]
        public string ShortDescription { get; set; }
        [Display(Name = "Max Einlösungen")]
        public int? MaxRedeems { get; set; }
        [Required]
        [Display(Name = "Wert")]
        public decimal Value { get; set; }
        [Required]
        [Display(Name = "Gutscheintyp")]
        [Range(1,4,ErrorMessage = "Wählen Sie einen gültigen Gutscheintyp aus.")]
        public CouponType CouponType { get; set; }
        public bool Enabled { get; set; }
        [NotMapped]
        public String ImageData { get; set; }


        public string CreatedUserId { get; set; }
        public virtual User CreatedUser { get; set; }

        [Display(Name = "Zugewiesene Filialen")]
        public virtual ICollection<Store> AssignedStores { get; set; }
        public virtual ICollection<CouponImage> CouponImages { get; set; }

        public virtual ICollection<Redeem> Redeems { get; set; }
        public virtual ICollection<AbstractCouponVisibility> Visibilities { get; set; } = new List<AbstractCouponVisibility>();
        public virtual ICollection<AbstractUsageRequirement> UsageRequirements { get; set; } = new List<AbstractUsageRequirement>();
        public virtual ICollection<AbstractUsageAction> UsageActions { get; set; } = new List<AbstractUsageAction>();
        public bool IsDeleted { get; set; }

        public Guid GetId()
        {
            return Id;
        }

        /// <summary>
        /// Gibt an ob dieser Gutschein mit den gegebenen Visibility Einträgen IM MOMENT gültig ist.
        /// </summary>
        /// <returns></returns>
        public bool IsCurrentlyValidForUser(string userId)
        {
            var valid = Enabled && Visibilities.Aggregate(true, (current, Visibility) => current && Visibility.IsValid(userId));
            return valid && this.UserHasRedeemsLeft(userId);
        }

        public bool UserHasRedeemsLeft(string userId)
        {
            return Redeems.Count(redeem => redeem.UserId == userId) < (MaxRedeems ?? int.MaxValue);
        }

        public IEnumerable<Redeem> GetRedeemsForUser(string userId)
        {
            return Redeems.Where(x => x.UserId == userId);
        }

        public IEnumerable<TType> GetVisibilitiesForType<TType>()
        {
            return Visibilities.OfType<TType>();
        }

        public DateTime? GetValidFrom(string userId)
        {
            var validFromDates = GetVisibilitiesForType<IDateTimeBounded>().Select(x => x.GetValidFrom(userId));
            return validFromDates.Max();
        }

        public DateTime? GetValidTo(string userId)
        {
            var validToDates = GetVisibilitiesForType<IDateTimeBounded>().Select(x => x.GetValidTo(userId));
            return validToDates.Max();
        }
    }
}