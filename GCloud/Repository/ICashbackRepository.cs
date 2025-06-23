using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;
using GCloud.Repository;

namespace GCloud.Repository
{
    public interface ICashbackRepository : IAbstractRepository<Cashback>
    {
        Cashback FindCurrectCashback(string userId, Guid storeId);
    }
}