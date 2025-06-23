using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;

namespace GCloud.Service
{
    public interface ICashbackService : IAbstractService<Cashback>
    {
        Cashback ApplyCashback(string deviceToken, string userId, decimal invoiceAmount, Guid cashRegisterId);
        Cashback UseCashback(string deviceToken, string userId, decimal paymentAmount, decimal invoiceAmount, Guid cashRegisterId);
        IQueryable<Cashback> FindByStoreAndUser(string userId, Guid storeId);
    }
}