using GCloud.Models.Domain;
using System.Threading.Tasks;

namespace GCloud.Service
{
    public interface IFirebaseNotificationService : IAbstractService<FirebaseNotification>
    {
        Task<ActResult> Send(FirebaseNotification notification);
    }
}