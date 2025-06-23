using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;
using GCloud.Repository;

namespace GCloud.Repository.Impl
{
    public class StoreRepository : AbstractRepository<Store>, IStoreRepository
    {
        public StoreRepository(DbContext context) : base(context)
        {
        }
    }
}