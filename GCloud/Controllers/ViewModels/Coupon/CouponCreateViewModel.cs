using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using GCloud.Models.Domain;
using GCloud.Models.Domain.CouponUsageAction;

namespace GCloud.Controllers.ViewModels.Coupon
{
    public class CouponCreateViewModel
    {
        [Required]
        [DisplayName("Bezeichnung")]
        public string Name { get; set; }
        [DisplayName("Kurze Beschreibung")]
        public string ShortDescription { get; set; }
        [DisplayName("Max Einlösungen")]
        public int? MaxRedeems { get; set; }
        [Required]
        [DisplayName("Wert")]
        public decimal Value { get; set; }
        [Required]
        [DisplayName("GutscheinTyp")]
        [Range(1, 5, ErrorMessage = "Wählen Sie einen gültigen Gutscheintyp aus.")]
        public CouponType CouponType { get; set; }
        [Required(ErrorMessage = "Image is required.")]
        public String ImageData { get; set; }
        [DisplayName("Gutscheinscope")]
        public CouponScope CouponScope { get; set; }
        [DisplayName("Artikelnummer")]
        public int? ArticleNumber { get; set; }
        [DisplayName("Zugewiesene Filialen")]
        public virtual List<CheckBoxListItem> AssignedStores { get; set; } = new List<CheckBoxListItem>();

        public virtual ICollection<AbstractCouponVisibility> Visibilities { get; set; } = new List<AbstractCouponVisibility>();
        public virtual AbstractUsageAction Usage { get; set; }
        [DisplayName("Aktiviert?")]
        public bool Enabled { get; set; } = true;
    }
}