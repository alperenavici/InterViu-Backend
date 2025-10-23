using Interviu.Data.Context;
using Interviu.Data.IRepositories;
using Interviu.Entity.Entities;
using Interviu.Entity.Enums;
using Microsoft.EntityFrameworkCore;

namespace Interviu.Data.Repositories;

public class InterviewRepository : GenericRepository<Interview>, IInterviewRepository
{
    public InterviewRepository(ApplicationDbContext dbcontext) : base(dbcontext)
    {
    }


    public async Task<Interview?> GetInterviewWithDetailAsync(Guid id)
    {
        return await _dbcontext.Interviews
            .Include(i=>i.User)
            .Include(i=>i.Cv)
            
            .Include(i => i.InterviewQuestions)           
                .ThenInclude(iq => iq.Question)
            
            .FirstOrDefaultAsync(i => i.Id == id );
            
    }

    public async Task<IEnumerable<Interview>> GetInterviewsByUserIdAsync(string userId)
    {
        return await _dbcontext.Interviews
            .Where(i=>i.UserId == userId)
            .OrderByDescending(i=>i.StartedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Interview>> GetInterviewsByStatusAsync(InterviewStatus status)
    {
        return await _dbcontext.Interviews
            .Where(i=>i.Status == status)
            .OrderByDescending(i=>i.StartedAt)
            .ToListAsync();
            
    }

    public async Task<IEnumerable<Interview>> GetInterviewsByCvAsync(Guid cvId)
    {
        return await _dbcontext.Interviews
            .Where(i=>i.CvId==cvId)
            .ToListAsync();
    }
    
}
