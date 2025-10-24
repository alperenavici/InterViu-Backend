using System.Collections;
using Interviu.Data.Context;
using Interviu.Data.IRepositories;
using Interviu.Data.Repositories;

namespace Interviu.Data.UnitOfWork;

public class UnitOfWork: IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;
    private Hashtable _repositories;
    
    private IQuestionRepository  questionRepository;
    private IUserRepository  userRepository;
    private IInterviewRepository interviewRepository;
    private ICVRepository cvRepository;

    public UnitOfWork(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }

    public IUserRepository Users { get; }
    public IInterviewRepository Interviews { get; }
    public IQuestionRepository Questions { get; }
    public ICVRepository CVs { get; }
    public IGenericRepository<T> Repository<T>() where T : class
    {
        if (_repositories == null)
        {
            _repositories = new Hashtable();
        }
        var type=typeof(T).Name;
        if (!_repositories.ContainsKey(type))
        {
            var repositoryType = typeof(GenericRepository<>);
            var repositoryInstance=Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), _dbContext);
            _repositories.Add(type, repositoryInstance);
            
        }
        return (IGenericRepository<T>)_repositories[type];
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }
}