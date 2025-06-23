using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GCloud.Models.Domain
{
    public class Tag
    {
        private DateTime? _creationDatetime;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreationDateTime { get => _creationDatetime ?? DateTime.Now; set => _creationDatetime = value; }

        public virtual ICollection<Store> Stores { get; set; }
    }
}