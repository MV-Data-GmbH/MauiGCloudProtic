using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using AutoMapper;
using GCloud.App_Start.AutomapperProfiles;
using GCloud.AutomapperProfiles;
using GCloud.Models.Domain;
using GCloud.Models;
using GCloud.Models.Domain.CouponUsageAction;
using GCloud.Models.Domain.CouponUsageRequirement;
using GCloud.Shared.Dto.Api;
using GCloud.Shared.Dto.Domain;
using GCloud.Shared.Dto.Domain.CouponUsageAction;
using GCloud.Shared.Dto.Domain.CouponUsageRequirement;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using CouponScope = GCloud.Models.Domain.CouponScope;
using MinimumTurnoverRequirement = GCloud.Models.Domain.CouponUsageRequirement.MinimumTurnoverRequirement;

namespace GCloud.App_Start
{
    public static class AutomapperConfig
    {
        public const string UserId = "userId";
        public const string IncludeImage = "includeImage";
        public const string IncludeBanner = "includeBanner";
        public const string IncludeCompanyLogo = "includeCompanyLogo";

        //Takes the coupon and extracts the CouponImage
        private static readonly Func<Coupon,bool, string> _loadCouponImage = (coupon, includeIcon) =>
        {
            if (!includeIcon) return null;
            var couponImage = coupon.CouponImages.OrderByDescending(x => x.CreationDateTime).FirstOrDefault();
            if (couponImage != null)
            {
                var filePath = Path.Combine(HttpContext.Current.Request.MapPath("~/UploadedFiles"), couponImage.FileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return null;
                }

                var bytes = File.ReadAllBytes(filePath);
                return Convert.ToBase64String(bytes);
            }
            return null;
        };

        private static readonly Func<Store, bool, string> _loadStoreImage = (store, includeBanner) =>
        {
            if (!includeBanner) return null;
            var filePath = Path.Combine(HttpContext.Current.Request.MapPath("~/UploadedFiles/Stores"), store.Id.ToString());
            if (!System.IO.File.Exists(filePath))
            {
                return null;
            }

            var bytes = File.ReadAllBytes(filePath);
            return Convert.ToBase64String(bytes);
        };

        private static readonly Func<Company, bool, string> _loadCompanyLogo = (company, includeLogo) =>
        {
            if (!includeLogo) return null;
            var filePath = Path.Combine(HttpContext.Current.Request.MapPath("~/UploadedFiles/CompanyLogos"), company.Id.ToString());
            if (!System.IO.File.Exists(filePath))
            {
                return null;
            }

            var bytes = File.ReadAllBytes(filePath);
            return Convert.ToBase64String(bytes);
        };

        private static TTarget GetValueFromResolutionContext<TTarget>(ResolutionContext context, string key, TTarget defaultValue)
        {
            try
            {
                if (key != null && context.Items.ContainsKey(key))
                {
                    return (TTarget) context.Items[key];
                }

                if (defaultValue != null)
                {
                    return defaultValue;
                }
            }
            catch (Exception)
            {
                if (defaultValue != null)
                {
                    return defaultValue;
                }
            }

            return default;
        }

        public static void InitAutomapper()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Coupon, CouponDto>()
                    .ForMember(dst => dst.RedeemsLeft,
                        opt => opt.ResolveUsing((coupon, dto, arg3, arg4) =>
                            dto.RedeemsLeft = coupon.MaxRedeems.HasValue ? coupon.MaxRedeems - coupon.Redeems.Count(x => x.UserId == GetValueFromResolutionContext(arg4, UserId, string.Empty))
                                : null))
                    .ForMember(dst => dst.ValidFrom, opt => opt.ResolveUsing((coupon, dto, arg3, arg4) => dto.ValidFrom = coupon.GetValidFrom(GetValueFromResolutionContext(arg4, UserId, string.Empty))))
                    .ForMember(dst => dst.ValidTo, opt => opt.ResolveUsing((coupon, dto, arg3, arg4) => dto.ValidTo = coupon.GetValidTo(GetValueFromResolutionContext(arg4, UserId, string.Empty))))
                    .ForMember(dst => dst.IsValid, opt => opt.ResolveUsing((coupon, dto, arg3, arg4) => dto.IsValid = coupon.IsCurrentlyValidForUser(GetValueFromResolutionContext(arg4, UserId, string.Empty))))
                    .ForMember(dst => dst.CouponScope, opt => opt.MapFrom(src => !src.UsageActions.Any() || src.UsageActions.OfType<InvoiceDiscountUsageAction>().Any() ? CouponScope.Invoice : CouponScope.Article))
                    .ForMember(dst => dst.ArticleNumber, opt => opt.MapFrom(src => src.UsageActions.OfType<ArticleDiscountUsageAction>().Any() ? src.UsageActions.OfType<ArticleDiscountUsageAction>().First().TargetArticle : new int?()))
                    .ForMember(dst => dst.IconBase64, opt => opt.ResolveUsing((coupon, dto, arg3, arg4) => dto.IconBase64 = _loadCouponImage(coupon, GetValueFromResolutionContext(arg4, IncludeImage, false))))
                    .ForMember(dst => dst.AssignedStores, opt => opt.MapFrom(src => src.AssignedStores.Select(a => a.Id).ToList()));

