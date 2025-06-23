using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;

namespace GCloud.Service
{
    public interface IStoreService : IAbstractService<Store>
    {
        IQueryable<Store> FindByUserId(string userId);
        Store FindByApiToken(string apiToken);
        void AssignDeviceToStore(Guid storeId,CashRegister cashRegister);

        void DeleteStore(Guid storeId);
    }
}