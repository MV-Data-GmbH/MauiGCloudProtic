using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper.QueryableExtensions;
using GCloud.Extensions;
using GCloud.Models.Domain;
using GCloud.Repository;
using GCloud.Service;
using GCloud.Shared.Dto.Api;
using GCloud.Shared.Exceptions;
using GCloud.Shared.Exceptions.General;
using GCloud.Shared.Exceptions.Home;
using GCloud.Shared.Exceptions.User;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace GCloud.Controllers.api.CashierApi
{
    [RoutePrefix("api/Config")]
    public class StoreConfigApiController : ApiController
    {
        public ApplicationUserManager UserManager => Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

        private readonly IUserRepository _userRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IStoreService _storeService;
        private readonly ICashRegisterRepository _cashRegisterRepository;

        public StoreConfigApiController(IUserRepository userRepository, IStoreRepository storeRepository, ICashRegisterRepository cashRegisterRepository, IStoreService storeService)
        {
            _userRepository = userRepository;
            _storeRepository = storeRepository;
            _storeService = storeService;
            _cashRegisterRepository = cashRegisterRepository;
        }

        [Route("AvailableStores")]
        [HttpGet]
        public List<AvailableStoresResponse> GetAvailableStores(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new GustavArgumentException(nameof(username), password);
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new GustavArgumentNullException(nameof(password));
            }

            var user = _userRepository.FindBy(x => x.UserName == username).SingleOrDefault();

            if (user == null)
            {
                throw new UserNotFoundException(username);
            }

            if (UserManager.CheckPassword(user, password))
            {
                return _storeRepository.FindBy(store => store.Company.UserId == user.Id).ProjectTo<AvailableStoresResponse>().ToList();
            }
            else
            {
                throw new CredentialsWrongException();
            }

        }

        [HttpPut]
        [Route("ApiToken/{storeGuid}")]
        public DeviceConfigResponse GetStoreApiToken(string username, string password, Guid storeGuid, string macAddress, string deviceName)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new GustavArgumentException(nameof(username), username);
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new GustavArgumentNullException(nameof(password));
            }

            var user = _userRepository.FindBy(x => x.UserName == username).SingleOrDefault();

            if (user == null)
            {
                throw new UserNotFoundException(username);
            }

            if (UserManager.CheckPassword(user, password))
            {
                var store = _storeRepository.FindById(storeGuid);
                var cashRegister = new CashRegister
                {
                    MacAddress = macAddress,
                    Name = deviceName,
                    PublicIpAddress = Request.GetClientIpAddress(),
                    StoreId = store.Id
                };
                _storeService.AssignDeviceToStore(store.Id, cashRegister);
                return new DeviceConfigResponse
                {
                    ApiToken = store.ApiToken,
                    CashRegisterId = cashRegister.Id
                };
            }
            throw new CredentialsWrongException();
        }
    }
}