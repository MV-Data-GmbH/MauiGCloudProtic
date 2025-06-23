using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;

namespace GCloud.Controllers.ViewModels.SpecialProduct
{
    public class SpecialProductEditViewModel
    {

        public Guid Id { get; set; }

        [Display(Name = "Bezeichnung")]
        public string Name { get; set; }

        [Display(Name = "Kurze Beschreibung")]
        public string ShortDescription { get; set; }

        [Display(Name = "Wert")]
        public decimal Value { get; set; }

        public string CreatedUserId { get; set; }

        public virtual GCloud.Models.Domain.User CreatedUser { get; set; }

        public virtual ICollection<GCloud.Models.Domain.Store> AssignedStores { get; set; }



        public bool Enabled { get; set; }

        public bool IsDeleted { get; set; }

     

    }
}