using BusinessLogic.Models;
using System;
using WebSite.ViewModels.Mapping;

namespace WebSite.ViewModels.Admin
{
    public class AdminMappingProfile : AutoMapper.Profile
    {
        public AdminMappingProfile()
        {
            CreateMap<Company, CompanyFormViewModel>();

            CreateMap<Company, CompanyDataTableFields>();

            CreateMap<User, UserFormViewModel>()
                .ForMember(d => d.CompanyId, o => o.MapFrom<CompanyIdResolver, Company?>(s => s.Company))
                .ForMember(d => d.Companies, o => o.Ignore())
                .ForMember(d => d.Password, o => o.Ignore());

            CreateMap<User, UserDataTableFields>()
                .ForMember(d => d.CompanyName, o => o.MapFrom<CompanyNameResolver, Company?>(s => s.Company))
                .ForMember(d => d.CreateDate, o => o.MapFrom<DateToFormattedStringResolver, DateTime?>(s => s.CreateDate))
                .ForMember(d => d.Role, o => o.MapFrom<UserRoleListToStringResolver>());
        }
    }
}