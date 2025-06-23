
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using GCloud.Models;

namespace GCloud.Models.Domain
{
    public class TelNr : ISoftDeletable, IIdentifyable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string TelephoneNumber { get; set; }

        public Guid StoreId { get; set; }
        public virtual Store Store { get; set; }
        public bool IsDeleted { get; set; }

        public Guid GetId()
        {
            return Id;
        }
    }
}