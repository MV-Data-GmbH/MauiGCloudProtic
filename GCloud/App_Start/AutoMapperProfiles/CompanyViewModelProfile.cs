using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using GCloud.Controllers.ViewModels.Company;
using GCloud.Models.Domain;

namespace GCloud.App_Start.AutomapperProfiles
{
    public class CompanyViewModelProfile : Profile
    {
        public CompanyViewModelProfile()
        {
            CreateMap<CompanyCreateViewModel, Company>()
                .ForMember(dst => dst.UserId, opt => opt.MapFrom(src => src.AssociatedUserId));
            CreateMap<CompanyEditViewModel, Company>()
                .ForMember(dst => dst.UserId, opt => opt.MapFrom(src => src.AssignedUserId));
            CreateMap<Company, CompanyEditViewModel>()
                .ForMember(dst => dst.AssignedUserId, opt => opt.MapFrom(src => src.UserId));
        }
    }
}