using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;
using GCloud.Repository;
using GCloud.Repository.Impl;

namespace GCloud.Service.Impl
{
    public class CompanyService : AbstractService<Company>, ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;

        public CompanyService(ICompanyRepository repository) : base(repository)
        {
           this._companyRepository = repository;
        }

        public int GetRegistrationPoints()
        {
          return  _companyRepository.FindFirstOrDefault(x => x.RegistrationPoints != -1).RegistrationPoints;

        }

        public string GetManagerIdByCompanyName(string name)
        {
            return _companyRepository.FindFirstOrDefault(x => x.Name == name).UserId;
        }
    }
}