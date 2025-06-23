using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GCloud.Models
{
    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }
    }
}