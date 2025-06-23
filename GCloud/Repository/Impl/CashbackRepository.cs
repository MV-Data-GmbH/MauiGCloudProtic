using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;
using GCloud.Repository;

namespace GCloud.Repository.Impl
{
    public class CashbackRepository : AbstractRepository<Cashback>, ICashbackRepository
    {
        public CashbackRepository(DbContext context) : base(context)
        {
        }

        public Cashback FindCurrectCashback(string userId, Guid storeId)
        {
            return FindBy(x => x.UserId == userId && x.StoreId == storeId).OrderByDescending(x => x.CreditDateTime).FirstOrDefault();
        }
    }
}