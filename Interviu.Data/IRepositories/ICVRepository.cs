using Interviu.Entity.Entities;

namespace Interviu.Data.IRepositories;

public interface ICVRepository:IGenericRepository<CV>
{
     Task <IEnumerable<CV>>GetByCvsByUserIdAsync(string userId);
     Task<CV?> GetCvWithUserAsync(Guid cvId);
     Task<IEnumerable<CV>> GetAllCvsSummaryAsync();
     
}