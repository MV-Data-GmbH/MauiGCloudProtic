using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;

namespace GCloud.Repository.Impl
{
    public class EBillCategoryRepository : AbstractRepository<EBillCategory>, IEBillCategoryRepository
    {
        public EBillCategoryRepository(DbContext context) : base(context)
        {
        }
    }
}