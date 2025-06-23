using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GCloud.Models.Domain
{
    public abstract class AbstractCouponVisibility
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public Coupon Coupon { get; set; }

        public AbstractCouponVisibility() { }

        /// <summary>
        /// Evaluiert ob diese Datumseinschränkung für den übergebenen User mit der UserId gilt
        /// </summary>
        /// <param name="userId">Der User für den Die Bedingung überprüft werden soll</param>
        /// <returns>True, wenn die Bedingung greift, also wenn der Gutschein sichtbar sein sollte</returns>
        public abstract bool IsValid(string userId);

        public abstract string GetHumanReadableName();
    }
}