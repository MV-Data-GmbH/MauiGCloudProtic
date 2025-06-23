using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GCloud.App_Start;
using GCloud.Service;
using GCloud.Extensions;
using GCloud.Models.Domain;
using GCloud.Shared.Dto.Api;
using GCloud.Shared.Dto.Domain;
using GCloud.Shared.Exceptions.Cashier;
using GCloud.Shared.Exceptions.Coupon;
using GCloud.Shared.Exceptions.Store;
using GCloud.Shared.Exceptions.User;
using Microsoft.AspNet.Identity;

namespace GCloud.Controllers.api
{
    public class StoreCouponsApiController : ApiController
    {
        private readonly IUserService _userService;
        private readonly IStoreService _storeService;
        private readonly ICouponService _couponService;
        private readonly IRedeemService _redeemService;
        private readonly ITurnoverJournalService _turnoverJournalService;

        public StoreCouponsApiController(IUserService userService, IStoreService storeService, ICouponService couponService, IRedeemService redeemService, ITurnoverJournalService turnoverJournalService)
        {
            _userService = userService;
            _storeService = storeService;
            _couponService = couponService;
            _redeemService = redeemService;
            _turnoverJournalService = turnoverJournalService;}

        [AllowAnonymous]
        public List<CouponDto> Get(string storeApiToken, string userId, Guid cashRegisterId)
        {
            var store = _storeService.FindByApiToken(storeApiToken);
            if (store == null)
            {
                throw new ApiTokenInvalidException(storeApiToken);
            }

            var user = _userService.FindById(userId);
            if (user == null)
            {
                throw new UserNotFoundException(userId);
            }

            if (store.CashRegisters.All(cr => cr.Id != cashRegisterId))
            {
                throw new CashRegisterNotInStoreException(cashRegisterId);
            }

            //return _couponService.FindByStoreId(store.Id)
            //    .Include(x => x.Redeems)
            //    .WhereIsCurrentlyValid(userId)
            //    .ToList()
            //    .Select(c => Mapper.Map<CouponDto>(c, opts => opts.Items[AutomapperConfig.UserId] = User.Identity.GetUserId()))
            //    .ToList();

            var coupons = _couponService.FindByStoreId(store.Id)
                .Include(x => x.Redeems)
                .WhereIsCurrentlyValid(userId)
                .ToList()
                .Select(c => Mapper.Map<CouponDto>(c, opts => opts.Items[AutomapperConfig.UserId] = User.Identity.GetUserId()))
                .ToList();

            foreach (CouponDto coupon in coupons)
            {
                if (coupon.CouponType == CouponTypeDto.Points)
                {
                    coupon.Value = Convert.ToDecimal(user.TotalPoints);
                }
            }
            return coupons;
        }

        [AllowAnonymous]
        public CouponDto Get(Guid couponId, string userId, string storeApiToken, Guid cashRegisterId)
        {
            var store = _storeService.FindByApiToken(storeApiToken);
            if (store == null)
            {
                throw new ApiTokenInvalidException(storeApiToken);
            }

            var coupon = store.Coupons.FirstOrDefault(x => x.Id == couponId);

            if (coupon == null)
            {
                throw new CouponNotFoundException(couponId);
            }

            var user = _userService.FindById(userId);
            if (user == null)
            {
                throw new UserNotFoundException(userId);
            }

            if (coupon.CouponType == CouponType.Points)
            {
                coupon.Value = Convert.ToDecimal(user.TotalPoints);
            }

            if (store.CashRegisters.All(cr => cr.Id != cashRegisterId))
            {
                throw new CashRegisterNotInStoreException(cashRegisterId);
            }

            return Mapper.Map<CouponDto>(coupon, opts => opts.Items.Add(AutomapperConfig.UserId, userId));
        }

        [AllowAnonymous]
        public void Put(StoreCouponApiBindingModel.StoreCouponApiPutModel model)
        {
            var store = _storeService.FindByApiToken(model.StoreApiToken);
            if (store == null)
            {
                throw new ApiTokenInvalidException(model.StoreApiToken);
            }

            var user = _userService.FindById(model.UserGuid.ToString());
            if (user == null)
            {
                throw new UserNotFoundException(model.UserGuid.ToString());
            }

            var coupon = _couponService.FindById(model.CouponId);
            if (coupon == null)
            {
                throw new CouponNotFoundException(model.CouponId);
            }

            if (store.CashRegisters.All(cr => cr.Id != model.CashRegisterId))
            {
                throw new CashRegisterNotInStoreException(model.CashRegisterId);
            }

            var redeem = new Redeem
            {
                CashValue = model.CashValue,
                CouponId = coupon.Id,
                CouponValue = coupon.Value,
                RedeemDateTime = model.RedeemDateTime,
                RedeemedStoreId = store.Id,
                UserId = user.Id
            };

            //check if the users already follows the store
            if (user.InterrestedStores.All(x => x.Id != store.Id))
            {
                user.InterrestedStores.Add(store);
                _userService.Update(user);
            }

            _redeemService.RedeemCoupon(redeem);

            if (model.InvoiceTurnover.HasValue)
            {
                _turnoverJournalService.Add(store.Id, user.Id, model.InvoiceTurnover.Value);
            }
        }
    }
}
