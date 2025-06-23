using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;

namespace GCloud.Service
{
    public interface ICompanyService : IAbstractService<Company>
    {
        int GetRegistrationPoints();

        string GetManagerIdByCompanyName(string name);
    }
}