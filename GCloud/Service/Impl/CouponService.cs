using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using GCloud.Models.Domain;
using GCloud.Repository;
using Microsoft.Ajax.Utilities;
using RefactorThis.GraphDiff;

namespace GCloud.Service.Impl
{
    public class CouponService : AbstractService<Coupon>, ICouponService
    {
        private ICouponRepository _couponRepository;
        private IUserRepository _userRepository;
        private IStoreRepository _storeRepository;

        public CouponService(ICouponRepository repository, IUserRepository userRepository, IStoreRepository storeRepository) : base(repository)
        {
            _couponRepository = repository;
            _userRepository = userRepository;
            _storeRepository = storeRepository;
        }

        public IQueryable<Coupon> FindByInterestedUser(string userId)
        {
            var user = _userRepository.FindById(userId);

            return user.InterrestedStores.SelectMany(s => s.Coupons).AsQueryable();
        }

        public IQueryable<Coupon> FindByInterestedUserAndCompany(string userId, Guid storeGuid)
        {
            var user = _userRepository.FindById(userId);
            return user.InterrestedStores.Where(x => x.Id == storeGuid).SelectMany(s => s.Coupons).AsQueryable();
        }

        public IQueryable<Coupon> FindByStoreId(Guid storeGuid)
        {
            return _storeRepository.FindById(storeGuid).Coupons.AsQueryable();
        }

        public IQueryable<Coupon> FindByManagerId(string managerId)
        {
            return _couponRepository.FindBy(x => x.CreatedUserId == managerId).Include(x => x.UsageActions).Include(x => x.Visibilities);
        }

        public IQueryable<Coupon> FindByNameLike(string searchText)
        {
            return _couponRepository.FindBy(c => c.Name.Contains(searchText));
        }

        public override void Update<TUpdateType>(TUpdateType entity, Expression<Func<IUpdateConfiguration<TUpdateType>, object>> mapping)
        {
            _repository.Update(entity, map => map.OwnedCollection(p => p.UsageActions));
            _repository.Save();

            base.Update(entity, mapping);
        }

        public decimal GetSpecialProductValue(string productId)
        {
            return _couponRepository.FindFirstOrDefault(x => x.Id.ToString() == productId).Value;
        }

    }
}