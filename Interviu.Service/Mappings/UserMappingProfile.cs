using AutoMapper;
using Interviu.Core.DTOs;
using Interviu.Entity.Entities;

namespace Interviu.Service.Mappings
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<ApplicationUser, UserDto>();
            
            CreateMap<RegisterDto, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));
            
            CreateMap<ApplicationUser, UserWithCvDto>()
                .ForMember(dest => dest.Cvs, opt => opt.MapFrom(src => src.Cvs));
            
            CreateMap<CV, CvDto>();
        }
    }
}