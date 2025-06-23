using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;

namespace GCloud.Models.Domain
{
    public class TurnoverJournal : IIdentifyable
    {
        private DateTime? _creditDateTime;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public decimal TurnoverOld { get; set; }
        public decimal TurnoverChange { get; set; }
        public decimal TurnoverNew { get; set; }
        public DateTime CreditDateTime { get => _creditDateTime ?? DateTime.Now; set => _creditDateTime = value; }
        public virtual User User { get; set; }
        public string UserId { get; set; }
        public virtual Store Store { get; set; }
        public Guid StoreId { get; set; }
        public bool IsDeleted { get; set; }
        public Guid GetId()
        {
            return Id;
        }
    }
}