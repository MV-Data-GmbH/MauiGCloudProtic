using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;

namespace GCloud.Repository.Impl
{
    public class CountryRepository : AbstractRepository<Country>, ICountryRepository
    {
        public CountryRepository(DbContext context) : base(context)
        {
        }
    }
}