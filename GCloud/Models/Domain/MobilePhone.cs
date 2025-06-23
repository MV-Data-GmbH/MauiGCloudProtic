using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using GCloud.Models;

namespace GCloud.Models.Domain
{
    public class MobilePhone : ISoftDeletable, IIdentifyable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string FirebaseInstanceId { get; set; }

        public string UserId { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<FirebaseNotification> FirebaseNotifications { get; set; }

        public bool IsDeleted { get; set; }
        public Guid GetId()
        {
            return Id;
        }
    }
}