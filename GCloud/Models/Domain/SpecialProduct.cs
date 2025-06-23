using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GCloud.Models.Domain
{
    public class SpecialProduct : ISoftDeletable, IIdentifyable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [Display(Name = "Bezeichnung")]
        public string Name { get; set; }

        [Display(Name = "Kurze Beschreibung")]
        public string ShortDescription { get; set; }

        [Required]
        [Display(Name = "Wert")]
        public decimal Value { get; set; }

        public string CreatedUserId { get; set; }

        //Navigation property
        public virtual User CreatedUser { get; set; }

        public bool Enabled { get; set; }

        public bool IsDeleted { get; set; }

        public virtual ICollection<Store> AssignedStores { get; set; }

        public Guid GetId()
        {
            return Id;
        }
    }
}