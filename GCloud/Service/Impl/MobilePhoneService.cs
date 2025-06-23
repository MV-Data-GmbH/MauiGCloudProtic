using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;
using GCloud.Repository;
using GCloud.Service.Impl;

namespace GCloud.Service.Impl
{
    public class MobilePhoneService : AbstractService<MobilePhone>, IMobilePhoneService
    {
        private readonly IMobilePhoneRepository _mobilePhoneRepository;
        private readonly IFirebaseNotificationRepository _firebaseNotificationRepository;
        private readonly IAnonymousMobilePhoneRepository _anonymousMobilePhoneRepository;

        public MobilePhoneService(IAbstractRepository<MobilePhone> repository,
            IFirebaseNotificationRepository firebaseNotificationRepository,
            IAnonymousMobilePhoneRepository anonymousMobilePhoneRepository) : base(repository)
        {
            _mobilePhoneRepository = (IMobilePhoneRepository) repository;
            _firebaseNotificationRepository = firebaseNotificationRepository;
            _anonymousMobilePhoneRepository = anonymousMobilePhoneRepository;
        }

        public MobilePhone CreateNewDevice(string userId, string firebaseInstanceId)
        {
            var mobilePhone = new MobilePhone
            {
                UserId = userId,
                FirebaseInstanceId = firebaseInstanceId
            };
            _mobilePhoneRepository.Add(mobilePhone);
            _mobilePhoneRepository.Save();
            return mobilePhone;
        }

        public IQueryable<AnonymousMobilePhone> GetAnonymousMobilePhones()
        {
            return _anonymousMobilePhoneRepository.FindAll();
        }

        public IQueryable<MobilePhone> GetMobilePhones()
        {
            return _mobilePhoneRepository.FindAll();
        }

        public AnonymousMobilePhone GetAnonymousMobilePhoneById(Guid id)
        {
            return _anonymousMobilePhoneRepository.FindById(id);
        }

        public bool RemoveDevice(string userId, Guid deviceId)
        {
            try
            {
                var toDelete = _firebaseNotificationRepository.FindBy(n => n.DeviceId == deviceId);
                if (toDelete.Any())
                    _firebaseNotificationRepository.Delete(toDelete);

                var p = _mobilePhoneRepository.FindFirstOrDefault(m => m.Id == deviceId);
                if (p != null)
                    _mobilePhoneRepository.Delete(p);

                return true;
            }
            catch
            { return false; }
        }
    }
}