using Interviu.Core.DTOs;
using Interviu.Entity.Entities;

namespace Interviu.Service.IServices;

public interface IQuestionService
{
    Task<QuestionDto> GetQuestionByIdAsync(Guid id);
    Task<IEnumerable<QuestionDto>>GetAllQuestionsAsync();
    Task<QuestionDto> CreateQuestionAsync(CreateQuestionDto dto);
    Task UpdateQuestionAsync(Guid id, UpdateQuestionDto dto);
    Task DeleteQuestionAsync(Guid id);
    Task<IEnumerable<QuestionDto>>GetRandomQuestionsAsync(int count, string? category=null);
    Task<IEnumerable<string>>GetCategoriesAsync();
}