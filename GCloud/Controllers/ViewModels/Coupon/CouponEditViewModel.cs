using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GCloud.Models.Domain;
using GCloud.Models.Domain.CouponUsageAction;

namespace GCloud.Controllers.ViewModels.Coupon
{
    public class CouponEditViewModel
    {
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
        [Range(1, 5, ErrorMessage = "Wählen Sie einen gültigen Gutscheintyp aus.")]
        public CouponType CouponType { get; set; }
        [DisplayName("Gutscheinscope")]
        public CouponScope CouponScope { get; set; }
        [DisplayName("Artikelnummer")]
        public int? ArticleNumber { get; set; }
        [NotMapped]
        [Required(ErrorMessage = "Image is required.")]
        public String ImageData { get; set; }


        public string CreatedUserId { get; set; }
        public virtual Models.Domain.User CreatedUser { get; set; }

        public virtual List<CheckBoxListItem> AssignedStores { get; set; }
        public virtual ICollection<CouponImage> CouponImages { get; set; }

        public virtual ICollection<Redeem> Redeems { get; set; }
        public virtual ICollection<AbstractCouponVisibility> Visibilities { get; set; } = new List<AbstractCouponVisibility>();
        public virtual AbstractUsageAction Usage { get; set; }
        [DisplayName("Aktiviert?")]
        public bool Enabled { get; set; }
    }
}