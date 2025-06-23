using GCloud.Models.Domain;
using GCloud.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GCloud.Service.Impl
{
    public class SpecialProductService : AbstractService<SpecialProduct>, ISpecialProductService
    {
        private readonly ISpecialProductRepository _specialProductRepository;

        public SpecialProductService(ISpecialProductRepository specialProductRepository) : base(specialProductRepository)
        {
            _specialProductRepository = specialProductRepository;
        }

        public IQueryable<SpecialProduct> FindByUserId(string userId)
        {
            return _specialProductRepository.FindBy(x => x.CreatedUserId == userId);
        }

        public void DeleteSpecialProduct(Guid specialProductId)
        {
            SpecialProduct specialProduct = _specialProductRepository.FindFirstOrDefault(x => x.Id == specialProductId);
            _specialProductRepository.Delete(specialProduct);

        }

        public decimal GetSpecialProductValue(string productId)
        {
            return _specialProductRepository.FindFirstOrDefault(x => x.Id.ToString() == productId).Value;
        }
    }
}