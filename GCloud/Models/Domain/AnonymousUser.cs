using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GCloud.Models.Domain
{
    [Table("AnonymousUsers", Schema = "anonymous")]
    public class AnonymousUser : IIdentifyable
    {
        private DateTime? _creationDateTime = null;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public DateTime CreationDateTime { get => _creationDateTime ?? DateTime.Now; set => _creationDateTime = value; }

        public virtual User User { get; set; }
        public string UserId { get; set; }

        public virtual ICollection<AnonymousMobilePhone> AnonymousMobilePhones { get; set; }
        public virtual ICollection<Bill> Bills { get; set; }

        public bool IsDeleted { get; set; }
        public Guid GetId() => Id;
    }
}