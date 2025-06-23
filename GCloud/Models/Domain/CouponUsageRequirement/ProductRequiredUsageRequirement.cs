using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GCloud.Models.Domain.CouponUsageAction;

namespace GCloud.Models.Domain.CouponUsageRequirement
{
    public class ProductRequiredUsageRequirement : AbstractUsageRequirement
    {
        private List<string> _requiredProducts = new List<string>();

        /// <summary>
        /// A List of Products that you need to use
        /// </summary>
        public List<string> RequiredProducts { get => _requiredProducts; set => _requiredProducts = value; }

        public string RequiredProductsString { get => string.Join(",", _requiredProducts); set => _requiredProducts = value?.Split(',').ToList() ?? new List<string>(); }

        /// <summary>
        /// Describes the coupling of the <see cref="RequiredProducts"/> Entities. Do you need All products (AND) or Any (OR) of the products to meet the Requirement
        /// </summary>
        public Coupling PredicateCoupling { get; set; } = Coupling.Or;

    }
}