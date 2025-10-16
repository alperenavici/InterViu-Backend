using Interviu.Entity.Entities;
using Interviu.Entity.Enums;
using Microsoft.EntityFrameworkCore;

namespace Interviu.Data.IRepositories;

public interface IQuestionRepository:IGenericRepository<Question>
{
    Task<IEnumerable<Question>> GetAllQuestionByCategoryAsync(string category);
    Task<IEnumerable<Question>>GetAllQuestionByDifficultyAsync(Difficulty difficulty);
    Task<List<Question>>GetRandomQuestionsAsync(int count,string? category=null);
    
}