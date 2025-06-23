using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCloud.Models.Domain;
using GCloud.Repository;
using GCloud.Service;

namespace GCloud.Service
{
    public interface ICountryService : IAbstractService<Country>
    {
    }
}
