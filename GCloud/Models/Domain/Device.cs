using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using GCloud.Models;

namespace GCloud.Models.Domain
{
    public class Device : ISoftDeletable, IIdentifyable
    {
        private DateTime? _creationDateTime;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreationDateTime
        {
            get => this._creationDateTime ?? DateTime.Now;
            set => _creationDateTime = value;
        }

        public Guid StoreId { get; set; }
        public virtual Store Store { get; set; }
        public bool IsDeleted { get; set; }

        public Guid GetId()
        {
            return Id;
        }
    }
}