using GCloud.Models.Domain;
using System.Data.Entity;

namespace GCloud.Repository.Impl
{
    public class FirebaseNotificationRepository : AbstractRepository<FirebaseNotification>, IFirebaseNotificationRepository
    {
        public FirebaseNotificationRepository(DbContext context) : base(context)
        {
        }
    }
}