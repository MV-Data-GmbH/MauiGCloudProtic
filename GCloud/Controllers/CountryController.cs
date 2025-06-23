using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Autofac.Integration.WebApi;
using GCloud.Models.Domain;
using GCloud.Service;

namespace GCloud.Controllers
{
    [AutofacControllerConfiguration]
    public class CountryController : ApiController
    {
        private readonly ICountryService _countryService;

        public CountryController(ICountryService countryService)
        {
            _countryService = countryService;
        }

        [HttpGet()]
        [Authorize]
        public List<Country> Index()
        {
            return _countryService.FindAll().ToList();
        }
    }
}
