using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;
using GCloud.Repository;
using GCloud.Service.Impl;

namespace GCloud.Service.Impl
{
    public class CouponImageService : AbstractService<CouponImage>, ICouponImageService
    {
        private readonly ICouponImageRepository _couponImageRepository;

        public CouponImageService(IAbstractRepository<CouponImage> repository) : base(repository)
        {
            _couponImageRepository = repository as ICouponImageRepository;
        }

        public IQueryable<CouponImage> FindByCouponId(Guid id)
        {
            return _couponImageRepository.FindBy(x => x.CouponId == id);
        }
    }
}