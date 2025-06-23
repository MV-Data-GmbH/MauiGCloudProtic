using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GCloud.Extensions
{
    public static class DateTimeExtenstions
    {
        public static bool IsBetween(this TimeSpan instance, TimeSpan from, TimeSpan to, bool excludeUpperBound = true)
        {
            if (excludeUpperBound)
            {
                return instance >= from && instance < to;
            }

            return instance >= @from && instance <= to;
        }
    }
}