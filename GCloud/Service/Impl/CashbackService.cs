using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using GCloud.Models.Domain;
using GCloud.Repository;
using GCloud.Shared.Exceptions.Cashback;
using GCloud.Shared.Exceptions.Cashier;
using GCloud.Shared.Exceptions.Store;
using GCloud.Shared.Exceptions.User;
using LinqKit;

namespace GCloud.Service.Impl
{
    public class CashbackService : AbstractService<Cashback>, ICashbackService
    {
        private readonly IStoreService _storeService;
        private readonly IUserService _userService;
        private readonly ICashbackRepository _cashbackRepository;
        private readonly IStoreRepository _storeRepository;

        public CashbackService(ICashbackRepository repository, IStoreService storeService, IUserService userService, IStoreRepository storeRepository) : base(repository)
        {
            _storeService = storeService;
            _userService = userService;
            _cashbackRepository = repository;
            _storeRepository = storeRepository;
        }

        public Cashback ApplyCashback(string deviceToken, string userId, decimal invoiceAmount, Guid cashRegisterId)
        {
            var store = _storeService.FindByApiToken(deviceToken);
            var user = _userService.FindById(userId);

            if (store == null)
            {
                throw new ApiTokenInvalidException(deviceToken);
            }

            if (user == null)
            {
                throw new UserNotFoundException(userId);
            }

            if (store.CashRegisters.All(cr => cr.Id != cashRegisterId))
            {
                throw new CashRegisterNotInStoreException(cashRegisterId);
            }

            if (store.CashRegisters.All(cr => cr.Id != cashRegisterId))
            {
                throw new CashRegisterNotInStoreException(cashRegisterId);
            }

            var lastCashback = _cashbackRepository.FindCurrectCashback(userId, store.Id);

            var incrementCashbackValue = decimal.Divide(Math.Floor(decimal.Multiply(decimal.Multiply(invoiceAmount, 0.1m),100)),100);

            var now = DateTime.Now;

            var cashback = new Cashback
            {
                StoreId = store.Id,
                TurnoverChange = invoiceAmount,
                TurnoverOld = lastCashback?.TurnoverNew ?? 0,
                TurnoverNew = (lastCashback?.TurnoverNew ?? 0) + invoiceAmount,
                UserId = userId,
                CreditDateTime = now,
                CreditOld = lastCashback?.CreditNew ?? 0,
                CreditChange = incrementCashbackValue,
                CreditNew = (lastCashback?.CreditNew ?? 0) + incrementCashbackValue
            };

            _cashbackRepository.Add(cashback);
            _cashbackRepository.Save();

            return cashback;
        }

        public Cashback UseCashback(string deviceToken, string userId, decimal paymentAmount, decimal invoiceAmount, Guid cashRegisterId)
        {
            var store = _storeService.FindByApiToken(deviceToken);
            var user = _userService.FindById(userId);

            if (store == null)
            {
                throw new ApiTokenInvalidException(deviceToken);
            }

            if (user == null)
            {
                throw new UserNotFoundException(userId);
            }

            var lastCashback = _cashbackRepository.FindCurrectCashback(userId, store.Id);

            if (lastCashback == null)
            {
                throw new CashbackNotFoundException(null);
            }

            if (lastCashback.CreditNew < paymentAmount)
            {
                throw new NoLastCashbackException(userId, store.Id);
            }

            lastCashback = ApplyCashback(deviceToken, userId, invoiceAmount, cashRegisterId);

            var newCashback = new Cashback
            {
                CreditOld = lastCashback.CreditNew,
                CreditChange = decimal.Negate(paymentAmount),
                CreditDateTime = DateTime.Now,
                CreditNew = lastCashback.CreditNew + decimal.Negate(paymentAmount),
                TurnoverOld = (decimal) lastCashback?.TurnoverNew,
                TurnoverChange = invoiceAmount,
                TurnoverNew = (decimal) lastCashback?.TurnoverNew + invoiceAmount,
                UserId = userId,
                StoreId = store.Id
            };

            _cashbackRepository.Add(newCashback);
            _cashbackRepository.Save();

            return newCashback;
        }

        public IQueryable<Cashback> FindByStoreAndUser(string userId, Guid storeId)
        {
            var companyId = _storeRepository.FindFirstOrDefault(x => x.Id == storeId)?.CompanyId;

            return _repository.FindBy(x => x.CreditChange != 0 && x.UserId == userId && x.Store.CompanyId == companyId);
        }
    }
}