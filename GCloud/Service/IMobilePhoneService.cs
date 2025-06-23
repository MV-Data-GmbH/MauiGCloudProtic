using System;
using System.Linq;
using GCloud.Models.Domain;

namespace GCloud.Service
{
    public interface IMobilePhoneService : IAbstractService<MobilePhone>
    {
        MobilePhone CreateNewDevice(string userId, string firebaseInstanceId);

        IQueryable<AnonymousMobilePhone> GetAnonymousMobilePhones();
        AnonymousMobilePhone GetAnonymousMobilePhoneById(Guid id);

        bool RemoveDevice(string userId, Guid deviceId);

        IQueryable<MobilePhone> GetMobilePhones();
    }
}