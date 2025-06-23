using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using GCloud.Models.Domain;

namespace GCloud.Controllers.ViewModels.Store
{
    public class StoreEditViewModel
    {
        public Guid Id { get; set; }
        [Display(Name = "Filialname")]
        public string Name { get; set; }
        [Display(Name = "Stadt")]
        public string City { get; set; }
        [Display(Name = "Straße")]
        public string Street { get; set; }
        [Display(Name = "Hausnummer")]
        public string HouseNr { get; set; }
        [Display(Name = "Plz")]
        public string Plz { get; set; }
        [Display(Name = "Erstellt am")]
        public DateTime CreationDateTime { get; set; }
        [DisplayName("Filial-Banner")]
        public String ImageData { get; set; }
        [Display(Name = "ApiToken")]
        public string ApiToken { get; set; }
        public virtual ICollection<Models.Domain.User> InterestedUsers { get; set; }
        public virtual ICollection<Device> Devices { get; set; }
        public virtual ICollection<TelNr> TelNrs { get; set; }
        public virtual ICollection<Models.Domain.Coupon> Coupons { get; set; }
        public virtual ICollection<TurnoverJournal> TurnoverJournals { get; set; }
        public virtual ICollection<Redeem> Redeems { get; set; }
        public List<string> SelectCoupons { get; set; }
        [DisplayName("E-Bill-Kategorie")]
        public string EBillCategory { get; set; }
        [DisplayName("Tags")]
        public List<string> SelectedTags { get; set; }
        [DisplayName("Firma")]
        public Guid CompanyId { get; set; }
        public virtual Models.Domain.Company Company { get; set; }
        public Guid CountryId { get; set; }
        public virtual Country Country { get; set; }

        [DisplayName("Euro per Points")]
        public decimal EuroPerPoints { get; set; }
        [DisplayName("Points per Euro")]
        public int PointsPerEuro { get; set; }

        public bool IsDeleted { get; set; }
    }
}