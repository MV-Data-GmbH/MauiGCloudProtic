using GCloud.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCloud.Service
{
    public interface ISpecialProductService : IAbstractService<SpecialProduct>
    {
        IQueryable<SpecialProduct> FindByUserId(string userId);

        void DeleteSpecialProduct(Guid storeId);

        decimal GetSpecialProductValue(string productId);
    }
}
