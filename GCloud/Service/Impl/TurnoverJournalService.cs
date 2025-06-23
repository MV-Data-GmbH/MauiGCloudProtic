using System;
using System.Linq;
using GCloud.Models.Domain;
using GCloud.Repository;
using GCloud.Shared.Exceptions.Store;
using GCloud.Shared.Exceptions.User;

namespace GCloud.Service.Impl
{
    public class TurnoverJournalService : AbstractService<TurnoverJournal>, ITurnoverJournalService
    {
        private readonly IStoreRepository _storeRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICashbackRepository _cashbackRepository;

        public TurnoverJournalService(IAbstractRepository<TurnoverJournal> repository, IStoreRepository storeRepository, IUserRepository userRepository, ICashbackRepository cashbackRepository) : base(repository)
        {
            _storeRepository = storeRepository;
            _userRepository = userRepository;
            _cashbackRepository = cashbackRepository;
        }

        public void Add(Guid storeId, string userId, decimal increaseTurnover)
        {
            var store = _storeRepository.FindById(storeId);

            if (store == null)
            {
                throw new StoreNotFoundException(storeId);
            }

            if (!_userRepository.Exists(userId))
            {
                throw new UserNotFoundException(userId);
            }

            var lastCashback = _cashbackRepository.FindBy(x => x.UserId == userId).OrderByDescending(x => x.CreditDateTime).FirstOrDefault();

            var cashback = new Cashback
            {
                StoreId = storeId,
                UserId = userId,
                TurnoverOld = lastCashback?.TurnoverNew ?? 0,
                TurnoverChange = increaseTurnover,
                TurnoverNew = (lastCashback?.TurnoverNew ?? 0) + increaseTurnover,
                CreditChange = 0,
                CreditNew = lastCashback?.CreditNew ?? 0,
                CreditOld = lastCashback?.CreditOld ?? 0
            };

            _cashbackRepository.Add(cashback);
        }
    }
}