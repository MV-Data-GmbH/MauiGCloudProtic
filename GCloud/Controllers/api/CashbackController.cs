using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper.QueryableExtensions;
using GCloud.Models.Domain;
using GCloud.Service;
using GCloud.Shared.Dto.Domain;
using GCloud.Shared.Exceptions.Store;
using Microsoft.AspNet.Identity;

namespace GCloud.Controllers.api
{
    public class CashbackController : ApiController
    {
        private readonly ICashbackService _cashbackService;
        private readonly IStoreService _storeService;

        public CashbackController(ICashbackService cashbackService, IStoreService storeService)
        {
            _cashbackService = cashbackService;
            _storeService = storeService;
        }

        public IList<CashbackDto> Get(string storeGuid)
        {
            if (Guid.TryParse(storeGuid, out Guid guid))
            {
                var store = _storeService.FindById(guid);

                if (store == null)
                {
                    throw new StoreNotFoundException(guid);
                }

                return _cashbackService.FindByStoreAndUser(User.Identity.GetUserId(), guid).ProjectTo<CashbackDto>().ToList();
            }
            return null;
        }
    }
}
