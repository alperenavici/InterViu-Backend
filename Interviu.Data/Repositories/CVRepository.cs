using Interviu.Data.Context;
using Interviu.Data.IRepositories;
using Interviu.Entity.Entities;
using Microsoft.EntityFrameworkCore;

namespace Interviu.Data.Repositories;

public class CVRepository : GenericRepository<CV>, ICVRepository
{
    public CVRepository(ApplicationDbContext dbcontext):base(dbcontext)
    {
    }

    public async Task<IEnumerable<CV>> GetByCvsByUserIdAsync(string userId)
    {
        return await _dbcontext.CVs
            .Where(cv => cv.UserId == userId)
            .ToListAsync();
    }

    public async Task<CV?> GetCvWithUserAsync(Guid cvId)
    {
        return await _dbcontext.CVs
            .Include(cv=>cv.User)
            .FirstOrDefaultAsync(cv=>cv.Id==cvId);
    }

    public async Task<IEnumerable<CV>> GetAllCvsSummaryAsync()
    {
        return await _dbcontext.CVs
            .Select(cv => new CV
            {
                Id = cv.Id,
                FileName = cv.FileName,
                FilePath = cv.FilePath,
                UploadDate = cv.UploadDate,
                UserId = cv.UserId
            })
            .ToListAsync();
        
    }
    
}