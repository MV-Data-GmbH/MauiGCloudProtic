using FcmSharp;
using FcmSharp.Requests;
using FcmSharp.Settings;
using GCloud.Models.Domain;
using GCloud.Repository;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GCloud.Shared.Exceptions.Anonymous;

namespace GCloud.Service.Impl
{
    public class FirebaseNotificationService : AbstractService<FirebaseNotification>, IFirebaseNotificationService
    {
        private readonly IAnonymousMobilePhoneRepository _anonymousMobilePhoneRepository;
        private readonly IMobilePhoneRepository _mobilePhoneRepository;
        private readonly FcmClientSettings _fcmClientSettings;

        public FirebaseNotificationService(IFirebaseNotificationRepository repository,
             FcmClientSettings fcmClientSettings,
            IMobilePhoneRepository mobilePhoneRepository,
            IAnonymousMobilePhoneRepository anonymousMobilePhoneRepository) : base(repository)
        {
            _anonymousMobilePhoneRepository = anonymousMobilePhoneRepository;
            _mobilePhoneRepository = mobilePhoneRepository;
            _fcmClientSettings = fcmClientSettings;
        }

        public async Task<ActResult> Send(FirebaseNotification notification)
        {
            try
            {
                var val = ValidateNotification(notification);
                //if (val.Success == false)
                //    return val;

                var sent = await NotifyAsync(notification);
                notification.LastAttemptOn = DateTime.Now;
                notification.Sent = sent;
                Update(notification);

                return new ActResult
                {
                    Success = sent,
                };
            }
            catch (Exception ex)
            {
                return new ActResult
                {
                    Message = ex.Message
                };
            }
        }

        private async Task<bool> NotifyAsync(FirebaseNotification notification)
        {
            try
            {
                var firebaseIstanceId = notification?.Device?.FirebaseInstanceId ?? notification?.AnonymousMobilePhone?.FirebaseInstanceId;

                if (string.IsNullOrWhiteSpace(firebaseIstanceId))
                {
                    throw new InvalidFirebaseTokenException(firebaseIstanceId);
                }

                using (var client = new FcmClient(_fcmClientSettings))
                {
                    var data = new Dictionary<string, string>()
                    {
                        {"title", notification.Title},
                        {"body", notification.Body},
                         {"type", notification.Type},
                        {"billId",notification.BillId.ToString() }
                    };

                    var message = new FcmMessage()
                    {
                        ValidateOnly = false,
                        Message = new Message
                        {
                            Token = firebaseIstanceId,
                            Data = data
                        }
                    };

                    var cts = new CancellationTokenSource();
                    var result = await client.SendAsync(message, cts.Token);
                    // success
                    return result != null;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private ActResult ValidateNotification(FirebaseNotification notification)
        {
            if (notification == null)
                return new ActResult { Message = "Notification is not defined" };

            if (notification.IsDeleted)
                return new ActResult { Message = "Notification is deleted" };

            if (notification.Sent)
                return new ActResult { Message = "Notification is sent already" };

            if (notification.Device == null)
                notification.Device = _mobilePhoneRepository.FindById(notification.DeviceId);

            if (notification.AnonymousMobilePhone == null)
            {
                notification.AnonymousMobilePhone =_anonymousMobilePhoneRepository.FindById(notification.AnonymousMobilePhoneId);
            }

            if (notification.AnonymousMobilePhone == null && notification.Device == null || string.IsNullOrWhiteSpace(notification.Device?.FirebaseInstanceId) && string.IsNullOrWhiteSpace(notification.AnonymousMobilePhone?.FirebaseInstanceId))
                return new ActResult { Message = "Target Device not defined" };

            return new ActResult { Success = true };
        }
    }
}