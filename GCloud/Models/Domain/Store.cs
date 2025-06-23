using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using GCloud.Models;
using GCloud.Models.Domain;

namespace GCloud.Models.Domain
{
    public class Store : ISoftDeletable, IIdentifyable
    {
        private DateTime? _creationDateTime;
        private string _apiToken;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Display(Name = "Filialname")] public string Name { get; set; }
        [Display(Name = "Stadt")] public string City { get; set; }
        [Display(Name = "Straße")] public string Street { get; set; }
        [Display(Name = "Hausnummer")] public string HouseNr { get; set; }
        [Display(Name = "Plz")] public string Plz { get; set; }

        public int PointsPerEuro { get; set; }
        public decimal EuroPerPoints { get; set; }

        public string StoreWebSite { get; set; }


        [Display(Name = "Erstellt am")]
        public DateTime CreationDateTime
        {
            get => this._creationDateTime ?? DateTime.Now;
            set => _creationDateTime = value;
        }

        [Display(Name = "ApiToken")]
        public string ApiToken
        {
            get => this._apiToken ?? GenerateToken();
            set => this._apiToken = value;
        }

        public virtual ICollection<User> InterestedUsers { get; set; }
        public virtual ICollection<Device> Devices { get; set; }
        public virtual ICollection<TelNr> TelNrs { get; set; }
        public virtual ICollection<Coupon> Coupons { get; set; }
        public virtual ICollection<TurnoverJournal> TurnoverJournals { get; set; }
        public virtual ICollection<Redeem> Redeems { get; set; }
        public virtual ICollection<CashRegister> CashRegisters { get; set; }
        public virtual ICollection<Tag> Tags { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }

        [DisplayName("Firma")] public Guid CompanyId { get; set; }
        public virtual Company Company { get; set; }
        [DisplayName("Staat")] public Guid CountryId { get; set; }
        public virtual Country Country { get; set; }
        public virtual EBillCategory EBillCategory { get; set; }
        public bool IsDeleted { get; set; }
        [Display(Name = "Breitengrad")] public double Latitude { get; set; }
        [Display(Name = "Längengrad")] public double Longitude { get; set; }

        public Guid GetId()
        {
            return Id;
        }

        private string GenerateToken()
        {
            var maxSize = 64;
            char[] chars = new char[62];
            chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            byte[] data = new byte[1];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[maxSize];
                crypto.GetNonZeroBytes(data);
            }

            StringBuilder result = new StringBuilder(maxSize);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }

            return result.ToString();
        }
    }
}