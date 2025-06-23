using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using GCloud.Controllers.ViewModels.Coupon;
using GCloud.Models.Domain;
using GCloud.Models.Domain.CouponUsageAction;

namespace GCloud.App_Start.AutomapperProfiles
{
    public class CouponViewModelProfile : Profile
    {
        public CouponViewModelProfile()
        {
            CreateMap<CouponCreateViewModel, Coupon>()
                .ForMember(dst => dst.AssignedStores, opt => opt.MapFrom(src => src.AssignedStores.Where(x => x.IsChecked).Select(x => new Store { Id = x.Id }).ToList()))
                .ForMember(dst => dst.UsageActions, opt => opt.MapFrom(src => src.CouponScope == CouponScope.Invoice ? new List<AbstractUsageAction> {new InvoiceDiscountUsageAction{Discount = src.Value, DiscountType = src.CouponType, SortOrder = 1}} : new List<AbstractUsageAction> { new ArticleDiscountUsageAction { TargetArticle = src.ArticleNumber.GetValueOrDefault(-1), DiscountType = src.CouponType, Discount = src.Value, SortOrder = 1 } }));
            CreateMap<Coupon, CouponEditViewModel>()
                .ForMember(dst => dst.AssignedStores, opt => opt.Ignore())
                .ForMember(dst => dst.CouponScope, opt => opt.MapFrom(src => src.UsageActions.OfType<InvoiceDiscountUsageAction>().Any() ? CouponScope.Invoice : CouponScope.Article))
                .ForMember(dst => dst.ArticleNumber, opt => opt.MapFrom(src => src.UsageActions.OfType<ArticleDiscountUsageAction>().Any() ? src.UsageActions.OfType<ArticleDiscountUsageAction>().First().TargetArticle : new int?()));
            CreateMap<CouponEditViewModel, Coupon>()
                .ForMember(dst => dst.AssignedStores, opt => opt.MapFrom(src => src.AssignedStores.Where(x => x.IsChecked).Select(x => new Store { Id = x.Id }).ToList()))
                .ForMember(dst => dst.UsageActions, opt => opt.MapFrom(src => src.CouponScope == CouponScope.Invoice ? new List<AbstractUsageAction> { new InvoiceDiscountUsageAction {Discount = src.Value, DiscountType = src.CouponType, SortOrder = 1 } } : new List<AbstractUsageAction> { new ArticleDiscountUsageAction { TargetArticle = src.ArticleNumber.GetValueOrDefault(-1), DiscountType = src.CouponType, Discount = src.Value, SortOrder = 1 } }));
            ;
        }
    }
}