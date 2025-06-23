using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using GCloud.Models;

namespace GCloud.Models.Domain
{
    public class Company : ISoftDeletable, IIdentifyable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [Display(Name = "Unternehmensname")]
        public string Name { get; set; }
        [Display(Name = "UID")]
        public string TaxNumber { get; set; }
        [Display(Name = "Steuernummer")]
        public string CommercialRegisterNumber { get; set; }
        [Display(Name = "Cashback aktiviert?")]
        public bool IsCashbackEnabled { get; set; } = false;
        [Display(Name = "Benutzer")]
        public string UserId { get; set; }
        public virtual User User { get; set; }

        public virtual ICollection<Store> Stores { get; set; }
        public bool IsDeleted { get; set; }

        public int AwardPoints { get; set; }
        public int RegistrationPoints { get; set; }

        public Guid GetId()
        {
            return Id;
        }
    }
}