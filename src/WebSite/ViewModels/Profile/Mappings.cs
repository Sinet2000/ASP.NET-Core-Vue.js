using BusinessLogic.Models;

namespace WebSite.ViewModels.Profile
{
    public class ProfileMappingProfile : AutoMapper.Profile
    {
        public ProfileMappingProfile()
        {
            CreateMap<User, ChangeBaseInfoViewModel>();
        }
    }
}