using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using GCloud.Controllers.ViewModels.User;
using GCloud.Models.Domain;
using GCloud.Shared.Dto.Domain;

namespace GCloud.App_Start.AutomapperProfiles
{
    public class UserViewModelProfile : Profile
    {
        public UserViewModelProfile()
        {
            CreateMap<User, UserIndexViewModel>()
                .ForMember(dst => dst.Enabled, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dst => dst.CreatedByUsername, opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.UserName : ""))
                .ForMember(dst => dst.RoleName, opt => opt.Ignore());
            CreateMap<UserCreateViewModel, User>()
                .ForMember(dst => dst.PasswordHash, opt => opt.Ignore())
                .ForMember(dst => dst.IsActive, opt => opt.MapFrom(src => src.Enabled));
            CreateMap<User, UserEditViewModel>()
                .ForMember(dst => dst.Enabled, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dst => dst.RoleId, opt => opt.MapFrom(src => src.Roles.Any() ? src.Roles.FirstOrDefault().RoleId : null));
            CreateMap<UserEditViewModel, User>()
                .ForMember(dst => dst.IsActive, opt => opt.MapFrom(src => src.Enabled))
                .ForMember(dst => dst.CreatedById, opt => opt.MapFrom(src => src.CreatedById))
                ;
        }
    }
}