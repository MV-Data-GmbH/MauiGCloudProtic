using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GCloud.Models.Domain
{
    public class Notification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [Column(@"DayOfWeek", TypeName = "int")]
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan NotifyTime { get; set; }
        public string Message { get; set; }

        public virtual Store Store { get; set; }
        public Guid StoreId { get; set; }
    }
}