using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;

namespace GCloud.Repository.Impl
{
    public class NotificationRepository : AbstractRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(DbContext context) : base(context)
        {
        }
    }
}