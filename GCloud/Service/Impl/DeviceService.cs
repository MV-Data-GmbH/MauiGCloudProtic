using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;
using GCloud.Repository;
using GCloud.Service.Impl;

namespace GCloud.Service.Impl
{
    public class DeviceService : AbstractService<Device>, IDeviceService
    {
        private IDeviceRepository _deviceRepository;
        public DeviceService(IAbstractRepository<Device> repository) : base(repository)
        {
            _deviceRepository = (IDeviceRepository) repository;
        }
    }
}