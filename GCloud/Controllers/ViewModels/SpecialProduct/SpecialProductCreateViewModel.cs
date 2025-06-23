using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GCloud.Controllers.ViewModels.SpecialProduct
{
    public class SpecialProductCreateViewModel
    {
        [Required]
        [DisplayName("Bezeichnung")]
        public string Name { get; set; }
        [Required]
        [DisplayName("Kurze Beschreibung")]
        public string ShortDescription { get; set; }
        [Required]
        [DisplayName("Wert")]
        public decimal Value { get; set; }
      
        [DisplayName("Zugewiesene Filialen")]
        public virtual List<CheckBoxListItem> AssignedStores { get; set; } = new List<CheckBoxListItem>();

        [DisplayName("Aktiviert?")]
        public bool Enabled { get; set; } = true;
    }
}