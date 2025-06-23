using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using AutoMapper;
using GCloud.Controllers.ViewModels.Coupon;
using GCloud.Controllers.ViewModels.Store;
using GCloud.Extensions;
using GCloud.Models;
using GCloud.Models.Domain;

namespace GCloud.App_Start.AutomapperProfiles
{
    public class StoreViewModelProfile : Profile
    {
        public StoreViewModelProfile()
        {
            CreateMap<StoreCreateViewModel, Store>()
                .ForMember(dst => dst.EBillCategory, opt => opt.Ignore());
            CreateMap<StoreEditViewModel, Store>()
                .ForMember(dst => dst.EBillCategory, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dst => dst.EBillCategory, opt => opt.MapFrom(src => src.EBillCategory.Name));
            CreateMap<Store, NotificationEditViewModel>()
                .ForMember(dst => dst.StoreId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.Message, opt => opt.MapFrom(src => src.Notifications.Any() ? src.Notifications.First().Message : ""))
                .ForMember(dst => dst.DaySelection, opt => opt.MapFrom(src =>
                    new List<NotificationDaySelectionViewModel>() {
                        new NotificationDaySelectionViewModel
                        {
                            DayOfWeek = DayOfWeek.Monday,
                            Vormittag = src.Notifications != null && src.Notifications.Any(x => x.DayOfWeek == DayOfWeek.Monday && x.NotifyTime < new TimeSpan(10,0,0)),
                            Mittag = src.Notifications != null && src.Notifications.Any(x => x.DayOfWeek == DayOfWeek.Monday && x.NotifyTime.IsBetween(new TimeSpan(10,0,0), new TimeSpan(14,0,0), true)) ,
                            Nachmittag = src.Notifications != null && src.Notifications.Any(x => x.DayOfWeek == DayOfWeek.Monday && x.NotifyTime.IsBetween(new TimeSpan(14,0,0), new TimeSpan(18,0,0), true)) ,
                            Abend = src.Notifications != null && src.Notifications.Any(x => x.DayOfWeek == DayOfWeek.Monday && x.NotifyTime >= new TimeSpan(18,0,0))
                        },
                        new NotificationDaySelectionViewModel
                        {
                            DayOfWeek = DayOfWeek.Tuesday,
                            Vormittag = src.Notifications != null && src.Notifications.Any(x => x.DayOfWeek == DayOfWeek.Tuesday && x.NotifyTime < new TimeSpan(10,0,0)),
                            Mittag = src.Notifications != null && src.Notifications.Any(x => x.DayOfWeek == DayOfWeek.Tuesday && x.NotifyTime.IsBetween(new TimeSpan(10,0,0), new TimeSpan(14,0,0), true)) ,
                            Nachmittag = src.Notifications != null && src.Notifications.Any(x => x.DayOfWeek == DayOfWeek.Tuesday && x.NotifyTime.IsBetween(new TimeSpan(14,0,0), new TimeSpan(18,0,0), true)) ,
                            Abend = src.Notifications != null && src.Notifications.Any(x => x.DayOfWeek == DayOfWeek.Tuesday && x.NotifyTime >= new TimeSpan(18,0,0))
                        },
                        new NotificationDaySelectionViewModel
                        {
                            DayOfWeek = DayOfWeek.Wednesday,
                            Vormittag = src.Notifications != null && src.Notifications.Any(x => x.DayOfWeek == DayOfWeek.Wednesday && x.NotifyTime < new TimeSpan(10,0,0)),
                            Mittag = src.Notifications != null && src.Notifications.Any(x => x.DayOfWeek == DayOfWeek.Wednesday && x.NotifyTime.IsBetween(new TimeSpan(10,0,0), new TimeSpan(14,0,0), true)) ,
                            Nachmittag = src.Notifications != null && src.Notifications.Any(x => x.DayOfWeek == DayOfWeek.Wednesday && x.NotifyTime.IsBetween(new TimeSpan(14,0,0), new TimeSpan(18,0,0), true)) ,
                            Abend = src.Notifications != null && src.Notifications.Any(x => x.DayOfWeek == DayOfWeek.Wednesday && x.NotifyTime >= new TimeSpan(18,0,0))
                        },
                        new NotificationDaySelectionViewModel
                        {
                            DayOfWeek = DayOfWeek.Thursday,
                            Vormittag = src.Notifications != null && src.Notifications.Any(x => x.DayOfWeek == DayOfWeek.Thursday && x.NotifyTime < new TimeSpan(10,0,0)),
                            Mittag = src.Notifications != null && src.Notifications.Any(x => x.DayOfWeek == DayOfWeek.Thursday && x.NotifyTime.IsBetween(new TimeSpan(10,0,0), new TimeSpan(14,0,0), true)) ,
                            Nachmittag = src.Notifications != null && src.Notifications.Any(x => x.DayOfWeek == DayOfWeek.Thursday && x.NotifyTime.IsBetween(new TimeSpan(14,0,0), new TimeSpan(18,0,0), true)) ,
                            Abend = src.Notifications != null && src.Notifications.Any(x => x.DayOfWeek == DayOfWeek.Thursday && x.NotifyTime >= new TimeSpan(18,0,0))
                        },
                        new NotificationDaySelectionViewModel
                        {
                            DayOfWeek = DayOfWeek.Friday,
                            Vormittag = src.Notifications != null && src.Notifications.Any(x => x.DayOfWeek == DayOfWeek.Friday && x.NotifyTime < new TimeSpan(10,0,0)),
                            Mittag = src.Notifications != null && src.Notifications.Any(x => x.DayOfWeek == DayOfWeek.Friday && x.NotifyTime.IsBetween(new TimeSpan(10,0,0), new TimeSpan(14,0,0), true)) ,
                            Nachmittag = src.Notifications != null && src.Notifications.Any(x => x.DayOfWeek == DayOfWeek.Friday && x.NotifyTime.IsBetween(new TimeSpan(14,0,0), new TimeSpan(18,0,0), true)) ,
                            Abend = src.Notifications != null && src.Notifications.Any(x => x.DayOfWeek == DayOfWeek.Friday && x.NotifyTime >= new TimeSpan(18,0,0))
                        },
                        new NotificationDaySelectionViewModel
                        {
                            DayOfWeek = DayOfWeek.Saturday,
                            Vormittag = src.Notifications != null && src.Notifications.Any(x => x.DayOfWeek == DayOfWeek.Saturday && x.NotifyTime < new TimeSpan(10,0,0)),
                            Mittag = src.Notifications != null && src.Notifications.Any(x => x.DayOfWeek == DayOfWeek.Saturday && x.NotifyTime.IsBetween(new TimeSpan(10,0,0), new TimeSpan(14,0,0), true)) ,
                            Nachmittag = src.Notifications != null && src.Notifications.Any(x => x.DayOfWeek == DayOfWeek.Saturday && x.NotifyTime.IsBetween(new TimeSpan(14,0,0), new TimeSpan(18,0,0), true)) ,
                            Abend = src.Notifications != null && src.Notifications.Any(x => x.DayOfWeek == DayOfWeek.Saturday && x.NotifyTime >= new TimeSpan(18,0,0))
                        },
                        new NotificationDaySelectionViewModel
                        {
                            DayOfWeek = DayOfWeek.Sunday,
                            Vormittag = src.Notifications != null && src.Notifications.Any(x => x.DayOfWeek == DayOfWeek.Sunday && x.NotifyTime < new TimeSpan(10,0,0)),
                            Mittag = src.Notifications != null && src.Notifications.Any(x => x.DayOfWeek == DayOfWeek.Sunday && x.NotifyTime.IsBetween(new TimeSpan(10,0,0), new TimeSpan(14,0,0), true)) ,
                            Nachmittag = src.Notifications != null && src.Notifications.Any(x => x.DayOfWeek == DayOfWeek.Sunday && x.NotifyTime.IsBetween(new TimeSpan(14,0,0), new TimeSpan(18,0,0), true)) ,
                            Abend = src.Notifications != null && src.Notifications.Any(x => x.DayOfWeek == DayOfWeek.Sunday && x.NotifyTime >= new TimeSpan(18,0,0))
                        }
                    }
                ));
        }
    }
}