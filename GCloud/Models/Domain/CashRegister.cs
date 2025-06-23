using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GCloud.Models.Domain
{
    public class CashRegister : IIdentifyable
    {
        private DateTime? _creationDateTime;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreationDateTime { get => _creationDateTime ?? DateTime.Now; set => _creationDateTime = value; }
        public string MacAddress { get; set; }
        public string PublicIpAddress { get; set; }
        


        public Guid StoreId { get; set; }
        public virtual Store Store { get; set; }

        public virtual ICollection<Bill> Bills { get; set; }
        public bool IsDeleted { get; set; }


        public Guid GetId()
        {
            return Id;
        }
    }
}