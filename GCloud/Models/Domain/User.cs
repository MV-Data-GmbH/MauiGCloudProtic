using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using GCloud.Models;
using GCloud.Models.Domain;

namespace GCloud.Models.Domain
{
    public class User : IdentityUser, IIdentifyable
    {
        public bool IsActive { get; set; }

        [Display(Name = "Vorname")]
        public string FirstName { get; set; }
        [Display(Name = "Nachname")]
        public string LastName { get; set; }

        public string InvitationCode { get; set; }

        public string TotalPoints { get; set; }

        public string InvitationCodeSender { get; set; }

        public bool DataProtection { get; set; }

        public bool AGB { get; set; }

        public bool MarketingAgreement { get; set; }

        [Display(Name = "Email Adresse")]
        [Required(ErrorMessage = "Eine E-Mail Adresse ist erforderlich")]
        [EmailAddress(ErrorMessage = "Ungültige E-Mail Adresse")]
        [DataType(DataType.EmailAddress)]
        public override string Email
        {
            get => base.Email;
            set => base.Email = value.Trim();
        }
        
        public DateTime? Birthday { get; set; }

        public virtual ICollection<Company> Companies { get; set; }
        public virtual ICollection<Store> InterrestedStores { get; set; }

        public virtual ICollection<User> CreatedUsers { get; set; }

        public string CreatedById { get; set; }
        public virtual User CreatedBy { get; set; }
        /// <summary>
        /// Das sind die Benutzer, welche mal von diesem Benutzer verwendet wurden. Zum Beispiel von der E-Bill App
        /// </summary>
        public virtual ICollection<AnonymousUser> AnonymousUsers { get; set; }

        public virtual ICollection<Bill> Bills { get; set; }
        public bool IsDeleted { get; set; }
        public virtual ICollection<TurnoverJournal> TurnoverJournals { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<GCloud.Models.Domain.User> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }

        public Guid GetId()
        {
            return new Guid(Id);
        }

       
    }
}