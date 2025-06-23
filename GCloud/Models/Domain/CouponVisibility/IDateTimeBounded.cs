using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCloud.Models.Domain
{
    public interface IDateTimeBounded
    {
        DateTime? GetValidFrom(string userId);
        DateTime? GetValidTo(string userId);
    }
}
