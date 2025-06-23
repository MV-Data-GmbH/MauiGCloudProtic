using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GCloud.Controllers.ViewModels.Home
{
    public class ResetPasswordViewModel
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public string Code { get; set; }
        [DisplayName("Passwort")]
        [Required]
        [MinLength(6, ErrorMessage = "Das Passwort muss mindestens 6 Zeichen lang sein.")]
        public string Password { get; set; }
        [DisplayName("Passwort wiederholen")]
        [Required]
        [MinLength(6, ErrorMessage = "Das Passwort muss mindestens 6 Zeichen lang sein.")]
        [Compare(nameof(Password), ErrorMessage = "Die Passwörter stimmen nicht überein!")]
        public string RepeatPassword { get; set; }
    }
}