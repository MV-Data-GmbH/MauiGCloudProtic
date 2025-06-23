using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;
using GCloud.Repository;

namespace GCloud.Service.Impl
{
    public class StoreService : AbstractService<Store> ,IStoreService
    {
        private readonly IStoreRepository _storeRepository;
        private readonly ICashRegisterRepository _cashRegisterRepository;

        public StoreService(IStoreRepository storeRepository, ICashRegisterRepository cashRegisterRepository) : base(storeRepository)
        {
            _storeRepository = storeRepository;
            _cashRegisterRepository = cashRegisterRepository;
        }

        public void Add(Store store, DateTime? creationDateTime = null)
        {
            _storeRepository.Add(store);
            _storeRepository.Save();
        }

        public IQueryable<Store> FindByUserId(string userId)
        {
            return _storeRepository.FindBy(x =>x.Company.UserId == userId);
        }

        public Store FindByApiToken(string apiToken)
        {
            return _storeRepository.FindBy(x => x.ApiToken == apiToken).FirstOrDefault();
        }

        public void AssignDeviceToStore(Guid storeId, CashRegister cashRegister)
        {
            if (cashRegister?.MacAddress != null && cashRegister.MacAddress != "02:00:00:00:00:00") //This is the static android MacAddress returned with Android 6 onwards
            {
                var oldCashRegister = _cashRegisterRepository.FindFirstOrDefault(x => x.MacAddress == cashRegister.MacAddress);
                if (oldCashRegister != null)
                {
                    oldCashRegister.StoreId = storeId;
                    oldCashRegister.Name = cashRegister.Name;
                    oldCashRegister.PublicIpAddress = cashRegister.PublicIpAddress;
                    _cashRegisterRepository.Update(oldCashRegister);
                    cashRegister.Id = oldCashRegister.Id;
                    return;
                }
            }

            if (cashRegister != null)
            {
                cashRegister.StoreId = storeId;
                _cashRegisterRepository.Add(cashRegister);
            }
        }

        public void DeleteStore(Guid storeId)
        {
            Store store = _storeRepository.FindFirstOrDefault(x => x.Id == storeId);
            _storeRepository.Delete(store);

        }
    }
}