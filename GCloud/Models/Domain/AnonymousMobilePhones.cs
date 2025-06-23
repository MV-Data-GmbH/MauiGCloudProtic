using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GCloud.Models.Domain
{
    [Table("AnonymousMobilePhones", Schema = "anonymous")]
    public class AnonymousMobilePhone : IIdentifyable
    {
        private DateTime? _creationDateTime = null;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public DateTime CreationDateTime { get => _creationDateTime ?? DateTime.Now; set => _creationDateTime = value; }

        public string FirebaseInstanceId { get; set; }

        public virtual AnonymousUser AnonymousUser { get; set; }
        public Guid AnonymousUserId { get; set; }

        public virtual ICollection<FirebaseNotification> FirebaseNotifications { get; set; }

        public bool IsDeleted { get; set; }
        public Guid GetId() => Id;
    }
}