using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GCloud.Models.Domain
{
    public class FirebaseNotification : ISoftDeletable, IIdentifyable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public Guid GetId() => Id;

        public virtual MobilePhone Device { get; set; }
        public Guid? DeviceId { get; set; }

        public virtual AnonymousMobilePhone AnonymousMobilePhone { get; set; }
        public Guid? AnonymousMobilePhoneId { get; set; }

        public string Title { get; set; }
        public string Body { get; set; }
        public string Type { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime LastAttemptOn { get; set; }
        public bool Sent { get; set; }
        [NotMapped]
        public Guid BillId { get; set; }

        public bool IsDeleted { get; set; }
    }
}