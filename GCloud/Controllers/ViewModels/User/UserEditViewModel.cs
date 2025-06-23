using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GCloud.Controllers.ViewModels.User
{
    public class UserEditViewModel
    {
        [Required]
        public string Id { get; set; }
        [DisplayName("Benutzername")]
        [MinLength(3, ErrorMessage = "Der Benutzername muss mindestens aus 3 Zeichen bestehen")]
        [Required(ErrorMessage = "Es muss ein Benutzername angegeben werden")]
        public string Username { get; set; }
        [DisplayName("Aktiviert")]
        public bool Enabled { get; set; }
        [Required(ErrorMessage = "Eine Email muss angegeben werden")]
        [EmailAddress]
        [DisplayName("E-Mail")]
        public string Email { get; set; }
        [DisplayName("Telefonnummer")]
        public string PhoneNumber { get; set; }
        [DisplayName("Vorname")]
        [Required(ErrorMessage = "Der Vorname darf nicht leer sein")]
        public string FirstName { get; set; }
        [DisplayName("Nachname")]
        [Required(ErrorMessage = "Der Nachname darf nicht leer sein")]
        public string LastName { get; set; }
        [DisplayName("Geburtstag")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? Birthday { get; set; }
        [DisplayName("Rolle")]
        [Required(ErrorMessage = "Dem Benutzer muss eine Role zugewiesen werden")]
        public string RoleId { get; set; }
        public string CreatedById { get; set; }

        public string InvitationCode { get; set; }

        public string TotalPoints { get; set; }

        public string InvitationCodeSender { get; set; }

        public bool DataProtection { get; set; }

        public bool AGB { get; set; }

        public bool MarketingAgreement { get; set; }
    }
}