using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using GCloud.Models;

namespace GCloud.Models.Domain
{
    public class CouponImage : ISoftDeletable, IIdentifyable
    {
        private DateTime? _creationDateTime;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string OrigFileName { get; set; }
        public DateTime CreationDateTime
        {
            get => _creationDateTime ?? DateTime.Now;
            set => _creationDateTime = value;
        }

        public Guid? CouponId { get; set; }
        public virtual Coupon Coupon { get; set; }
        public string CreatorId { get; set; }
        public User Creator { get; set; }
        public bool IsDeleted { get; set; }

        public Guid GetId()
        {
            return Id;
        }
    }
}