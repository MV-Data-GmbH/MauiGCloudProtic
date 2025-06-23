using System;
using System.ComponentModel;
using GCloud.Models;

namespace GCloud.Controllers.ViewModels.User
{
    public class UserIndexViewModel : IIdentifyable
    {
        public string Id { get; set; }
        [DisplayName("Benutzername")]
        public string Username { get; set; }
        [DisplayName("Erstellt von")]
        public string CreatedByUsername { get; set; }
        [DisplayName("E-Mail")]
        public string Email { get; set; }
        [DisplayName("Rolle")]
        public string RoleName { get; set; }
        [DisplayName("Aktiviert")]
        public bool Enabled { get; set; }
        public bool IsDeleted { get; set; }
        public Guid GetId()
        {
            if (Guid.TryParse(Id, out var guid))
            {
                return guid;
            }
            return Guid.Empty;
        }
    }
}