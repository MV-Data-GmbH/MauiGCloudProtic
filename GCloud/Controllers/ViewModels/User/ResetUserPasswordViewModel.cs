using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GCloud.Controllers.ViewModels.User
{
    public class ResetUserPasswordViewModel
    {
        [Required]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Passwort ist erforderlich")]
        [StringLength(255,ErrorMessage = "Password Länge muss zwischen 7 und 255 Zeichen liegen.", MinimumLength = 7)]
        [DataType(DataType.Password)]
        [DisplayName("Passwort")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Passwort-Wiederholung ist erforderlich")]
        [StringLength(255, ErrorMessage = "Password Länge muss zwischen 7 und 255 Zeichen liegen.", MinimumLength = 7)]
        [Compare("NewPassword")]
        [DataType(DataType.Password)]
        [DisplayName("Passwort Wiederholen")]
        public string RepeatNewPassword { get; set; }

        [DisplayName("Benutzer Benachrichtigen?")]
        public bool InformUser { get; set; }
    }
}