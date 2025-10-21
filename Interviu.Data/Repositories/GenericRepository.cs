using System.Linq.Expressions;
using Interviu.Data.Context;
using Interviu.Data.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Interviu.Data.Repositories;

public class GenericRepository<T>:IGenericRepository<T> where T:class
{
    protected readonly ApplicationDbContext _dbcontext;
    private readonly DbSet<T> _dbset;
    
    public GenericRepository(ApplicationDbContext dbcontext)
    {
        _dbcontext = dbcontext;
        _dbset = _dbcontext.Set<T>();
    }
    
    public async Task<T> GetByIdAsync(Guid id)
    {
        return await _dbcontext.Set<T>().FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbset.ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbset.Where(predicate).ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        await _dbset.AddAsync(entity);
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbset.AddRangeAsync(entities);
    }

    public void RemoveRange(IEnumerable<T> entities)
    {
        _dbset.RemoveRange(entities);
    }

    public void Update(T entity)
    {
        _dbset.Update(entity);
    }

    public void Remove(T entity)
    {
        _dbset.Remove(entity);
    }

    public async Task SaveChangesAsync()
    {
        await _dbcontext.SaveChangesAsync();
    }
}