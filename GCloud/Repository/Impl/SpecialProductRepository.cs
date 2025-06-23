using GCloud.Models.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace GCloud.Repository.Impl
{
    public class SpecialProductRepository : AbstractRepository<SpecialProduct>, ISpecialProductRepository
    {
        public SpecialProductRepository(DbContext context) : base(context)
        {
        }
    }
}