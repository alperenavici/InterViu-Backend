using Interviu.Data.IRepositories;
using Interviu.Entity.Entities;
using Microsoft.EntityFrameworkCore;
using Interviu.Data.Context;
using Interviu.Entity.Enums;

namespace Interviu.Data.Repositories;

public class QuestionRepository : GenericRepository<Question>, IQuestionRepository
{
    public QuestionRepository(ApplicationDbContext dbcontext) : base(dbcontext)
    {
    }

    public async Task<IEnumerable<Question>> GetAllQuestionByCategoryAsync(string category)
    {
        return await _dbcontext.Questions
            .Where(qu=>qu.Category == category)
            .ToListAsync();
    }

    public async Task<IEnumerable<Question>> GetAllQuestionByDifficultyAsync(Difficulty difficulty)
    {
        return await _dbcontext.Questions
            .Where(qu => qu.Difficulty == difficulty)
            .ToListAsync();
    }

    public async Task<List<Question>> GetRandomQuestionsAsync(int count, string? category = null)
    {
        IQueryable<Question> query = _dbcontext.Questions;
        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(q => q.Category == category);
        }
        
        var randomQuestions = await query
            .OrderBy(q => Guid.NewGuid()) 
            .Take(count)                  
            .ToListAsync();               
        
        return randomQuestions;
    }
}

    
