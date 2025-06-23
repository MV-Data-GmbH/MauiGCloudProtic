using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GCloud.Controllers.ViewModels.Company
{
    public class CompanyCreateViewModel
    {
        [DisplayName("Unternehmensname")]
        [Required]
        public string Name { get; set; }
        [DisplayName("UID")]
        [Required]
        public string TaxNumber { get; set; }
        [DisplayName("Steuernummer")]
        [Required]
        public string CommercialRegisterNumber { get; set; }
        [DisplayName("Cashback aktiviert?")]
        public bool IsCashbackEnabled { get; set; } = false;
        [DisplayName("Zugewiesener User")]
        [Required]
        public string AssociatedUserId { get; set; }
        [DisplayName("Logo")]
        [Required(ErrorMessage ="Logo is required.")]
        public String LogoData { get; set; }

        [DisplayName("Award Points")]
        public int AwardPoints { get; set; }

        [DisplayName("Registration Points")]
        public int RegistrationPoints { get; set; }
    }
}