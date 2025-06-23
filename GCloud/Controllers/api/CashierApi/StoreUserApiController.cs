using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using GCloud.Service;
using GCloud.Shared.Dto.Domain;
using GCloud.Shared.Exceptions.Cashier;
using GCloud.Shared.Exceptions.Store;

namespace GCloud.Controllers.api.CashierApi
{
    public class StoreUserApiController : ApiController
    {
        private readonly IUserService _userService;
        private readonly IStoreService _storeService;

        public StoreUserApiController(IUserService userService, IStoreService storeService)
        {
            _userService = userService;
            _storeService = storeService;
        }

        public UserDto Get(string storeApiToken, string userId, Guid cashRegisterId)
        {
            var store = _storeService.FindByApiToken(storeApiToken);

            if (store == null)
            {
                throw new ApiTokenInvalidException(storeApiToken);
            }

            if (store.CashRegisters.All(cr => cr.Id != cashRegisterId))
            {
                throw new CashRegisterNotInStoreException(cashRegisterId);
            }

            var user = _userService.FindById(userId);

            return Mapper.Map<UserDto>(user, x => x.Items.Add("storeId", store.Id));
        }
    }
}
