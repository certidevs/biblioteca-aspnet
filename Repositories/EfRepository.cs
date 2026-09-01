using BibliotecaAspNet.Data;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAspNet.Repositories;

public class EfRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected ApplicationDbContext Context { get; }

    public EfRepository(ApplicationDbContext context)
    {
        Context = context;
    }

    public virtual async Task<TEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<TEntity>().FindAsync(
            new object?[] { id },
            cancellationToken);
    }

    public virtual Task<List<TEntity>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return Context.Set<TEntity>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public virtual Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        return Context.Set<TEntity>().AddAsync(entity, cancellationToken).AsTask();
    }

    public virtual void Update(TEntity entity)
    {
        Context.Set<TEntity>().Update(entity);
    }

    public virtual void Delete(TEntity entity)
    {
        Context.Set<TEntity>().Remove(entity);
    }

    public virtual Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return Context.SaveChangesAsync(cancellationToken);
    }
}
