using AutoMapper;
using GCloud.Controllers.ViewModels.SpecialProduct;
using GCloud.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GCloud.App_Start.AutomapperProfiles
{
    public class SpecialProductViewModelProfile : Profile
    {
        public SpecialProductViewModelProfile()
        {
            CreateMap<SpecialProductCreateViewModel, SpecialProduct>()
                .ForMember(dst => dst.AssignedStores, opt => opt.MapFrom(src => src.AssignedStores.Where(x => x.IsChecked).Select(x => new Store { Id = x.Id }).ToList()));
            ;
        }
    }
}