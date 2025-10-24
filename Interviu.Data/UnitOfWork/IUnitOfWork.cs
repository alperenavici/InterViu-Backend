using Interviu.Data.IRepositories;

namespace Interviu.Data.UnitOfWork;

public interface IUnitOfWork: IAsyncDisposable
{
    IUserRepository Users { get; }
    IInterviewRepository Interviews { get; }
    IQuestionRepository Questions { get; }
    ICVRepository CVs { get; }
    IGenericRepository<T> Repository<T>() where T : class;
    
    Task<int>SaveChangesAsync();
    
    
}