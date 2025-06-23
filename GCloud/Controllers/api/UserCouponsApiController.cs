using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GCloud.App_Start;
using GCloud.Models.Domain;
using GCloud.Service;
using Microsoft.AspNet.Identity;
using GCloud.Helper;
using Newtonsoft.Json;
using GCloud.Extensions;
using GCloud.Shared.Dto.Domain;
using GCloud.Shared.Exceptions.User;
using Quartz.Spi;

namespace GCloud.Controllers.api
{
    [RoutePrefix("api/UserCouponsApi")]
    public class UserCouponsApiController : ApiController
    {
        private readonly ICouponService _couponService;
        private readonly IUserService _userService;

        public UserCouponsApiController(ICouponService couponService, IUserService userService)
        {
            _couponService = couponService;
            _userService = userService;
        }

        [Authorize(Roles = "Managers,Customers")]
        public List<CouponDto> Get(bool skipUserValidation = false)
        {
            var currentUser = _userService.FindById(User.Identity.GetUserId());
            if (currentUser == null)
            {
                throw new UserNotFoundException(User.Identity.GetUserId());
            }

            var coupons = _couponService.FindByInterestedUser(currentUser.Id);
            coupons = !skipUserValidation ? coupons.WhereIsCurrentlyValid(User.Identity.GetUserId()) : coupons.WhereDateIsValid();
            var cs = coupons.ToList().Select(c => AutoMapper.Mapper.Map<CouponDto>(c, opts => opts.Items.Add(AutomapperConfig.UserId, User.Identity.GetUserId()))).ToList();

            return cs;
        }

        [Authorize(Roles = "Managers,Customers")]
        public CouponDto Get(Guid id)
        {
            var currentUser = _userService.FindById(User.Identity.GetUserId());
            if (currentUser == null)
            {
                throw new UserNotFoundException(User.Identity.GetUserId());
            }

            return Mapper.Map<CouponDto>(_couponService.FindById(id), opt => opt.Items.Add(AutomapperConfig.UserId,currentUser.Id));
        }

        [HttpGet]
        [Route("LoadCouponQrCode/{id}")]
        [Authorize(Roles = "Administrators,Managers,Customers")]
        public HttpResponseMessage LoadCouponQrCode(Guid id)
        {
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            var userCoupon = new UserCouponDto {CouponId = id.ToString(), UserId = User.Identity.GetUserId()};
            var image = QrCodeHtmlHelper.GetQrCodeImage(JsonConvert.SerializeObject(userCoupon), 250, 1);

            using (var ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Png);
                result.Content = new ByteArrayContent(ms.ToArray());
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                return result;
            }
        }

        [HttpGet]
        //[Authorize(Roles = "Administrators,Managers,Customers")]
        [Route("store/{id}")]
        public List<Coupon> GetCouponsByStoreGuid(Guid id, bool skipUserValidation = true, bool includeImage = true)
        {
        
                return _couponService.FindByStoreId(id)
 
                    .ToList();
           

          

        }

        //[HttpGet]
        ////[Authorize(Roles = "Administrators,Managers,Customers")]
        //[Route("store/{id}")]
        //public List<CouponDto> GetCouponsByStoreGuid(Guid id, bool skipUserValidation = true, bool includeImage = true)
        //{
        //    if (skipUserValidation)
        //    {
        //        return _couponService.FindByStoreId(id)
        //            .Include(c => c.Redeems)
        //            .ToList()
        //            .Select(c => Mapper.Map<CouponDto>(c, opts =>
        //            {
        //                opts.Items.Add(AutomapperConfig.UserId, User.Identity.GetUserId());
        //                opts.Items.Add(AutomapperConfig.IncludeImage, includeImage);
        //            }))
        //            .ToList();
        //    }

        //    return _couponService.FindByStoreId(id)
        //        .WhereIsCurrentlyValid(User.Identity.GetUserId())
        //        .Include(c => c.Redeems)
        //        .ToList()
        //        .Select(c => Mapper.Map<CouponDto>(c, opts =>
        //        {
        //            opts.Items.Add(AutomapperConfig.UserId, User.Identity.GetUserId());
        //            opts.Items.Add(AutomapperConfig.IncludeImage, includeImage);
        //        }))
        //        .ToList();

        //}



        [HttpGet]
        [Route("GetManagerCoupons")]
        [Authorize(Roles = "Managers")]
        public List<CouponDto> GetManagerCoupons()
        {
            var user = _userService.FindById(User.Identity.GetUserId());

            if (user == null)
            {
                throw new UserNotFoundException(User.Identity.GetUserId());
            }

            return _couponService
                .FindByManagerId(user.Id)
                .ToList()
                .Select(x => Mapper.Map<CouponDto>(x, opts => opts.Items.Add(AutomapperConfig.IncludeImage, true)))
                .ToList();
        }

        private class UserCouponDto
        {
            public string UserId { get; set; }
            public string CouponId { get; set; }
        }
    }
}