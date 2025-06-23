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
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GCloud.App_Start;
using GCloud.Extensions;
using GCloud.Helper;
using GCloud.Models.Domain;
using GCloud.Service;
using GCloud.Shared.Dto.Domain;
using GCloud.Shared.Exceptions.Store;
using GCloud.Shared.Exceptions.User;
using Microsoft.AspNet.Identity;
using RefactorThis.GraphDiff;

namespace GCloud.Controllers.api
{
    [RoutePrefix("api/UserStoresApi")]
    public class UserStoresApiController : ApiController
    {
        private IStoreService _storeService;
        private IUserService _userService;

        public UserStoresApiController(IStoreService storeService, IUserService userService)
        {
            _storeService = storeService;
            _userService = userService;
        }

        [Authorize(Roles = "Administrators,Managers")]
        [HttpGet]
        public HttpResponseMessage GetQrCode(Guid id)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var image = QrCodeHtmlHelper.GetQrCodeImage(id.ToString(), 500, 1);

            using (var ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Png);
                response.Content = new ByteArrayContent(ms.ToArray());
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                return response;
            }
        }

        [Authorize(Roles = "Customers,Managers")]
        public List<StoreDto> Get(bool updateImages = true)
        {
            var user = _userService.FindById(User.Identity.GetUserId());

            if (user == null)
            {
                throw new UserNotFoundException(User.Identity.GetUserId());
            }

            return user.InterrestedStores.ToList().Select(x => AutoMapper.Mapper.Map<StoreDto>(x, opts =>
            {
                opts.Items.Add(AutomapperConfig.UserId, User.Identity.GetUserId());
                opts.Items.Add(AutomapperConfig.IncludeBanner, updateImages);
            })).ToList();
        }

        [Route("GetManagerStores")]
        [Authorize(Roles = "Managers")]
        public List<StoreDto> GetManagerStores(bool updateImages = true)
        {
            var user = _userService.FindById(User.Identity.GetUserId());

            if (user == null)
            {
                throw new UserNotFoundException(User.Identity.GetUserId());
            }

            return user.Companies.AsQueryable().SelectMany(c => c.Stores).ToList().Select(x => Mapper.Map<StoreDto>(x,
                opts =>
                {
                    opts.Items.Add(AutomapperConfig.UserId, User.Identity.GetUserId());
                    opts.Items.Add(AutomapperConfig.IncludeBanner, updateImages);
                })).ToList();
        }

        [Authorize(Roles = "Customers,Managers")]
        public HttpResponseMessage Put(string id)
        {
            if (Guid.TryParse(id, out var guid))
            {
                var store = _storeService.FindById(guid);
                if (store == null)
                {
                    throw new StoreNotFoundException(guid);
                }
                var user = _userService.FindById(User.Identity.GetUserId());
                if (user == null)
                {
                    throw new UserNotFoundException(User.Identity.GetUserId());
                }

                user.InterrestedStores.Add(store);
                _userService.Update(user);

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            throw new FormatException($"{id} ist keine gültige Guid.");
        }

        [Authorize(Roles = "Customers,Managers")]
        public HttpResponseMessage Delete(Guid id)
        {
            var store = _storeService.FindById(id);
            if (store == null)
            {
                throw new StoreNotFoundException(id);
            }
            var user = _userService.FindById(User.Identity.GetUserId());
            if (user == null)
            {
                throw new UserNotFoundException(User.Identity.GetUserId());
            }

            user.InterrestedStores.Remove(store);
            _userService.Update(user);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}