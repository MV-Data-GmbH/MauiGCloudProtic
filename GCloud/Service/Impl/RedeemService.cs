using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using GCloud.Models.Domain;
using GCloud.Repository;
using GCloud.Repository.Impl;
using GCloud.Service;
using GCloud.Service.Impl;
using GCloud.Shared.Exceptions.Coupon;
using GCloud.Shared.Exceptions.User;

namespace GCloud.Service.Impl
{
    public class RedeemService : AbstractService<Redeem>, IRedeemService
    {
        private readonly IRedeemRepository _redeemRepository;
        private readonly ICouponRepository _couponRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICashbackService _cashbackService;
        public RedeemService(IAbstractRepository<Redeem> repository, ICouponRepository couponRepository, IUserRepository userRepository, ICashbackService cashbackService) : base(repository)
        {
            _redeemRepository = (IRedeemRepository) repository;
            _couponRepository = couponRepository;
            _userRepository = userRepository;
            _cashbackService = cashbackService;
        }

        public Redeem RedeemCoupon(Redeem redeem)
        {
            var targetCoupon = _couponRepository.FindById(redeem.CouponId);
            var targetUser = _userRepository.FindById(redeem.UserId);

            if (targetCoupon == null)
            {
                throw new CouponNotFoundException(redeem.CouponId);
            }

            if (targetUser == null)
            {
                throw new UserNotFoundException(redeem.UserId);
            }

            //Anzahl der Einlösungen von dem anzuwendenden User
            var redeemsCount = _redeemRepository.FindBy(x => x.CouponId == redeem.CouponId && x.UserId == redeem.UserId).Count();

            if (redeemsCount >= (targetCoupon.MaxRedeems ?? int.MaxValue))
            {
                throw new AlreadyRedeemedException(targetCoupon.Id);
            }

            _redeemRepository.Add(redeem);
            _redeemRepository.Save();
            return redeem;
        }
    }
}