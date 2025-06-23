using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GCloud.Controllers.ViewModels.User
{
    public class UserCreateViewModel
    {
        [DisplayName("Benutzername")]
        [MinLength(3, ErrorMessage = "Der Benutzername muss mindestens aus 3 Zeichen bestehen")]
        [Required(ErrorMessage = "Es muss ein Benutzername angegeben werden")]
        public string Username { get; set; }
        [DisplayName("Passwort")]
        [Required(ErrorMessage = "Es muss ein Passwort angegeben werden")]
        [MinLength(5, ErrorMessage = "Das Passwort muss aus mindestens 5 Zeichen bestehen")]
        public string Password { get; set; }

        [DisplayName("Aktiviert")]
        public bool Enabled { get; set; } = true;
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
        public DateTime? Birtday { get; set; }
        [DisplayName("Rolle")]
        [Required(ErrorMessage = "Dem Benutzer muss eine Role zugewiesen werden")]
        public string RoleId { get; set; }

        public string InvitationCode { get; set; }

        public string TotalPoints { get; set; }

        public string InvitationCodeSender { get; set; }

        public bool DataProtection { get; set; }

        public bool AGB { get; set; }

        public bool MarketingAgreement { get; set; }

    }
}