using Interviu.Entity.Entities;
using Interviu.Entity.Enums;

namespace Interviu.Data.IRepositories;

public interface IInterviewRepository:IGenericRepository<Interview>
{
    Task<Interview?> GetInterviewWithDetailAsync(Guid id);
    Task<IEnumerable<Interview>>GetInterviewsByUserIdAsync(string userId);
    Task<IEnumerable<Interview>> GetInterviewsByStatusAsync(InterviewStatus status);
    Task<IEnumerable<Interview>> GetInterviewsByCvAsync(Guid cvId);

}