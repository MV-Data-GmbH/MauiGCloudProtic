using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;
using GCloud.Repository;
using GCloud.Service.Impl;

namespace GCloud.Service.Impl
{
    public class CountryService : AbstractService<Country>, ICountryService
    {
        private readonly ICountryRepository _countryRepository;


        public CountryService(ICountryRepository countryRepository) : base(countryRepository)
        {
            _countryRepository = countryRepository;
        }
    }
}