                cfg.CreateMap<Store, StoreDto>()
                    .ForMember(dst => dst.IsUserFollowing, opt => opt.ResolveUsing((store, dto, arg3, arg4) => dto.IsUserFollowing = store.InterestedUsers.Any(user => user.Id == GetValueFromResolutionContext(arg4, UserId, string.Empty))))
                    .ForMember(dst => dst.BannerImage, opt => opt.ResolveUsing((store, dto, arg3, arg4) => dto.BannerImage = _loadStoreImage(store, GetValueFromResolutionContext(arg4, IncludeBanner, false))));
                cfg.CreateMap<Company, CompanyDto>()
                    .ForMember(dst => dst.CompanyLogoBase64, opt => opt.ResolveUsing((company, dto, arg3, arg4) => dto.CompanyLogoBase64 = _loadCompanyLogo(company, GetValueFromResolutionContext(arg4, IncludeCompanyLogo, false))));
                cfg.CreateMap<Country, CountryDto>();
                cfg.CreateMap<Cashback, CashbackDto>();

                cfg.CreateMap<User, UserDto>().ForMember(d => d.LastCashback, opt => opt.MapFrom(src => src.TurnoverJournals.OfType<Cashback>().OrderByDescending(x => x.CreditDateTime).FirstOrDefault()));
                cfg.CreateMap<Store, AvailableStoresResponse>()
                    .ForMember(dst => dst.StoreId, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dst => dst.StoreName, opt => opt.MapFrom(src => src.Name));

                cfg.CreateMap<AbstractUsageAction, AbstractUsageActionDto>()
                    .Include<ArticleDiscountUsageAction, ArticleDiscountUsageActionDto>()
                    .Include<InvoiceDiscountUsageAction, InvoiceDiscountUsageActionDto>()
                    .Include<OrderArticleUsageAction, OrderArticleUsageActionDto>();
                cfg.CreateMap<ArticleDiscountUsageAction, ArticleDiscountUsageActionDto>();
                cfg.CreateMap<InvoiceDiscountUsageAction, InvoiceDiscountUsageActionDto>();
                cfg.CreateMap<OrderArticleUsageAction, OrderArticleUsageActionDto>();
                cfg.CreateMap<OrderArticleUsageActionItem, OrderArticleUsageActionItemDto>();

                cfg.CreateMap<AbstractUsageRequirement, AbstractUsageRequirementDto>()
                    .Include<ProductRequiredUsageRequirement, ProductRequiredUsageRequirementDto>()
                    .Include<MinimumTurnoverRequirement, MinimumTurnoverRequirementDto>();
                cfg.CreateMap<ProductRequiredUsageRequirement, ProductRequiredUsageRequirementDto>();
                cfg.CreateMap<MinimumTurnoverRequirement, MinimumTurnoverRequirementDto>();
                cfg.CreateMap<Bill, Bill_Out_Dto>();
                cfg.CreateMap<SpecialProduct, SpecialProductsDto>();
                cfg.CreateMap<User, UserPointsDto>();

                cfg.AddProfile<EnumProfile>();
                //ViewModelProfiles
                cfg.AddProfile<StoreViewModelProfile>();                
                cfg.AddProfile<CouponViewModelProfile>();
                cfg.AddProfile<UserViewModelProfile>();
                cfg.AddProfile<CompanyViewModelProfile>();
                cfg.AddProfile<SpecialProductViewModelProfile>();

                
            });
        }
    }
 }