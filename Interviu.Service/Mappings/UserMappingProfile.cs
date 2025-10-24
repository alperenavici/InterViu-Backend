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
                        
            // Question mappings
            CreateMap<Question, QuestionDto>();
            CreateMap<CreateQuestionDto, Question>();
            
            // Interview mappings
            CreateMap<Interview, InterviewDto>()
                .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.InterviewQuestions))
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.Cv, opt => opt.MapFrom(src => src.Cv));
            CreateMap<InterviewQuestion, InterviewQuestionDto>();
        }
    }
}