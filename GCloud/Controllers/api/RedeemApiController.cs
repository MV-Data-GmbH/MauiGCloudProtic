using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using GCloud.Models.Domain;
using GCloud.Service;
using GCloud.Shared.Dto.Api;
using Microsoft.AspNet.Identity;

namespace GCloud.Controllers.api
{
    public class RedeemApiController : ApiController
    {
        private readonly IUserService _userService;
        private readonly IRedeemService _redeemService;

        public RedeemApiController(IUserService userService, IRedeemService redeemService)
        {
            _userService = userService;
            _redeemService = redeemService;
        }

        public void Put(RedeemRequestModel model)
        {
            var redeem = new Redeem
            {
                CashValue = model.CashValue,
                CouponId = model.CouponId,
                CouponValue = model.CouponValue,
                RedeemDateTime = model.RedeemDateTime ?? DateTime.Now,
                UserId = model.UserGuid.ToString(),
            };

            _redeemService.RedeemCoupon(redeem);
        }
    }
}
