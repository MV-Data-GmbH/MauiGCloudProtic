using AutoMapper;
using GCloud.Models.Domain;
using GCloud.Models.Domain.CouponUsageAction;
using GCloud.Shared.Dto.Domain;

namespace GCloud.AutomapperProfiles
{
    public class EnumProfile : Profile
    {
        public EnumProfile()
        {
            CreateMap<CouponType, CouponTypeDto>().ProjectUsing(s => (CouponTypeDto)s);
            CreateMap<CouponTypeDto,CouponType> ().ProjectUsing(s => (CouponType)s);
            CreateMap<CouponScope, CouponScopeDto>().ProjectUsing(s => (CouponScopeDto)s);
            CreateMap<CouponScopeDto, CouponScope>().ProjectUsing(s => (CouponScope)s);
            CreateMap<Coupling, Shared.Dto.Domain.CouponUsageAction.Coupling>().ProjectUsing(s => (Shared.Dto.Domain.CouponUsageAction.Coupling)s);
            CreateMap<Shared.Dto.Domain.CouponUsageAction.Coupling, Coupling>().ProjectUsing(s => (Coupling)s);
        }
    }
}