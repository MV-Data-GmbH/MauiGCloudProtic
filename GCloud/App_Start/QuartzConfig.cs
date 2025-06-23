using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using FcmSharp;
using FcmSharp.Requests;
using FcmSharp.Settings;
using GCloud.Extensions;
using GCloud.Models.Domain;
using GCloud.Repository;
using GCloud.Repository.Impl;
using Quartz;
using Quartz.Impl;
using Notification = GCloud.Models.Domain.Notification;

namespace GCloud.App_Start
{
    public class QuartzConfig
    {
        private const string DefaultGroup = "DefaultGroup";
        private const string VormittagKey = "Vormittag";
        private const string MittagKey = "Mittag";
        private const string NachmittagKey = "Nachmittag";
        private const string AbendKey = "Abend";

        public static async void Initialize()
        {
            var factory = new StdSchedulerFactory();
            var scheduler = await factory.GetScheduler();

            await scheduler.Start();

            var job = JobBuilder.Create<CheckForPushNotificationJob>().WithIdentity("CheckNotifications", DefaultGroup).Build();

            var triggerVormittag = TriggerBuilder.Create()
                .WithIdentity(VormittagKey, DefaultGroup)
                .ForJob(job)
                .WithDailyTimeIntervalSchedule(x => x.WithIntervalInHours(24).StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(8, 0)))
                .Build();
            var triggerMittag = TriggerBuilder.Create()
                .WithIdentity(MittagKey, DefaultGroup)
                .ForJob(job)
                .WithDailyTimeIntervalSchedule(x => x.WithIntervalInHours(24).StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(10, 0)))
                .Build();
            var triggerNachmittag = TriggerBuilder.Create()
                .WithIdentity(NachmittagKey, DefaultGroup)
                .ForJob(job)
                .WithDailyTimeIntervalSchedule(x => x.WithIntervalInHours(24).StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(14, 0)))
                .Build();
            var triggerAbend = TriggerBuilder.Create()
                .WithIdentity(AbendKey, DefaultGroup)
                .ForJob(job)
                .WithDailyTimeIntervalSchedule(x => x.WithIntervalInHours(24).StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(18, 0)))
                .Build();

            var triggerTest = TriggerBuilder.Create()
                .WithIdentity("test", DefaultGroup)
                .ForJob(job)
                .StartNow()
                .Build();

            await scheduler.ScheduleJob(job, triggerVormittag);
            await scheduler.ScheduleJob(triggerMittag);
            await scheduler.ScheduleJob(triggerNachmittag);
            await scheduler.ScheduleJob(triggerAbend);
            //await scheduler.ScheduleJob(triggerTest);
        }

        public class CheckForPushNotificationJob : IJob
        {
            private static readonly Expression<Func<Notification, bool>> VormittagExpression = n => n.NotifyTime < new TimeSpan(10, 0, 0);
            private static readonly Expression<Func<Notification, bool>> MittagExpression = n => n.NotifyTime.IsBetween(new TimeSpan(10, 0, 0), new TimeSpan(14, 0, 0), true);
            private static readonly Expression<Func<Notification, bool>> NachmittagExpression = n => n.NotifyTime.IsBetween(new TimeSpan(14, 0, 0), new TimeSpan(18, 0, 0), true);
            private static readonly Expression<Func<Notification, bool>> AbendExpression = n => n.NotifyTime >= new TimeSpan(18, 0, 0);

            public async Task Execute(IJobExecutionContext context)
            {
                var random = new Random();

                await Task.Delay(TimeSpan.FromMinutes(random.Next(10, 90)));

                using (var dbContext = new GCloudContext())
                {
                    //Manually creating an instance, because the Autofac integration framework has an error so that the DI is not working...
                    var notificationRepository = new NotificationRepository(dbContext);

                    Expression<Func<Notification, bool>> expr = null;

                    switch (context.JobDetail.Key.Name)
                    {
                        case VormittagKey:
                            expr = VormittagExpression;
                            break;
                        case MittagKey:
                            expr = MittagExpression;
                            break;
                        case NachmittagKey:
                            expr = NachmittagExpression;
                            break;
                        case AbendKey:
                            expr = AbendExpression;
                            break;
                        default:
                            //For the testjob to trigger all notifications
                            expr = VormittagExpression;
                            break;
                    }

                    List<Notification> notifications;

                    try
                    {
                        var day = (int)DateTime.Now.DayOfWeek;
                        notifications = notificationRepository.FindBy(expr).Where(x => (int)x.DayOfWeek == day).Include(x => x.Store).ToList();
                    }
                    catch (Exception ex)
                    {
                        Debugger.Break();
                        return;
                    }

                    var fcmClientSettings = LoadFirebasePrivateKey();

                    using (var fcmClient = new FcmClient(fcmClientSettings))
                    {

                        foreach (var notification in notifications)
                            await fcmClient.SendAsync(new FcmMessage()
                            {
                                Message = new Message()
                                {
                                    Topic = notification.StoreId.ToString(),
                                    AndroidConfig = new AndroidConfig()
                                    {
                                        Data = new Dictionary<string, string>
                                        {
                                            { "title", "FoodJet Update!" },
                                            { "body", notification.Message },
                                            { "storeGuid", notification.StoreId.ToString() },
                                            { "storeName", notification.Store.Name },
                                            { "cashbackEnabled", (notification.Store?.Company?.IsCashbackEnabled ?? false).ToString() }
                                        }
                                    },
                                    ApnsConfig = new ApnsConfig()
                                    {
                                        Payload = new ApnsConfigPayload()
                                        {
                                            Aps = new Aps()
                                            {
                                                Alert = new ApsAlert()
                                                {
                                                    Title = "FoodJet Update!",
                                                    Body = $@"Nachricht von {notification.Store.Name}:\n{notification.Message}"
                                                },
                                                Sound = "default",
                                                Badge = 1,
                                                ThreadId = notification.Store.Name
                                            },
                                            
                                            CustomData = new Dictionary<string, object>
                                            {
                                                { "cashbackEnabled", (notification.Store?.Company?.IsCashbackEnabled ?? false).ToString() },
                                                { "storeGuid", notification.StoreId.ToString() },
                                            }
                                        }
                                    }
                                }
                            });
                    }
                }
            }

            private static FcmClientSettings LoadFirebasePrivateKey()
            {
                var path = Path.Combine(HttpRuntime.AppDomainAppPath, "firebasePrivateKey.json");

                return FileBasedFcmClientSettings.CreateFromFile(@"gcloudprotic", path);
            }
        }
    }
}