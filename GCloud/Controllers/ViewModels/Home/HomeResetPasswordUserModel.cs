using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GCloud.Controllers.ViewModels.Home
{
    public class HomeResetPasswordUserModel
    {
        [Required]
        [DataType(DataType.Password)]
        [DisplayName("Altes Passwort")]
        public string OldPassword { get; set; }
        [Required]
        [MinLength(5, ErrorMessage = "Das Passwort muss mindestens 5 Zeichen lang sein.")]
        [DisplayName("Neues Passwort")]
        [DataType(DataType.Password)]
        public string PasswordNew { get; set; }
        [Required]
        [MinLength(5, ErrorMessage = "Das Passwort muss mindestens 5 Zeichen lang sein.")]
        [Compare("PasswordNew")]
        [DisplayName("Passwort wiederholen")]
        [DataType(DataType.Password)]
        public string PasswordNewRepeat { get; set; }
    }
}