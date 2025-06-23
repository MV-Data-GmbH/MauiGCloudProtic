using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;

namespace GCloud.Repository.Impl
{
    public class CashRegisterRepository : AbstractRepository<CashRegister>, ICashRegisterRepository
    {
        public CashRegisterRepository(DbContext context) : base(context)
        {
        }
    }
}