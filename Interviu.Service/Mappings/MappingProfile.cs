using AutoMapper;
using Interviu.Core.DTOs;
using Interviu.Entity.Entities;

namespace Interviu.Service.Mappings;


public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // --- TEMEL MAPPING'LER (İÇ İÇE NESNELER İÇİN GEREKLİ) ---

            // Kural 1: ApplicationUser'ı UserDto'ya nasıl çevireceğini öğret.
            CreateMap<ApplicationUser, UserDto>();

            // Kural 2: CV'yi CvDto'ya nasıl çevireceğini öğret.
            CreateMap<CV, CvDto>();
            
            // Kural 3: Question'ı QuestionDto'ya nasıl çevireceğini öğret.
            // Bu, InterviewQuestionDto'nun içindeki QuestionText'i doldurmak için gereklidir.
            CreateMap<Question, QuestionDto>();


            // --- ARA TABLO MAPPING'İ (ÖZEL KURALLARLA) ---

            // Kural 4: InterviewQuestion'ı InterviewQuestionDto'ya nasıl çevireceğini öğret.
            CreateMap<InterviewQuestion, InterviewQuestionDto>();


            // --- ANA MAPPING KURALI (TÜM PARÇALARI BİRLEŞTİRİR) ---

            // Kural 5: Interview'ü InterviewDto'ya nasıl çevireceğini öğret.
            // Yukarıdaki tüm kurallar tanımlandığı için, bu kural artık sorunsuz çalışacaktır.
            CreateMap<Interview, InterviewDto>()
                // Hedefteki 'Questions' listesini, kaynaktaki 'InterviewQuestions' listesinden al.
                // (Property isimleri farklı olduğu için bunu belirtmek gerekir)
                .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.InterviewQuestions))
                // Hedefteki 'User' alanını, kaynaktaki 'User' nesnesinden al ve UserDto'ya çevir.
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                // Hedefteki 'Cv' alanını, kaynaktaki 'Cv' nesnesinden al ve CvDto'ya çevir.
                .ForMember(dest => dest.Cv, opt => opt.MapFrom(src => src.Cv));
        }
    }